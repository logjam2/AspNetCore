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
        public static IServiceCollection UseLogJam(this IServiceCollection serviceCollection, Action<LogManagerConfig, IServiceProvider> configureLogManager)
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
        public static IServiceCollection UseLogJamTracing(this IServiceCollection serviceCollection, Action<TraceManagerConfig, IServiceProvider> configureTraceManager)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            if (configureTraceManager != null)
            {
                serviceCollection.ConfigureTraceManager(configureTraceManager);
            }

            serviceCollection.TryAddSingleton<TraceManagerConfig>(new TraceManagerConfig());
            serviceCollection.TryAddSingleton<ITracerFactory>(serviceProvider =>
            {
                var traceManagerConfig = serviceProvider.GetRequiredService<TraceManagerConfig>();
                var configurationActions = serviceProvider.GetServices<Action<TraceManagerConfig, IServiceProvider>>();
                foreach (var configurationAction in configurationActions)
                {
                    configurationAction(traceManagerConfig, serviceProvider);
                }

                return new TraceManager(traceManagerConfig);
            });

            // Setup default text formatters
            serviceCollection.UseLogJam((logManagerConfig, serviceProvider) => logManagerConfig.Writers.FormatAll(new DefaultTraceFormatter()));

            return serviceCollection;
        }

        /// <summary>Uses LogJam for calls to <c>Microsoft.Extensions.Logging</c> APIs. Replaces calls to <c>IServiceCollection.AddLogging()</c>.</summary>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> to configure.</param>
        /// <param name="createLogJamLoggerSettings">Creates <see cref="LogJamLoggerSettings"/>. May be <c>null</c>.</param>
        /// <returns>The <paramref name="serviceCollection"/>.</returns>
        /// <remarks>
        /// This method can only be called once per serviceCollection.
        /// </remarks>
        public static IServiceCollection UseLogJamLoggerFactory(this IServiceCollection serviceCollection, Func<ILogJamLoggerSettings> createLogJamLoggerSettings)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.TryAddSingleton<ILogJamLoggerSettings>(serviceProvider =>
                                                                  {
                                                                      if (createLogJamLoggerSettings == null)
                                                                      {
                                                                          return new LogJamLoggerSettings
                                                                                 {
                                                                                     Filter = (category, level) => level >= LogLevel.Information
                                                                                 };
                                                                      }
                                                                      else
                                                                      {
                                                                          return createLogJamLoggerSettings();
                                                                      }
                                                                  });

            // Add/replace ILoggerFactory with one that only uses LogJam, to avoid duplicate logging. If duplicate logging paths are desired, either
            // don't use this method, or explicitly replace the <c>ILoggerFactory</c> after calling this.
            serviceCollection.Replace(new ServiceDescriptor(typeof(ILoggerFactory),
                                                            serviceProvider =>
                                                            {
                                                                // TODO: Consider using a more performant implementation than LoggerFactory for this use case
                                                                var loggerFactory = new LoggerFactory();
                                                                loggerFactory.AddLogJam(serviceProvider.GetService<ILogJamLoggerSettings>(),
                                                                                        serviceProvider.GetService<LogManager>(),
                                                                                        false);
                                                                return loggerFactory;
                                                            }, ServiceLifetime.Singleton));

            // Do this so that IServiceCollection.AddLogging() isn't needed (but should be ok if both are called).
            serviceCollection.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));

            // Setup default text formatters
            serviceCollection.UseLogJam((logManagerConfig, serviceProvider) => AddDefaultLoggerTextFormatters(logManagerConfig, serviceProvider.GetService<ILogJamLoggerSettings>()) );

            return serviceCollection;
        }

        /// <summary>
        /// Enables the default text formatters for entry types that are logged when <see cref="ILogger"/> instances are used for logging.
        /// </summary>
        /// <param name="logManagerConfig"></param>
        /// <param name="loggerSettings"></param>
        internal static void AddDefaultLoggerTextFormatters(LogManagerConfig logManagerConfig, ILogJamLoggerSettings loggerSettings)
        {
            logManagerConfig.Writers.FormatAll<LoggerEntry>(new DefaultLoggerEntryFormatter());
            if (loggerSettings.IncludeScopes)
            {
                logManagerConfig.Writers.FormatAll(new GenericLoggerBeginScopeEntryFormatter());
                logManagerConfig.Writers.FormatAll(new GenericLoggerEndScopeEntryFormatter());
            }
        }

    }

}
