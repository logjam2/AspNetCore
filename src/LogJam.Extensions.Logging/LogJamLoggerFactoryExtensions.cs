// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggerFactoryExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace Microsoft.Extensions.Logging
{
    using System;

    using Microsoft.Extensions.Logging;

    using LogJam;
    using LogJam.Extensions.Logging;


    /// <summary>
    /// Extension methods for adding a <see cref="LogJamLoggerProvider"/> to a <see cref="ILoggerFactory"/>.
    /// </summary>
    public static class LogJamLoggerFactoryExtensions
    {

        /// <summary>
        /// Adds a <see cref="LogJamLoggerProvider"/> that forwards all calls to <paramref name="logManager"/>.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory" /> to use.</param>
        /// <param name="settings">Settings to use for <see cref="ILogger"/> calls.</param>
        /// <param name="logManager">A LogJam <see cref="LogManager"/> that is configured for desired logging.</param>
        /// <param name="disposeLogManager"><c>true</c> to dispose <paramref name="logManager"/> when <paramref name="factory"/> is disposed.</param>
        public static ILoggerFactory AddLogJam(this ILoggerFactory factory, ILogJamLoggerSettings settings, LogManager logManager, bool disposeLogManager)
        {
            if (logManager == null)
            {
                throw new ArgumentNullException(nameof(logManager));
            }

            factory.AddProvider(new LogJamLoggerProvider(settings, logManager, disposeLogManager));
            return factory;
        }

        ///// <summary>
        ///// </summary>
        ///// <param name="factory">The <see cref="ILoggerFactory" /> to use.</param>
        ///// <param name="configuration">The <see cref="IConfiguration" /> to use for <see cref="IConsoleLoggerSettings" />.</param>
        ///// <returns></returns>
        //public static ILoggerFactory AddLogJam(this ILoggerFactory factory, IConfiguration configuration, LogManager logManager, bool disposeLogManager)
        //{
        //    var settings = new ConfigurationLogJamLoggerSettings(configuration);
        //    return factory.AddConsole(settings);
        //}

    }
}
