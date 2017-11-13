// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerEntry.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Entries
{
    using System;

    using global::Microsoft.Extensions.Logging;


    /// <summary>
    /// An entry that is logged when <see cref="ILogger.Log{TState}"/> is called.
    /// </summary>
    public struct LoggerEntry<TState> : ILogEntry
    {

        public readonly string CategoryName;
        public readonly DateTime Timestamp;
        public readonly LogLevel LogLevel;
        public readonly EventId EventId;
        public readonly TState State;
        public readonly Exception Exception;
        public readonly Func<TState, Exception, string> DefaultFormatter;

        public LoggerEntry(string categoryName, LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            CategoryName = categoryName;
            Timestamp = DateTime.UtcNow;
            LogLevel = logLevel;
            EventId = eventId;
            State = state;
            Exception = exception;
            DefaultFormatter = formatter;
        }

    }

}
