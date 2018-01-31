// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericLoggerEndScopeEntryFormatter.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;

using LogJam.Extensions.Logging.Entries;
using LogJam.Writer.Text;

namespace LogJam.Extensions.Logging.Format
{

    /// <summary>
    /// Fallback formatter for writing <see cref="LoggerEndScopeEntry" /> entries to text logs. Can be enabled for log entries
    /// when a <c>TState</c>-specific formatter is not implemented.
    /// </summary>
    public sealed class GenericLoggerEndScopeEntryFormatter : EntryFormatter<LoggerEndScopeEntry>
    {

        /// <summary>
        /// Initializes a new <see cref="GenericLoggerEndScopeEntryFormatter" />.
        /// </summary>
        /// <param name="includeTypeNames">
        /// Set to <c>true</c> to include type names in state object output. Set to <c>false</c> to exclude type names. Defaults to <c>true</c>.
        /// </param>
        /// <param name="maxDepth">The max depth to walk when formatting objects. Default is 2.</param>
        public GenericLoggerEndScopeEntryFormatter()
        {
            IncludeTimestamp = true;
        }

        /// <summary>
        /// <c>true</c> to include the Date when formatting <see cref="LoggerEntry" />s. Default is <c>false</c>.
        /// </summary>
        public bool IncludeDate { get; set; }

        /// <summary>
        /// <c>true</c> to include the TimestampUtc when formatting <see cref="LoggerEntry" />s. Default is <c>true</c>.
        /// </summary>
        public bool IncludeTimestamp { get; set; }

        /// <inheritdoc />
        public override void Format(ref LoggerEndScopeEntry entry, FormatWriter formatWriter)
        {
            formatWriter.IndentLevel--;

            formatWriter.BeginEntry();

            if (IncludeDate)
            {
                formatWriter.WriteDate(entry.TimestampUtc, ColorCategory.Debug);
            }

            if (IncludeTimestamp)
            {
                formatWriter.WriteTimestamp(entry.TimestampUtc, ColorCategory.Detail);
            }

            formatWriter.WriteField("End", ColorCategory.Detail, 7);
            formatWriter.WriteAbbreviatedTypeName(entry.CategoryName, ColorCategory.Debug, 36);

            formatWriter.WriteField(entry.StateString, ColorCategory.Detail);

            formatWriter.EndEntry();
        }

    }

}
