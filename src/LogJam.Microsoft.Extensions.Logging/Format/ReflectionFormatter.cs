// // --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionFormatter.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging.Format
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using LogJam.Writer.Text;


    /// <summary>
    /// Provides shared formatting logic for logging an object via reflection. This is not expected to be a good choice
    /// for performant logging, but is useful for discovering new data that is logged.
    /// </summary>
    // TODO: This class could probably be move to the LogJam library, it's generally useful and not tied to ASP.NET logging.
    public sealed class ReflectionFormatter
    {

        private const int maxRecursion = 10;

        /// <summary>
        /// Reflects object <paramref name="o"/>, writes its type and properties to <paramref name="formatWriter"/>.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="formatWriter"></param>
        public void FormatObject(object o, FormatWriter formatWriter)
        {
            if (o == null)
            {
                // Top level object is not usually expected to be null, which is why it's formatted as a warning
                formatWriter.WriteField("(null)", ColorCategory.Warning);
                return;
            }
            // If o is a Primitive, don't format it as an object
            if (Convert.GetTypeCode(o) != TypeCode.Object)
            {
                // Format as a primitive
                formatWriter.WriteField(o.ToString(), ColorCategory.Detail);
                return;
            }

            InnerFormatObject(o, formatWriter, 0, new Stack<object>(10));
        }

        private void InnerFormatObject(object o, FormatWriter formatWriter, int recursionLevel, Stack<object> lineage)
        {
            if (recursionLevel >= maxRecursion)
            {
                formatWriter.WriteField("(Recursion limit exceeded)", ColorCategory.Warning);
                return;
            }
            if (lineage.Any(ancestor => object.ReferenceEquals(ancestor, o)))
            {
                formatWriter.WriteField("(Cycle in object graph)", ColorCategory.Detail);
                return;
            }

            int previousIndentLevel = formatWriter.IndentLevel;
            formatWriter.IndentLevel += 2;
            lineage.Push(this);

            formatWriter.WriteText("{ ", ColorCategory.Markup);
            formatWriter.WriteText("Type: ", ColorCategory.Debug);
            Type objectType = o.GetType();
            formatWriter.WriteText(objectType.FullName, ColorCategory.Debug);
            formatWriter.WriteSpaces(1);

            // TODO: Handle IEnumerable<KeyValuePair> + IEnumerable; don't reflect their properties

            // Write properties
            foreach (var property in objectType.GetRuntimeProperties())
            {
                formatWriter.WriteText(property.Name, ColorCategory.Detail);
                formatWriter.WriteText("(", ColorCategory.Markup);
                Type propertyType = property.PropertyType;
                formatWriter.WriteText(propertyType.FullName, ColorCategory.Debug);
                formatWriter.WriteText(")", ColorCategory.Markup);
                if (property.CanRead)
                {
                    formatWriter.WriteText(": ", ColorCategory.Markup);
                    try
                    {
                        var propertyValue = property.GetValue(o);
                        if (propertyValue == null)
                        {
                            formatWriter.WriteField("(null)", ColorCategory.Debug);
                        }
                        else
                        {
                            // Format primitives inline, and classes as sub-objects.
                            if (Convert.GetTypeCode(propertyValue) != TypeCode.Object)
                            {
                                // Format as a primitive
                                formatWriter.WriteField(propertyValue.ToString(), ColorCategory.Detail);
                            }
                            else
                            {
                                // Format as a sub-object (indented, on a newline)
                                formatWriter.WriteEndLine();
                                InnerFormatObject(propertyValue, formatWriter, recursionLevel + 1, lineage);
                                formatWriter.WriteEndLine();
                            }
                        }
                    }
                    catch (Exception excp)
                    {
                        // Exception reading or formatting the property value.
                        formatWriter.WriteText(excp.ToString(), ColorCategory.Warning);
                    }
                }

                formatWriter.WriteSpaces(1);
            }

            formatWriter.WriteText("}", ColorCategory.Markup);
            formatWriter.IndentLevel = previousIndentLevel;
            lineage.Pop();
        }

    }

}
