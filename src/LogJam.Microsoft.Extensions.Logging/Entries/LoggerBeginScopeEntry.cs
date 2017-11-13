// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerBeginScopeEntry.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Entries
{

    using System;

    /// <summary>
    /// A log entry type that is written when a scope begins.
    /// </summary>
    public struct LoggerBeginScopeEntry<TState> : ILogEntry
    {

        public readonly DateTime Timestamp;
        public readonly TState State;

        public LoggerBeginScopeEntry(TState state)
        {
            Timestamp = DateTime.UtcNow;
            State = state;
        }

    }

}
