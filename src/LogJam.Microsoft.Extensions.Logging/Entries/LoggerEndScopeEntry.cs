// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerEndScopeEntry.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Entries
{

    using System;

    /// <summary>
    /// A log entry type that is written when a scope ends.
    /// </summary>
    public readonly struct LoggerEndScopeEntry<TState> : ILogEntry
    {

        public readonly string CategoryName;
        public readonly DateTime TimestampUtc;
        public readonly TState State;

        public LoggerEndScopeEntry(string categoryName, TState state)
        {
            TimestampUtc = DateTime.UtcNow;
            CategoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            State = state;
        }

    }

}
