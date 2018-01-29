// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggerFactoryExtensions.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;

using LogJam;
using LogJam.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.Logging
{

    /// <summary>
    /// Extension methods for adding a <see cref="LogJamLoggerProvider" /> to a <see cref="ILoggerFactory" />.
    /// </summary>
    public static class LogJamLoggerFactoryExtensions
    {

        /// <summary>
        /// Adds a <see cref="LogJamLoggerProvider" /> that forwards all calls to <paramref name="logManager" />.
        /// </summary>
        /// <param name="factory">The <see cref="ILoggerFactory" /> to use.</param>
        /// <param name="logManager">A LogJam <see cref="LogManager" /> that is configured for desired logging.</param>
        /// <param name="disposeLogManager"><c>true</c> to dispose <paramref name="logManager" /> when <paramref name="factory" /> is disposed.</param>
        /// <param name="filter">A function used to filter events based on the log level. If <c>null</c> is passed in, the default of <see cref="LogJamLogger.LogInformation"/> is used.</param>
        public static ILoggerFactory AddLogJam(this ILoggerFactory factory, LogManager logManager, bool disposeLogManager, Func<string, LogLevel, bool> filter)
        {
            if (logManager == null)
            {
                throw new ArgumentNullException(nameof(logManager));
            }

            factory.AddProvider(new LogJamLoggerProvider(logManager, disposeLogManager, filter ?? LogJamLogger.LogInformation));
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
