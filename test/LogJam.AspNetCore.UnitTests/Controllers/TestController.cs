// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestController.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;

using Microsoft.AspNetCore.Mvc;

namespace LogJam.AspNetCore.Controllers
{

    /// <summary>
    /// Test controller for exercising ASP.NET Core
    /// </summary>
    [Route("api/test")]
    public sealed class TestController : Controller
    {

        [HttpGet]
        public string[] GetValues()
        {
            return new[] { "foo", "bar" };
        }

        [HttpGet("exception")]
        public string[] ThrowException()
        {
            throw new InvalidOperationException("Expected test exception");
        }

    }

}
