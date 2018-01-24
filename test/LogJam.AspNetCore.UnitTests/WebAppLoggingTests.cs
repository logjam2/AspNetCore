// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="WebAppLoggingTests.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.IO;
using System.Net.Http;

using LogJam.Config;
using LogJam.Extensions.Logging;
using LogJam.XUnit2;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;


namespace LogJam.AspNetCore
{

    /// <summary>
    /// Ensures that web app logging works as expected when using <c>LogJam.AspNetCore</c> and <c>LogJam.Extensions.Logging</c>.
    /// </summary>
    public sealed class WebAppLoggingTests
    {

        private readonly ITestOutputHelper _testOutput;

        public WebAppLoggingTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;
        }

        private IWebHostBuilder SetupWebHost(StringWriter testOutput)
        {
            var hostBuilder = new WebHostBuilder();

            // Configure services
            hostBuilder.UseLogJam((LogManagerConfig config, IServiceProvider serviceProvider) =>
                                  {
                                      config.UseTestOutput(_testOutput);
                                      config.UseTextWriter(testOutput);
                                  },
                                  () => new LogJamLoggerSettings
                                        {
                                            IncludeScopes = true,
                                            Filter = (category, logLevel) => true
                                        });

            hostBuilder.ConfigureServices(serviceCollection =>
                                          {
                                              IMvcCoreBuilder mvcCoreBuilder = serviceCollection.AddMvcCore();
                                              mvcCoreBuilder.AddJsonFormatters();
                                          });

            // Configure webapp
            hostBuilder.Configure(appBuilder =>
                                  {
                                      appBuilder.UseStatusCodePages();
                                      appBuilder.UseExceptionHandler(new ExceptionHandlerOptions()
                                                                     {
                                                                         ExceptionHandler = async context => {}
                                                                                            //{
                                                                                            //    context.Response.StatusCode = 500;
                                                                                            //    await context.Response.WriteAsync("Unhandled exception");
                                                                                            //}
                                                                     });
                                      appBuilder.UseMvc();
                                  });

            return hostBuilder;
        }

        [Fact]
        public async void SuccessfulApiResponseIsLogged()
        {
            var testOutput = new StringWriter();

            using (var testServer = new TestServer(SetupWebHost(testOutput)))
            {
                var httpClient = testServer.CreateClient();
                var responseJson = await httpClient.GetStringAsync("/api/test");
                Assert.Contains("\"foo\"", responseJson);

                _testOutput.WriteLine("Response body:\r\n" + responseJson);
            }

            var loggedOutput = testOutput.ToString();
            Assert.Contains("Selected output formatter 'Microsoft.AspNetCore.Mvc.Formatters.JsonOutputFormatter' and content type 'application/json' to write the response.", loggedOutput);
            Assert.Contains("Executed action LogJam.AspNetCore.Controllers.TestController.GetValues", loggedOutput);
            Assert.Contains("200 application/json", loggedOutput);
            Assert.DoesNotContain("Warn", loggedOutput);
            Assert.DoesNotContain("Error", loggedOutput);
        }

        [Fact]
        public async void NotFoundResponseIsLogged()
        {
            var testOutput = new StringWriter();

            using (var testServer = new TestServer(SetupWebHost(testOutput)))
            {
                var httpClient = testServer.CreateClient();
                var response = await httpClient.GetAsync("/api/notfound");
                Assert.Equal(404, (int) response.StatusCode);

                _testOutput.WriteLine("Response body:\r\n" + await response.Content.ReadAsStringAsync());
            }

            var loggedOutput = testOutput.ToString();
            Assert.Contains("404", loggedOutput);
            Assert.Contains("/api/notfound", loggedOutput);
            Assert.DoesNotContain("Error", loggedOutput);
        }

        [Fact]
        public async void ExceptionResponseIsLogged()
        {
            var testOutput = new StringWriter();

            using (var testServer = new TestServer(SetupWebHost(testOutput)))
            {
                var httpClient = testServer.CreateClient();
                var response = await httpClient.GetAsync("/api/test/exception");
                // TODO: Fix this
                Assert.Equal(500, (int) response.StatusCode);

                _testOutput.WriteLine("Response body:\r\n" + await response.Content.ReadAsStringAsync());
            }

            var loggedOutput = testOutput.ToString();
            Assert.Contains("500", loggedOutput);
            Assert.DoesNotContain("Warn", loggedOutput);
            Assert.Contains("Error", loggedOutput);
        }

    }

}
