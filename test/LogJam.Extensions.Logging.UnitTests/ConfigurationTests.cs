// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigurationTests.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;

using LogJam.Config;
using LogJam.Extensions.Logging.Entries;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Xunit;

namespace LogJam.Extensions.Logging
{

    /// <summary>
    /// Verifies that different approaches to configuring <c>Microsoft.Extensions.Logging</c> work with LogJam.
    /// </summary>
    public sealed class ConfigurationTests
    {

        /// <summary>
        /// Shared tests that are run against several ways that configuration can be done.
        /// </summary>
        public abstract class BaseConfigurationTests
        {

            public BaseConfigurationFixture ConfigurationFixture { get; }

            protected BaseConfigurationTests(BaseConfigurationFixture configurationFixture)
            {
                ConfigurationFixture = configurationFixture ?? throw new ArgumentNullException(nameof(configurationFixture));
            }

            public static TheoryData<string, LogLevel, bool> LoggerFilterTestCases()
            {
                var testCases = new TheoryData<string, LogLevel, bool>
                {
                    { /* TypeName: */ "Foo", /* Level: */ LogLevel.Debug, /* EntryExpected: */ false },
                    { /* TypeName: */ "Foo", /* Level: */ LogLevel.Information, /* EntryExpected: */ true },

                    { /* TypeName: */ "System.Data", /* Level: */ LogLevel.Trace, /* EntryExpected: */ false },
                    { /* TypeName: */ "System.Data", /* Level: */ LogLevel.Debug, /* EntryExpected: */ true },

                    { /* TypeName: */ "Microsoft.Extensions.Logging", /* Level: */ LogLevel.Trace, /* EntryExpected: */ false },
                    { /* TypeName: */ "Microsoft.Extensions.Logging", /* Level: */ LogLevel.Debug, /* EntryExpected: */ false }
                };
                return testCases;
            }

            [Theory]
            [MemberData(nameof(LoggerFilterTestCases))]
            public void LoggerFiltersWork(string categoryName, LogLevel logLevel, bool entryShouldBeWritten)
            {
                ConfigurationFixture.Entries.Clear();
                string logMessage = categoryName + ":" + logLevel;

                var logger = ConfigurationFixture.GetLoggerFactory().CreateLogger(categoryName);
                logger.Log(logLevel, 0, 0, null, (state, exception) => logMessage);

                if (! entryShouldBeWritten)
                {
                    Assert.Empty(ConfigurationFixture.Entries);
                }
                else
                {
                    Assert.Collection(ConfigurationFixture.Entries,
                                      loggerEntry =>
                                      {
                                          Assert.Equal(logMessage, loggerEntry.GetFormattedMessage());
                                          Assert.Equal(logLevel, loggerEntry.LogLevel);
                                      });
                }
            }
        }


        /// <summary>
        /// Shared DI + LoggerFactory setup + teardown.
        /// </summary>
        public abstract class BaseConfigurationFixture : IDisposable
        {
            protected readonly ServiceCollection serviceCollection = new ServiceCollection();
            private IServiceProvider _serviceProvider;

            protected BaseConfigurationFixture()
            {
                serviceCollection.ConfigureLogManager((logConfig, s) => logConfig.UseList(Entries).BackgroundLogging = false);
            }

            public void Dispose()
            {
                if (_serviceProvider is IDisposable disposableServiceProvider)
                {
                    disposableServiceProvider.Dispose();
                    _serviceProvider = null;
                }
            }

            /// <summary>
            /// ILogger output is written to entries.
            /// </summary>
            public List<LoggerEntry> Entries { get; } = new List<LoggerEntry>();

            public ILoggerFactory GetLoggerFactory()
            {
                if (_serviceProvider == null)
                {
                    _serviceProvider = serviceCollection.BuildServiceProvider();
                }

                return _serviceProvider.GetRequiredService<ILoggerFactory>();
            }

        }

#if ASPNETCORE2_0
        /// <summary>
        /// Configures LogJam via <see cref="ILoggingBuilder"/> configuration.
        /// </summary>
        public class UseBuilderConfigurationFixture : BaseConfigurationFixture
        {

            public UseBuilderConfigurationFixture()
            {
                serviceCollection.AddLogging(loggingBuilder =>
                                                 loggingBuilder.AddLogJam()
                                                               .SetMinimumLevel(LogLevel.Information)
                                                               //.AddFilter("Default", LogLevel.Information)
                                                               .AddFilter("System", LogLevel.Debug)
                                                               .AddFilter<LogJamLoggerProvider>("Microsoft", LogLevel.None));
            }

        }


        public class UseBuilderTests : BaseConfigurationTests, IClassFixture<UseBuilderConfigurationFixture>
        {
            public UseBuilderTests(UseBuilderConfigurationFixture configurationFixture)
            : base(configurationFixture)
            {}

        }
#endif

        /// <summary>
        /// Configure using LogJam via ServiceCollection configuration
        /// </summary>
        public class ServiceConfigurationFixture : BaseConfigurationFixture
        {

            public ServiceConfigurationFixture()
            {
                serviceCollection.AddLogJamLoggerProvider();
#if ASPNETCORE2_0
                serviceCollection.Configure<LoggerFilterOptions>(filterOptions =>
                                                                 {
                                                                     filterOptions.MinLevel = LogLevel.Information;
                                                                     filterOptions.AddFilter("System", LogLevel.Debug)
                                                                                  .AddFilter<LogJamLoggerProvider>("Microsoft", LogLevel.None);
                                                                 });
#else
                serviceCollection.AddSingleton<IFilterLoggerSettings>(new FilterLoggerSettings
                                                                      {
                                                                          { "Default", LogLevel.Information },
                                                                          { "System", LogLevel.Debug },
                                                                          { "Microsoft", LogLevel.None } // REVIEW: Not filtered to just LogJamLoggerProvider; no way to do that here
                                                                      });
#endif
            }

        }


        public class ServiceConfigurationTests : BaseConfigurationTests, IClassFixture<ServiceConfigurationFixture>
        {
            public ServiceConfigurationTests(ServiceConfigurationFixture configurationFixture)
                : base(configurationFixture)
            { }

        }


#if ASPNETCORE2_0

        public class JsonConfigurationFixture : BaseConfigurationFixture
        {

            public JsonConfigurationFixture()
            {
                var json =
@"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""System"": ""Debug""
    },
    ""LogJam"": {
      ""LogLevel"": {
        ""Microsoft"": ""None""
      }
    }
  }
}";
                var config = JsonTestConfiguration.Create(() => json);

                serviceCollection.AddLogging(loggingBuilder => loggingBuilder.AddLogJam()
                                                               .AddConfiguration(config.GetSection("Logging")));
            }

        }

        public class JsonConfigurationTests : BaseConfigurationTests, IClassFixture<JsonConfigurationFixture>
        {

            public JsonConfigurationTests(JsonConfigurationFixture configurationFixture)
                : base(configurationFixture)
            {}

        }
#endif

    }

}
