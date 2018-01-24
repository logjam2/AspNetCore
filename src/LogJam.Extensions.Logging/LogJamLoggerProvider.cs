// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLoggerProvider.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

using LogJam.Writer;


namespace LogJam.Extensions.Logging
{

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

        /// <summary>
        /// Initializes a new <see cref="LogJamLoggerProvider"/>, using the specified <paramref name="logManager"/>.
        /// </summary>
        /// <param name="loggerSettings"><see cref="ILogJamLoggerSettings"/> that specify what is logged.</param>
        /// <param name="logManager">A <see cref="LogManager"/>, which manages which log writers are used.</param>
        /// <param name="disposeLogManager">If <c>true</c>, <paramref name="logManager"/> is disposed when this <see cref="LogJamLoggerProvider"/> is <c>Dispose()</c>ed.</param>
        public LogJamLoggerProvider(ILogJamLoggerSettings loggerSettings, LogManager logManager, bool disposeLogManager)
        {
            _loggerSettings = loggerSettings ?? new LogJamLoggerSettings();
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _disposeLogManager = disposeLogManager;
        }

        /// <summary>
        /// Disposes the <see cref="LogJamLoggerProvider"/>.
        /// </summary>
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
