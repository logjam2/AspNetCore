﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ILogJamLoggerSettings.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------

using System;

using Microsoft.Extensions.Logging;


namespace LogJam.Extensions.Logging
{

    /// <summary>
    /// Configuration settings for <see cref="LogJamLogger"/>s. May be implemented via configuration files or coded configuration.
    /// </summary>
    public interface ILogJamLoggerSettings
    {

        /// <summary>
        /// If <c>true</c> <see cref="ILogger.BeginScope{TState}"/> creates and stores a scope hierarchy.
        /// </summary>
        bool IncludeScopes { get; }

        /// <summary>
        /// Used to determine which <see cref="Logger{T}"/> calls are logged. If the function returns
        /// <c>true</c>, log entries with the given category and <see cref="LogLevel"/> are logged.
        /// </summary>
        /// <remarks>TODO: Add support for one function per LogWriter.</remarks>
        Func<string, LogLevel, bool> Filter { get; }

    }

}
