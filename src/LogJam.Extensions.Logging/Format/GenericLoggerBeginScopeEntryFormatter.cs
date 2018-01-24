// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GenericLoggerBeginScopeEntryFormatter.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Extensions.Logging.Format
{
    using System;

    using LogJam.Extensions.Logging.Entries;
    using LogJam.Writer.Text;


    /// <summary>
    /// Fallback formatter for writing <see cref="LoggerBeginScopeEntry{TState}"/> entries to text logs. Can be enabled to log entries
    /// when a <c>TState</c>-specific formatter is not implemented.
    /// </summary>
    public sealed class GenericLoggerBeginScopeEntryFormatter : EntryFormatter<LoggerBeginScopeEntry<object>>
    {

        private readonly ReflectionFormatter _reflectionFormatter;

        /// <summary>
        /// Initializes a new <see cref="GenericLoggerBeginScopeEntryFormatter"/>.
        /// </summary>
        /// <param name="includeTypeNames">Set to <c>true</c> to include type names in state object output. Set to <c>false</c> to exclude type names. Defaults to <c>true</c>.</param>
        public GenericLoggerBeginScopeEntryFormatter(bool includeTypeNames = true)
        : this(new ReflectionFormatter()
               {
                   IncludeTypeNames = includeTypeNames
               })
        {}

        internal GenericLoggerBeginScopeEntryFormatter(ReflectionFormatter reflectionFormatter)
        {
            IncludeTimestamp = true;
            _reflectionFormatter = reflectionFormatter ?? throw new ArgumentNullException(nameof(reflectionFormatter));
        }

        /// <summary>
        /// <c>true</c> to include the Date when formatting <see cref="LoggerEntry" />s. Default is <c>false</c>.
        /// </summary>
        public bool IncludeDate { get; set; }

        /// <summary>
        /// <c>true</c> to include the TimestampUtc when formatting <see cref="LoggerEntry" />s. Default is <c>true</c>.
        /// </summary>
        public bool IncludeTimestamp { get; set; }

        public override void Format(ref LoggerBeginScopeEntry<object> entry, FormatWriter formatWriter)
        {
            formatWriter.BeginEntry();

            if (IncludeDate)
            {
                formatWriter.WriteDate(entry.TimestampUtc, ColorCategory.Debug);
            }
            if (IncludeTimestamp)
            {
                formatWriter.WriteTimestamp(entry.TimestampUtc, ColorCategory.Detail);
            }

            formatWriter.WriteField("Begin", ColorCategory.Detail, 7);
            formatWriter.WriteAbbreviatedTypeName(entry.CategoryName, ColorCategory.Debug, 36);

            _reflectionFormatter.FormatObject(entry.State, formatWriter);

            formatWriter.EndEntry();
        }

    }

}
