﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LoggerEndScopeEntry.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Extensions.Logging.Entries
{

    using System;

    /// <summary>
    /// A log entry type that is written when a scope ends.
    /// </summary>
    public readonly struct LoggerEndScopeEntry : ILogEntry
    {

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        public readonly string CategoryName;
        public readonly DateTime TimestampUtc;
        public readonly string StateString;

        public LoggerEndScopeEntry(string categoryName, string stateString)
        {
            TimestampUtc = DateTime.UtcNow;
            CategoryName = categoryName ?? throw new ArgumentNullException(nameof(categoryName));
            StateString = stateString;
        }

    }

}
