// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamWebHostBuilderExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;

using LogJam.Config;
using LogJam.Trace;
using LogJam.Trace.Config;
using LogJam.Trace.Switches;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{

    /// <summary>
    /// <see cref="IWebHostBuilder" /> extension methods for integrating LogJam.
    /// </summary>
    public static class LogJamWebHostBuilderExtensions
    {

#if ASPNETCORE2_0
        // WebHostBuilderContext isn't defined before ASP.NET Core 2.0

        /// <summary>Integrates LogJam into the web host.</summary>
        /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configureLogManager">A configuration delegate, which may configure a <see cref="LogManagerConfig"/> instance.</param>
        /// <returns>The <paramref name="webHostBuilder"/></returns>
        /// <remarks>
        /// This overload accepts a <paramref name="configureLogManager"/> delegate with a <see cref="WebHostBuilderContext"/> argument,
        /// which can be used to access host environment settings. This overload requires ASP.NET Core 2.0 or newer.
        /// </remarks>
        public static IWebHostBuilder UseLogJam(this IWebHostBuilder webHostBuilder, Action<LogManagerConfig, WebHostBuilderContext> configureLogManager)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            webHostBuilder.ConfigureServices((webhostBuilderContext, serviceCollection) =>
                                             {
                                                 serviceCollection.AddLogJam((logManagerConfig, serviceProvider) => configureLogManager(logManagerConfig, webhostBuilderContext));
                                                 serviceCollection.AddLogJamLoggerProvider();
                                             });
            return webHostBuilder;
        }

        /// <summary>Configures the LogJam <see cref="LogManagerConfig"/>.</summary>
        /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
        /// <param name="configureLogManager">A configuration delegate, which configures a <see cref="LogManagerConfig"/> instance.</param>
        /// <returns>The <paramref name="webHostBuilder"/></returns>
        /// <remarks>
        /// This method accepts a <paramref name="configureLogManager"/> delegate with a <see cref="WebHostBuilderContext"/> argument,
        /// which can be used to access host environment settings. This overload requires ASP.NET Core 2.0 or newer.
        /// </remarks>
        public static IWebHostBuilder ConfigureLogManager(this IWebHostBuilder webHostBuilder, Action<LogManagerConfig, WebHostBuilderContext> configureLogManager)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }
            if (configureLogManager == null)
            {
                throw new ArgumentNullException(nameof(configureLogManager));
            }

            webHostBuilder.ConfigureServices((webhostBuilderContext, serviceCollection) =>
                                             {
                                                 serviceCollection.ConfigureLogManager((logManagerConfig, serviceProvider) => configureLogManager(logManagerConfig, webhostBuilderContext));
                                             });
            return webHostBuilder;
        }
#endif

        /// <summary>Integrates LogJam into the web host.</summary>
        /// <param name="webHostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogJam">A configuration delegate, which may configure the <see cref="LogManagerConfig" /> instance. May be <c>null</c>.</param>
        /// <returns>The <paramref name="webHostBuilder" /></returns>
        public static IWebHostBuilder UseLogJam(this IWebHostBuilder webHostBuilder,
                                                Action<LogManagerConfig, IServiceProvider> configureLogJam)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            webHostBuilder.ConfigureServices(serviceCollection =>
                                             {
                                                 serviceCollection.AddLogJam(configureLogJam);
                                                 serviceCollection.AddLogJamLoggerProvider();
                                             });
            return webHostBuilder;
        }

        public static IWebHostBuilder UseLogJamTracing(this IWebHostBuilder webHostBuilder, SwitchSet switchSet = null)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            webHostBuilder.ConfigureServices((serviceCollection) =>
                                             {
                                                 serviceCollection.AddLogJamTracing(switchSet);
                                             });
            return webHostBuilder;
        }
    }
}
