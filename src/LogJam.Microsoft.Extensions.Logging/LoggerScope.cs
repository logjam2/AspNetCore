// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerScope.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;

    using LogJam.Microsoft.Extensions.Logging.Entries;
    using LogJam.Writer;


    /// <summary>
    /// A logger scope - for example, an HTTP request. A scope has a begin and an end, and
    /// other log entries can be created within the scope.
    /// </summary>
    internal sealed class LoggerScope<TState> : IDisposable
    {
        // TODO: Hold scope parent

        private readonly string _loggerName;
        private readonly TState _state;
        private readonly LogJamLoggerProvider _provider;
        private DateTime _beginTimestamp, _endTimestamp;

        public LoggerScope(string loggerName, TState state, LogJamLoggerProvider provider)
        {
            _loggerName = loggerName;
            _state = state;
            _provider = provider;
        }

        internal void WriteBeginScope()
        {
            if (_provider.TryGetEntryWriter(out IEntryWriter<LoggerBeginScopeEntry<TState>> entryWriter))
            {
                if (entryWriter.IsEnabled)
                {
                    var entry = new LoggerBeginScopeEntry<TState>(_loggerName, _state);
                    _beginTimestamp = entry.TimestampUtc;
                    entryWriter.Write(ref entry);
                }
            }
            // Use the fallback entryWriter if none is defined for TState
            else if (_provider.TryGetEntryWriter(out IEntryWriter<LoggerBeginScopeEntry<object>> entryWriter2))
            {
                if (entryWriter2.IsEnabled)
                {
                    var entry = new LoggerBeginScopeEntry<object>(_loggerName, _state);
                    _beginTimestamp = entry.TimestampUtc;
                    entryWriter2.Write(ref entry);
                }
            }
        }

        internal void WriteEndScope()
        {
            if (_provider.TryGetEntryWriter(out IEntryWriter<LoggerEndScopeEntry<TState>> entryWriter))
            {
                if (entryWriter.IsEnabled)
                {
                    var entry = new LoggerEndScopeEntry<TState>(_loggerName, _state);
                    _beginTimestamp = entry.TimestampUtc;
                    entryWriter.Write(ref entry);
                }
            }
            // Use the fallback entryWriter if none is defined for TState
            else if (_provider.TryGetEntryWriter(out IEntryWriter<LoggerEndScopeEntry<object>> entryWriter2))
            {
                if (entryWriter2.IsEnabled)
                {
                    var entry = new LoggerEndScopeEntry<object>(_loggerName, _state);
                    _beginTimestamp = entry.TimestampUtc;
                    entryWriter2.Write(ref entry);
                }
            }
        }

        public void Dispose()
        {
            // Using _endTimestamp as a bool _isDisposed field.
            if (_endTimestamp == default(DateTime))
            {
                WriteEndScope();
            }
        }

    }

}
