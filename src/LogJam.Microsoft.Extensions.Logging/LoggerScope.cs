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

        private readonly TState _state;
        private readonly LogJamLoggerProvider _provider;
        private DateTime _beginTimestamp, _endTimestamp;

        public LoggerScope(TState state, LogJamLoggerProvider provider)
        {
            _state = state;
            _provider = provider;
        }

        internal void WriteBeginScope()
        {
            IEntryWriter<LoggerBeginScopeEntry<TState>> entryWriter = _provider.GetEntryWriter<LoggerBeginScopeEntry<TState>>();
            if (entryWriter.IsEnabled)
            {
                var entry = new LoggerBeginScopeEntry<TState>(_state);
                _beginTimestamp = entry.Timestamp;
                entryWriter.Write(ref entry);
            }
        }

        internal void WriteEndScope()
        {
            IEntryWriter<LoggerEndScopeEntry<TState>> entryWriter = _provider.GetEntryWriter<LoggerEndScopeEntry<TState>>();
            if (entryWriter.IsEnabled)
            {
                var entry = new LoggerEndScopeEntry<TState>(_state);
                _endTimestamp = entry.Timestamp;
                entryWriter.Write(ref entry);
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
