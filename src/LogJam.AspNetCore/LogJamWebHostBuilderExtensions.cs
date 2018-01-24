// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamWebHostBuilderExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;

using LogJam.Config;

using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Hosting
{

    /// <summary>
    /// <see cref="IWebHostBuilder" /> extension methods for integrating LogJam.
    /// </summary>
    public static class LogJamWebHostBuilderExtensions
    {

#if !WEBHOSTING_1x
    /// <summary>Integrates LogJam into the web host.</summary>
    /// <param name="webHostBuilder">The <see cref="IWebHostBuilder"/> to configure.</param>
    /// <param name="configureLogJam">A configuration delegate, which must configure a <see cref="LogManagerConfig"/> instance.</param>
    /// <returns>The <paramref name="webHostBuilder"/></returns>
    /// <remarks>
    /// This overload accepts a <paramref name="configureLogJam"/> delegate with a <see cref="WebHostBuilderContext"/> argument,
    /// which can be used to access host environment settings. This overload requires ASP.NET Core 2.0 or newer.
    /// </remarks>
        public static IWebHostBuilder UseLogJam(this IWebHostBuilder webHostBuilder, Action<WebHostBuilderContext, LogManagerConfig> configureLogJam)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            webHostBuilder.ConfigureServices((webhostBuilderContext, serviceCollection) =>
            {
            });
            return webHostBuilder;
        }
#endif

        /// <summary>Integrates LogJam into the web host.</summary>
        /// <param name="webHostBuilder">The <see cref="IWebHostBuilder" /> to configure.</param>
        /// <param name="configureLogJam">A configuration delegate, which may configure the <see cref="LogManagerConfig" /> instance. May be <c>null</c>.</param>
        /// <param name="createLogJamLoggerSettings"></param>
        /// <returns>The <paramref name="webHostBuilder" /></returns>
        public static IWebHostBuilder UseLogJam(this IWebHostBuilder webHostBuilder,
                                                Action<LogManagerConfig, IServiceProvider> configureLogJam,
                                                Func<ILogJamLoggerSettings> createLogJamLoggerSettings = null)
        {
            if (webHostBuilder == null)
            {
                throw new ArgumentNullException(nameof(webHostBuilder));
            }

            webHostBuilder.ConfigureServices(serviceCollection =>
                                             {
                                                 serviceCollection.UseLogJam(configureLogJam);
                                                 serviceCollection.UseLogJamLoggerFactory(createLogJamLoggerSettings);
                                             });
            return webHostBuilder;
        }

    }
}
