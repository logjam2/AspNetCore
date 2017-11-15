// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultLoggerEntryFormatter.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Format
{
    using System;

    using global::Microsoft.Extensions.Logging;

    using LogJam.Microsoft.Extensions.Logging.Entries;
    using LogJam.Trace;
    using LogJam.Writer.Text;

    /// <summary>
    /// Default formatter for writing <see cref="LoggerEntry"/> entries to text logs.
    /// </summary>
    public class DefaultLoggerEntryFormatter : EntryFormatter<LoggerEntry>
    {
        /// <summary>
        /// The default value for <see cref="MaxIndentLevel" /> if no value is set.
        /// </summary>
        public const int DefaultMaxIndentLevel = 4;

        /// <summary>
        /// Initializes a new <see cref="DefaultLoggerEntryFormatter"/>.
        /// </summary>
        public DefaultLoggerEntryFormatter()
        {
            MaxIndentLevel = DefaultMaxIndentLevel;
            IncludeTimestamp = true;
        }

        #region Public Properties

        /// <summary>
        /// <c>true</c> to include the Date when formatting <see cref="LoggerEntry" />s. Default is <c>false</c>.
        /// </summary>
        public bool IncludeDate { get; set; }

        /// <summary>
        /// <c>true</c> to include the TimestampUtc when formatting <see cref="LoggerEntry" />s. Default is <c>true</c>.
        /// </summary>
        public bool IncludeTimestamp { get; set; }

        /// <summary>
        /// Set to a value to alter the trace entry indent level from what is set.
        /// </summary>
        public int RelativeIndentLevel { get; set; }

        /// <summary>
        /// Don't indent any trace entries by more than this value.
        /// </summary>
        public int MaxIndentLevel { get; set; }

        /// <summary>
        /// <c>true</c> to include the <see cref="EventId"/> when formatting <see cref="LoggerEntry" />s. Default is <c>false</c>.
        /// </summary>
        public bool IncludeEventId { get; set; }

        #endregion

        #region Formatter methods

        public override void Format(ref LoggerEntry entry, FormatWriter formatWriter)
        {
            ColorCategory color = ColorCategory.None;
            if (formatWriter.IsColorEnabled)
            {
                color = LogLevelToColorCategory(entry.LogLevel);
            }

            int entryIndentLevel = formatWriter.IndentLevel + RelativeIndentLevel;
            entryIndentLevel = Math.Min(entryIndentLevel, MaxIndentLevel);

            formatWriter.BeginEntry(entryIndentLevel);

            if (IncludeDate)
            {
                formatWriter.WriteDate(entry.TimestampUtc, ColorCategory.Debug);
            }
            if (IncludeTimestamp)
            {
                formatWriter.WriteTimestamp(entry.TimestampUtc, ColorCategory.Detail);
            }

            formatWriter.WriteField(LogLevelToLabel(entry.LogLevel), color, 7);
            formatWriter.WriteAbbreviatedTypeName(entry.CategoryName, ColorCategory.Debug, 36);

            if (IncludeEventId)
            {
                formatWriter.WriteField(entry.EventId.ToString(), color, 6);
            }

            string message = entry.DefaultFormatter(entry.State, null);
            formatWriter.WriteField(message.Trim(), color);
            if (entry.Exception != null)
            {
                ColorCategory detailColor = color == ColorCategory.Debug ? ColorCategory.Debug : ColorCategory.Detail;
                formatWriter.WriteLines(entry.Exception.ToString(), detailColor, 1);
            }

            formatWriter.EndEntry();
        }

        #endregion

        protected ColorCategory LogLevelToColorCategory(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    return ColorCategory.Info;
                case LogLevel.Warning:
                    return ColorCategory.Warning;
                case LogLevel.Debug:
                    return ColorCategory.Detail;
                case LogLevel.Trace: // In Microsoft.Extensions.Logging, Trace is more detailed than Debug
                    return ColorCategory.Debug;
                case LogLevel.Error:
                    return ColorCategory.Error;
                case LogLevel.Critical:
                    return ColorCategory.SevereError;
                default:
                    return ColorCategory.None;
            }
        }

        protected string LogLevelToLabel(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Information:
                    return "Info";
                case LogLevel.Warning:
                    return "Warning";
                case LogLevel.Debug:
                    return "Debug";
                case LogLevel.Trace:
                    return "Trace";
                case LogLevel.Error:
                    return "Error";
                case LogLevel.Critical:
                    return "CRITICAL";
                case LogLevel.None:
                    return "None";
                default:
                    return "";
            }
        }

    }

}
