// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TextLoggingTests.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Extensions.Logging
{
    using System;
    using System.IO;

    using Microsoft.Extensions.Logging;

    using LogJam.Config;
    using LogJam.Extensions.Logging.Entries;
    using LogJam.Extensions.Logging.Format;
    using LogJam.XUnit2;

    using Xunit;
    using Xunit.Abstractions;


    /// <summary>
    /// Validates that basic logging to text output works.
    /// </summary>
    public sealed class TextLoggingTests : IDisposable
    {

        private readonly ITestOutputHelper _testOutput;

        private readonly StringWriter _logOutput;
        private readonly LogManager _logManager;
        private readonly LoggerFactory _loggerFactory;

        public TextLoggingTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;

            _logOutput = new StringWriter();
            _logManager = new LogManager(new LogManagerConfig());
            _logManager.Config.UseTextWriter(_logOutput);
            _logManager.Config.UseTestOutput(_testOutput); // Show log output in test output, too.
            _logManager.Config.Writers.FormatAll<LoggerEntry>(new DefaultLoggerEntryFormatter());
            _logManager.Config.Writers.FormatAll(new GenericLoggerBeginScopeEntryFormatter());
            _logManager.Config.Writers.FormatAll(new GenericLoggerEndScopeEntryFormatter());

            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddLogJam(_logManager, true, LogJamLogger.LogInformation);
        }

        public void Dispose()
        {
            _loggerFactory?.Dispose();
        }

        [Fact]
        public void BasicTextLoggingWorks()
        {
            var logger = _loggerFactory.CreateLogger(GetType());
            logger.LogInformation("Information message.");
            logger.LogWarning("Formatted warning: {nbr} {date:d}", 3.14f, new DateTime(2000, 1, 1));
            logger.LogDebug("Debug is off.");

            var loggedText = _logOutput.ToString();
            Assert.Contains("Information message.", loggedText);
            Assert.Contains("Formatted warning: 3.14 01/01/2000", loggedText);
            Assert.DoesNotContain("Debug is off.", loggedText);
        }

        [Fact]
        public void ScopeWithMessageLoggingWorks()
        {
            var logger = _loggerFactory.CreateLogger(GetType());
            using (var scope = logger.BeginScope("Scope message {0}", 1))
            {
                logger.LogInformation("Info inside scope");
            }

            var loggedText = _logOutput.ToString();
            Assert.Contains("Begin", loggedText);
            Assert.Contains("End", loggedText);
        }

        [Fact]
        public void TypedScopeLoggingWorks()
        {
            var logger = _loggerFactory.CreateLogger(GetType());
            var testScope = new TestScope
                            {
                                TestClass = GetType(),
                                TestName = nameof(TypedScopeLoggingWorks)
                            };

            using (var loggerScope = logger.BeginScope(testScope))
            {
                logger.LogInformation("Info inside scope");
            }

            var loggedText = _logOutput.ToString();
            Assert.Contains("Begin", loggedText);
            Assert.Contains("End", loggedText);
        }


        private class TestScope
        {

            public Type TestClass { get; set; }
            public string TestName { get; set; }

        }
    }

}
