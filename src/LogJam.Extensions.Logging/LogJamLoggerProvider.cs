// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggerProvider.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Concurrent;

    using global::Microsoft.Extensions.Logging;

    using LogJam.Writer;


    /// <summary>
    /// <see cref="ILoggerProvider" /> for enabling LogJam for <c>Microsoft.Extensions.Logging</c> calls.
    /// </summary>
    //[ProviderAlias("LogJam")]
    public sealed class LogJamLoggerProvider : ILoggerProvider
    {

        private readonly ILogJamLoggerSettings _loggerSettings;
        private readonly LogManager _logManager;
        private readonly bool _disposeLogManager;


        private readonly ConcurrentDictionary<string, LogJamLogger> _loggers =
            new ConcurrentDictionary<string, LogJamLogger>(StringComparer.OrdinalIgnoreCase);

        public LogJamLoggerProvider(ILogJamLoggerSettings loggerSettings = null)
        {
            _loggerSettings = loggerSettings ?? new LogJamLoggerSettings();
            _logManager = new LogManager();
            _disposeLogManager = true;
        }

        public LogJamLoggerProvider(ILogJamLoggerSettings loggerSettings, LogManager logManager, bool disposeLogManager)
        {
            _loggerSettings = loggerSettings ?? new LogJamLoggerSettings();
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _disposeLogManager = disposeLogManager;
        }

        public void Dispose()
        {
            if (_disposeLogManager)
            {
                _logManager.Dispose();
            }
        }

        /// <summary>
        /// Returns the <see cref="LogManager"/> that manages all log writing.
        /// </summary>
        public LogManager LogManager => _logManager;

        /// <summary>
        /// Returns the <see cref="ILogJamLoggerSettings"/> for this provider.
        /// </summary>
        internal ILogJamLoggerSettings Settings => _loggerSettings;

        /// <summary>
        /// Returns a <see cref="ILogger"/> for <paramref name="categoryName"/>.
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLogJamLogger);
        }

        private LogJamLogger CreateLogJamLogger(string categoryName)
        {
            return new LogJamLogger(categoryName, _loggerSettings.Filter, this);
        }

        internal bool TryGetEntryWriter<TEntry>(out IEntryWriter<TEntry> entryWriter)
            where TEntry : ILogEntry
        {
            return _logManager.TryGetEntryWriter<TEntry>(out entryWriter);
        }

    }

}
