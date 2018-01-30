// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggingBuilderExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


// ReSharper disable once CheckNamespace


using System;

using LogJam;
using LogJam.Config;
using LogJam.Extensions.Logging;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{

    // ILoggingBuilder doesn't exist before ASP.NET Core 2.0
#if ASPNETCORE2_0

    /// <summary>
    /// Extension methods for configuring a <see cref="LogJamLoggerProvider" /> in an <see cref="ILoggingBuilder" />. Setup
    /// code should normally either call one of these methods within an <code>IServiceCollection.AddLogging(Action&lt;ILoggingBuilder&gt;)</code>
    /// method; or use <code>ILoggerFactory.AddLogJam()</code>.
    /// </summary>
    /// <seealso cref="LogJamLoggerFactoryExtensions"/>
    public static class LogJamWebHostLoggingBuilderExtensions
    {

        /// <summary>
        /// Adds a <see cref="LogJamLoggerProvider"/> to the factory.
        /// </summary>
        /// <param name="loggingBuilder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configureLogManagerAction">An optional configuration delegate that updates the <see cref="LogManagerConfig"/>.</param>
        public static ILoggingBuilder AddLogJam(this ILoggingBuilder loggingBuilder, Action<LogManagerConfig, WebHostBuilderContext> configureLogManagerAction)
        {
            if (configureLogManagerAction != null)
            {
                loggingBuilder.Services.AddLogJam((logManagerConfig, serviceProvider) => configureLogManagerAction(logManagerConfig, serviceProvider.GetService<WebHostBuilderContext>()));
            }
            else
            {
                loggingBuilder.Services.AddLogJam(null);
            }

            loggingBuilder.Services.AddSingleton<ILoggerProvider, LogJamLoggerProvider>();
            return loggingBuilder;
        }

    }

#endif

}
