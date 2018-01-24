// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggerSettings.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;

    using global::Microsoft.Extensions.Logging;


    /// <summary>
    /// Coded configuration settings for <see cref="LogJamLogger"/>s.
    /// </summary>
    public class LogJamLoggerSettings : ILogJamLoggerSettings
    {

        /// <summary>
        /// If <c>true</c> <see cref="ILogger.BeginScope{TState}"/> stores a scope hierarchy.
        /// </summary>
        public bool IncludeScopes { get; set; } = false;

        /// <summary>
        /// Used to determine which <see cref="ILogger"/> calls are logged. If the function returns
        /// <c>true</c>, log entries with the given category and <see cref="LogLevel"/> are logged.
        /// </summary>
        /// <remarks>TODO: Add support for one function per LogWriter.</remarks>
        public Func<string, LogLevel, bool> Filter { get; set; }

    }

}
