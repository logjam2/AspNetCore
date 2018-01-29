// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="JsonTestConfiguration.cs">
// Copyright (c) 2011-2018 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace LogJam.Extensions.Logging
{

    /// <summary>
    /// Reads JSON configuration from a string.
    /// </summary>
    public static class JsonTestConfiguration
    {

        public static ConfigurationRoot Create(Func<string> loadJson)
        {
            var provider = new JsonTestConfigurationProvider(new JsonConfigurationSource { Optional = true }, loadJson);
            return new ConfigurationRoot(new List<IConfigurationProvider> { provider });
        }

        private class JsonTestConfigurationProvider : JsonConfigurationProvider
        {
            private Func<string> _loadJson;

            public JsonTestConfigurationProvider(JsonConfigurationSource source, Func<string> loadJson)
                : base(source)
            {
                _loadJson = loadJson;
            }

            public override void Load()
            {
                var stream = new MemoryStream();
                var writer = new StreamWriter(stream);
                writer.Write(_loadJson());
                writer.Flush();
                stream.Seek(0, SeekOrigin.Begin);
                Load(stream);
            }
        }

    }

}
