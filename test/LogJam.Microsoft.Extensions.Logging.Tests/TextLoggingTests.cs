// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextLoggingTests.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;
    using System.IO;

    using global::Microsoft.Extensions.Logging;

    using LogJam.Config;
    using LogJam.Microsoft.Extensions.Logging.Entries;
    using LogJam.Microsoft.Extensions.Logging.Format;
    using LogJam.XUnit2;

    using Xunit;
    using Xunit.Abstractions;


    /// <summary>
    /// Validates that basic logging to text output works.
    /// </summary>
    public sealed class TextLoggingTests
    {

        private readonly ITestOutputHelper _testOutput;

        public TextLoggingTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public void BasicTextLoggingWorks()
        {
            var logOutput = new StringWriter();
            var logManager = new LogManager(new LogManagerConfig());
            logManager.Config.UseTextWriter(logOutput);
            logManager.Config.UseTestOutput(_testOutput); // Show log output in test output, too.
            logManager.Config.Writers.FormatAll<LoggerEntry>(new DefaultLoggerEntryFormatter() { IncludeEventId = true});

            var config = new LogJamLoggingConfig();
            var provider = new LogJamLoggerProvider(config, logManager, true);
            using (var loggerFactory = new LoggerFactory(new ILoggerProvider[] { provider }))
            {
                var logger = loggerFactory.CreateLogger(GetType());
                logger.LogInformation("Information message.");
                logger.LogWarning("Formatted warning: {nbr} {date:d}", 3.14f, new DateTime(2000, 1, 1));
                logger.LogDebug("Debug is off by default.");
            }

            var loggedText = logOutput.ToString();
            Assert.Contains("Information message.", loggedText);
            Assert.Contains("Formatted warning: 3.14 01/01/2000", loggedText);
            Assert.DoesNotContain("Debug is off by default.", loggedText);
        }

    }

}
