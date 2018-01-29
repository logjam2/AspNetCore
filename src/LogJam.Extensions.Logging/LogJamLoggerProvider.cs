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
#if ASPNETCORE2_0
    [ProviderAlias("LogJam")]
#endif
    public sealed class LogJamLoggerProvider : ILoggerProvider
    {

        private readonly LogManager _logManager;
        private readonly bool _disposeLogManager;
        private readonly Func<string, LogLevel, bool> _filter;


        private readonly ConcurrentDictionary<string, LogJamLogger> _loggers =
            new ConcurrentDictionary<string, LogJamLogger>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new <see cref="LogJamLoggerProvider"/>, using the specified <paramref name="logManager"/>.
        /// </summary>
        /// <param name="logManager">A <see cref="LogManager"/>, which manages which log writers are used.</param>
        public LogJamLoggerProvider(LogManager logManager)
        {
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _disposeLogManager = false;
            _filter = null;
        }

        /// <summary>
        /// Initializes a new <see cref="LogJamLoggerProvider"/>, using the specified <paramref name="logManager"/> and filter function.
        /// </summary>
        /// <param name="logManager">A <see cref="LogManager"/>, which manages which log writers are used.</param>
        /// <param name="disposeLogManager"><c>true</c> to shutdown + dispose <paramref name="logManager"/> when this provider is disposed.</param>
        /// <param name="filter">A function used to filter events based on the log level. If <c>null</c> is passed in, the default of <see cref="LogJamLogger.LogInformation"/> is used.</param>
        public LogJamLoggerProvider(LogManager logManager, bool disposeLogManager, Func<string, LogLevel, bool> filter)
        {
            _logManager = logManager ?? throw new ArgumentNullException(nameof(logManager));
            _disposeLogManager = disposeLogManager;
            _filter = filter ?? LogJamLogger.LogInformation;
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
            return new LogJamLogger(categoryName, _filter, this);
        }

        internal bool TryGetEntryWriter<TEntry>(out IEntryWriter<TEntry> entryWriter)
            where TEntry : ILogEntry
        {
            return _logManager.TryGetEntryWriter<TEntry>(out entryWriter);
        }

    }

}
