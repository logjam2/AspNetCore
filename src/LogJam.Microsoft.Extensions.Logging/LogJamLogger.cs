// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LogJamLogger.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;

    using global::Microsoft.Extensions.Logging;

    using LogJam.Microsoft.Extensions.Logging.Entries;
    using LogJam.Writer;


    /// <summary>
    /// An <see cref="ILogger"/> that forwards log writing to a <see cref="LogManager"/>.
    /// </summary>
    public sealed class LogJamLogger : ILogger
    {
        /// <summary>
        /// Filter function to log everything.
        /// </summary>
        public static Func<string, LogLevel, bool> LogEverything = (categoryName, logLevel) => true;
        /// <summary>
        /// Filter function to log Information and higher.
        /// </summary>
        public static Func<string, LogLevel, bool> LogInfo = (categoryName, logLevel) => logLevel >= LogLevel.Information;
        /// <summary>
        /// Filter function to log Information and higher.
        /// </summary>
        public static Func<string, LogLevel, bool> LogNothing = (categoryName, logLevel) => false;

        /// <summary>
        /// The parent <see cref="LogJamLoggerProvider"/>.
        /// </summary>
        private readonly LogJamLoggerProvider _provider;

        private Func<string, LogLevel, bool> _filter;

        internal LogJamLogger(string name, Func<string, LogLevel, bool> filter, LogJamLoggerProvider provider)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Filter = filter ?? LogInfo;
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        }

        /// <summary>
        /// Name for this <see cref="ILogger"/>.
        /// </summary>
        public string Name { get; }

        // TODO: Replace with levels per LogWriter
        /// <summary>
        /// Filter function for whether any given <see cref="LogLevel"/> should be logged.
        /// </summary>
        public Func<string, LogLevel, bool> Filter
        {
            get => _filter;
            set => _filter = value ?? throw new ArgumentNullException(nameof(value));
        }

#region Implementation of ILogger

        /// <summary>
        /// If this log entry isn't filtered, forwards a <see cref="LoggerEntry"/> to LogJam writers.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="state"></param>
        /// <param name="exception"></param>
        /// <param name="formatter"></param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (! IsEnabled(logLevel))
            {
                return;
            }

            if (_provider.TryGetEntryWriter(out IEntryWriter<LoggerEntry> entryWriter)
                && entryWriter.IsEnabled)
            {
                var entry = new LoggerEntry(Name, logLevel, eventId, state, exception, (obj, excp) => formatter((TState) obj, excp));
                entryWriter.Write(ref entry);
            }
        }

        /// <summary>
        /// Returns <c>true</c> if <paramref name="logLevel"/> will be logged.
        /// </summary>
        /// <param name="logLevel"></param>
        /// <returns></returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return _filter(Name, logLevel);
        }

        /// <summary>
        /// Results in 2 entries being logged: A <see cref="LoggerBeginScopeEntry{TState}"/> is logged immediately, and
        /// a <see cref="LoggerEndScopeEntry{TState}"/> is logged when the scope ends.
        /// </summary>
        /// <typeparam name="TState"></typeparam>
        /// <param name="state"></param>
        /// <returns></returns>
        public IDisposable BeginScope<TState>(TState state)
        {
            if ((! _provider.Config.IncludeScopes)
                || state.Equals(default(TState)))
            {
                return null;
            }

            var loggerScope = new LoggerScope<TState>(Name, state, _provider);
            loggerScope.WriteBeginScope();
            return loggerScope;
        }

#endregion

    }

}
