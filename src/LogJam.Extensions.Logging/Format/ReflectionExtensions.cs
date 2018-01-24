// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Format
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;


    /// <summary>
    /// Extension methods on <see cref="Type"/> and/or <see cref="TypeInfo"/>.
    /// </summary>
    internal static class ReflectionExtensions
    {

        /// <summary>
        /// Returns all the gettable public properties declared in <paramref name="type"/>.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static IEnumerable<PropertyInfo> GetReadablePublicProperties(this Type type)
        {
            while (type != null)
            {
                TypeInfo typeInfo = type.GetTypeInfo();
                foreach (var property in typeInfo.DeclaredProperties)
                {
                    if (property.GetMethod?.IsPublic == true)
                    {
                        yield return property;
                    }
                }

                type = typeInfo.BaseType;
            }
        }

    }

}
