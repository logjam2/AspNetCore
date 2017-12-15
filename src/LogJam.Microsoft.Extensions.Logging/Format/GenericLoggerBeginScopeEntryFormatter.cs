// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericLoggerBeginScopeEntryFormatter.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Format
{
    using LogJam.Microsoft.Extensions.Logging.Entries;
    using LogJam.Writer.Text;


    /// <summary>
    /// Fallback formatter for writing <see cref="LoggerBeginScopeEntry{TState}"/> entries to text logs. Can be enabled to log entries
    /// when a <c>TState</c>-specific formatter is not implemented.
    /// </summary>
    public class GenericLoggerBeginScopeEntryFormatter : EntryFormatter<LoggerBeginScopeEntry<object>>
    {

        public override void Format(ref LoggerBeginScopeEntry<object> entry, FormatWriter formatWriter)
        {
            throw new System.NotImplementedException();
        }

    }

}
