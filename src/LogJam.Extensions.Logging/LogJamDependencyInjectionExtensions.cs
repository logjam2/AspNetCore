// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamDependencyInjectionExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------

using System;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using LogJam;
using LogJam.Config;
using LogJam.Extensions.Logging;
using LogJam.Extensions.Logging.Entries;
using LogJam.Extensions.Logging.Format;
using LogJam.Trace;
using LogJam.Trace.Config;
using LogJam.Trace.Format;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{

    /// <summary>
    /// Extension methods to integrate LogJam into <c>Microsoft.Extensions.DependencyInjection</c>.
    /// </summary>
    public static class LogJamDependencyInjectionExtensions
    {

        /// <summary>Adds a configuration delegate for configuring the <see cref="LogManagerConfig"/>.</summary>
        /// <param name="serviceCollection">A <see cref="IServiceCollection"/>.</param>
        /// <param name="configureDelegate">A configuration delegate, which will be called to configure the <see cref="LogManagerConfig"/> instance.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can be called multiple times. All <paramref name="configureDelegate"/> methods are called sequentially.
        /// </remarks>
        public static IServiceCollection ConfigureLogManager(this IServiceCollection serviceCollection, Action<LogManagerConfig, IServiceProvider> configureDelegate)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            serviceCollection.AddSingleton(configureDelegate);
            return serviceCollection;
        }


        /// <summary>Adds a configuration delegate for configuring the <see cref="TraceManagerConfig"/>.</summary>
        /// <param name="serviceCollection">A <see cref="IServiceCollection"/>.</param>
        /// <param name="configureDelegate">A configuration delegate, which will be called to configure the <see cref="TraceManagerConfig"/> instance.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can be called multiple times. All <paramref name="configureDelegate"/> methods are called sequentially.
        /// </remarks>
        public static IServiceCollection ConfigureTraceManager(this IServiceCollection serviceCollection, Action<TraceManagerConfig, IServiceProvider> configureDelegate)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }

            serviceCollection.AddSingleton(configureDelegate);
            return serviceCollection;
        }

        /// <summary>Registers a LogJam <see cref="LogManager"/> singleton in <paramref name="serviceCollection"/>. It is configured via <paramref name="configureLogManager"/>.</summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureLogManager">A configuration delegate, which may configure the <see cref="LogManagerConfig"/> instance. May be <c>null</c>.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can be called multiple times. All <paramref name="configureLogManager"/> methods are called sequentially.
        /// </remarks>
        public static IServiceCollection AddLogJam(this IServiceCollection serviceCollection, Action<LogManagerConfig, IServiceProvider> configureLogManager)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (configureLogManager != null)
            {
                serviceCollection.ConfigureLogManager(configureLogManager);
            }

            serviceCollection.TryAddSingleton<LogManagerConfig>(new LogManagerConfig());
            serviceCollection.TryAddSingleton<LogManager>(serviceProvider =>
                                                          {
                                                              var logManagerConfig = serviceProvider.GetRequiredService<LogManagerConfig>();
                                                              var configurationActions = serviceProvider.GetServices<Action<LogManagerConfig, IServiceProvider>> ();
                                                              foreach (var configurationAction in configurationActions)
                                                              {
                                                                  configurationAction(logManagerConfig, serviceProvider);
                                                              }

                                                              return new LogManager(logManagerConfig);
                                                          });

            return serviceCollection;
        }


        /// <summary>Registers a LogJam <see cref="ITracerFactory"/> singleton in <paramref name="serviceCollection"/>. It is configured via <paramref name="configureTraceManager"/>.</summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="configureTraceManager">A configuration delegate, which may configure the <see cref="TraceManagerConfig"/> instance. May be <c>null</c>.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can be called multiple times. All <paramref name="configureTraceManager"/> methods are called sequentially.
        /// </remarks>
        public static IServiceCollection AddLogJamTracing(this IServiceCollection serviceCollection, Action<TraceManagerConfig, IServiceProvider> configureTraceManager)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (configureTraceManager != null)
            {
                serviceCollection.ConfigureTraceManager(configureTraceManager);
            }

            serviceCollection.TryAddSingleton<TraceManagerConfig>(serviceProvider => new TraceManagerConfig(serviceProvider.GetRequiredService<LogManagerConfig>()));
            serviceCollection.TryAddSingleton<ITracerFactory>(serviceProvider =>
            {
                var traceManagerConfig = serviceProvider.GetRequiredService<TraceManagerConfig>();
                var configurationActions = serviceProvider.GetServices<Action<TraceManagerConfig, IServiceProvider>>();
                foreach (var configurationAction in configurationActions)
                {
                    configurationAction(traceManagerConfig, serviceProvider);
                }

                return new TraceManager(serviceProvider.GetRequiredService<LogManager>(), traceManagerConfig);
            });

            // Setup default text formatters
            serviceCollection.AddLogJam((logManagerConfig, serviceProvider) => logManagerConfig.FormatAllTextLogWriters(new DefaultTraceFormatter()));

            return serviceCollection;
        }

        /// <summary>Registers a LogJam <see cref="ITracerFactory"/> singleton in <paramref name="serviceCollection"/>. Tracing  It is configured via <paramref name="configureTraceManager"/>.</summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="switchSet">A <see cref="SwitchSet"/>, which controls which trace levels are logged. May be <c>null</c>, which results in <see cref="TraceLevel.Info"/> and higher being logged.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        public static IServiceCollection AddLogJamTracing(this IServiceCollection serviceCollection, SwitchSet switchSet = null)
        {
            return serviceCollection.AddLogJamTracing((traceManagerConfig, s) => traceManagerConfig.TraceToAllLogWriters(switchSet));
        }

        /// <summary>Adds LogJam for calls to <c>Microsoft.Extensions.Logging</c> APIs. <c>IServiceCollection.AddLogging()</c> does not need to called separately.</summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to configure.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can only be called once per serviceCollection.
        /// </remarks>
        public static IServiceCollection AddLogJamLoggerProvider(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddLogging();

#if ASPNETCORE2_0
            // Just registering the ILoggerProvider works in ASP.NET Core 2.0
            serviceCollection.AddSingleton<ILoggerProvider, LogJamLoggerProvider>();
#else
            // For ASP.NET 1.1, things have to be manually wired.
            // The app can register IFilterLoggerSettings to set the filter settings 
            serviceCollection.Replace(ServiceDescriptor.Singleton<ILoggerFactory>(serviceProvider => new LoggerFactory()
                                                                                                     .WithFilter(serviceProvider.GetService<IFilterLoggerSettings>() ?? new FilterLoggerSettings())
                                                                                                     .AddLogJam(serviceProvider.GetRequiredService<LogManager>(), false, LogJamLogger.LogEverything)));
#endif

            // Setup default text formatters
            serviceCollection.AddLogJam((logManagerConfig, serviceProvider) => AddDefaultLoggerTextFormatters(logManagerConfig));

            return serviceCollection;
        }

        /// <summary>
        /// Enables the default text formatters for entry types that are logged when <see cref="ILogger"/> instances are used for logging.
        /// </summary>
        /// <param name="logManagerConfig"></param>
        internal static void AddDefaultLoggerTextFormatters(LogManagerConfig logManagerConfig)
        {
            logManagerConfig.FormatAllTextLogWriters(new DefaultLoggerEntryFormatter());
            logManagerConfig.FormatAllTextLogWriters(new GenericLoggerBeginScopeEntryFormatter());
            logManagerConfig.FormatAllTextLogWriters(new GenericLoggerEndScopeEntryFormatter());
        }

    }

}
