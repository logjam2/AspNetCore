// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebAppLoggingTests.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;


namespace LogJam.AspNetCore
{

    using LogJam.AspNetCore.Controllers;
    using LogJam.Config;
    using LogJam.Extensions.Logging;
    using LogJam.XUnit2;


    /// <summary>
    /// 
    /// </summary>
    public sealed class WebAppLoggingTests
    {

        private readonly ITestOutputHelper _testOutput;

        public WebAppLoggingTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        [Fact]
        public async void WebApiRequestIsLogged()
        {
            var testOutput = new StringWriter();

            var hostBuilder = new WebHostBuilder();

            // Configure services
            hostBuilder.UseLogJam((LogManagerConfig config, IServiceProvider serviceProvider) =>
                                  {
                                      config.UseTestOutput(_testOutput).BackgroundLogging = false;
                                      config.UseTextWriter(testOutput).BackgroundLogging = false;
                                  },
                                  () => new LogJamLoggerSettings
                                        {
                                            IncludeScopes = true,
                                            Filter = (category, logLevel) => true
                                        });

            hostBuilder.ConfigureServices(serviceCollection =>
                                              serviceCollection.AddMvcCore(options =>
                                                                               options.Conventions.Add(new SingleControllerApplicationModelConvention(typeof(TestController)))));

            // Configure webapp
            hostBuilder.Configure(appBuilder => appBuilder.UseMvc());

            using (var testServer = new TestServer(hostBuilder))
            {
                var httpClient = testServer.CreateClient();
                var responseJson = await httpClient.GetStringAsync("/api/test");
                Assert.Contains("\"foo\"", responseJson);
            }
        }

    }

}
