// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionFormatter.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Extensions.Logging.Format
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using LogJam.Trace.Config;
    using LogJam.Writer.Text;

#pragma warning disable IDE0008 // Use explicit type

    /// <summary>
    /// Provides shared formatting logic for logging an unknown entry type via reflection. This is not expected to be a good choice
    /// for performant logging, but is useful for discovering new entry types that are logged.
    /// </summary>
    // TODO: This class could probably be move to the LogJam library, it's generally useful and not tied to ASP.NET logging.
    // TODO: Separate type examination from recursion from formatting.
    // TODO: Improve performance by generating compiled code
    internal sealed class ReflectionFormatter
    {

        private const int c_defaultMaxRecursion = 10;

        private static readonly Type s_typeOfGenericIEnumerable = typeof(IEnumerable<>);
        private static readonly Type s_typeOfKeyValuePair = typeof(KeyValuePair<,>);

        private readonly Func<Type, string> _typeNameFunc = TraceManagerConfig.DefaultTypeNameFunc;

        /// <summary>
        ///  Set to <c>true</c> to include type names in the output.
        /// </summary>
        public bool IncludeTypeNames { get; set; }

        /// <summary>
        /// The max depth to walk when formatting objects.
        /// </summary>
        public int MaxDepth { get; set; } = c_defaultMaxRecursion;

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
            }
            else
            {
                // If o is a Primitive, don't format it as an object
                if (ShouldFormatAsPrimitive(o))
                {
                    FormatAsPrimitive(o, formatWriter);
                }
                else
                {
                    InnerFormatObject(o, formatWriter, 0, new Stack<object>(10), null);
                }
            }
        }

        /// <summary>
        /// Formats <paramref name="o"/> and its sub-objects.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="formatWriter"></param>
        /// <param name="recursionLevel"></param>
        /// <param name="lineage"></param>
        /// <param name="propertyType">The type of the property set to value <paramref name="o"/>. May be <c>null</c> if not applicable or unknown.</param>
        /// <returns>The max <paramref name="recursionLevel"/> reached when <paramref name="o"/> was formatted.</returns>
        private int InnerFormatObject(object o, FormatWriter formatWriter, int recursionLevel, Stack<object> lineage, Type propertyType)
        {
            if (o == null)
            {
                formatWriter.WriteText("(null)", ColorCategory.Debug);
                return recursionLevel;
            }

            if (recursionLevel >= MaxDepth)
            {
                formatWriter.WriteText("(Recursion limit exceeded)", ColorCategory.Warning);
                return recursionLevel;
            }

            int parentNumber = 0;
            foreach (var ancestor in lineage)
            {
                if (object.ReferenceEquals(ancestor, o))
                {
                    formatWriter.WriteText("(Parent " + parentNumber + ")", ColorCategory.Detail);
                    return recursionLevel;
                }

                parentNumber++;
            }

            // If o is a Primitive, don't format it as an object, don't change indent, etc
            if (ShouldFormatAsPrimitive(o))
            {
                FormatAsPrimitive(o, formatWriter);
                return recursionLevel;
            }

            int previousIndentLevel = formatWriter.IndentLevel;
            //formatWriter.IndentLevel += 1;
            lineage.Push(o);

            int maxRecursionLevel;
            if (o is IEnumerable)
            {
                if (IsDictionary(o, out var formatAsDictionaryFunc))
                {
                    // Handle IEnumerable<KeyValuePair>; don't reflect individual properties
                    maxRecursionLevel = formatAsDictionaryFunc(o, formatWriter, recursionLevel, lineage, propertyType);
                }
                else
                {
                    // Handle IEnumerable; don't reflect properties
                    maxRecursionLevel = FormatAsEnumerable((IEnumerable) o, formatWriter, recursionLevel, lineage);
                }
            }
            else
            {
                maxRecursionLevel = FormatAsObject(o, formatWriter, recursionLevel, lineage, propertyType);
            }

            formatWriter.IndentLevel = previousIndentLevel;
            lineage.Pop();
            return maxRecursionLevel;
        }

        private bool ShouldFormatAsPrimitive(object o)
        {
            if (Convert.GetTypeCode(o) != TypeCode.Object)
            {
                return true;
            }

            if ((o is Type) || (o is TypeInfo))
            {
                return true;
            }

            return false;
        }

        private bool IsDictionary(object o, out Func<object, FormatWriter, int, Stack<object>, Type, int> formatAsDictionaryFunc)
        {
            foreach (Type interfaceType in o.GetType().GetTypeInfo().ImplementedInterfaces)
            {
                if (interfaceType.IsConstructedGenericType && (interfaceType.GetGenericTypeDefinition() == s_typeOfGenericIEnumerable))
                {
                    Type[] genericTypeArguments = interfaceType.GenericTypeArguments;
                    if ((genericTypeArguments.Length == 1) &&
                        genericTypeArguments[0].IsConstructedGenericType &&
                        (genericTypeArguments[0].GetGenericTypeDefinition() == s_typeOfKeyValuePair))
                    {
                        Type[] keyValuePairTypeArguments = genericTypeArguments[0].GenericTypeArguments;
                        if (keyValuePairTypeArguments.Length == 2)
                        {   // We have an IEnumerable<KeyValuePair<,>>. Now, construct a typed call to FormatAsDictionary
                            var members = GetType().GetTypeInfo().DeclaredMembers;
                            var formatAsDictionaryMethod = (MethodInfo) members.First(m => m.Name == "FormatAsDictionary");//Get.GetRuntimeMethod(, keyValuePairTypeArguments));
                            var typedFormatAsDictionaryMethod = formatAsDictionaryMethod.MakeGenericMethod(keyValuePairTypeArguments);
                            formatAsDictionaryFunc = (Func<object, FormatWriter, int, Stack<object>, Type, int>) typedFormatAsDictionaryMethod.CreateDelegate(typeof(Func<object, FormatWriter, int, Stack<object>, Type, int>), this);
                            return true;
                        }
                    }
                }
            }

            formatAsDictionaryFunc = null;
            return false;
        }

        private void FormatAsPrimitive(object o, FormatWriter formatWriter)
        {
            var s = o.ToString();
            if (s.Any(c => (c == '"') || char.IsWhiteSpace(c)))
            {
                formatWriter.WriteText("\"", ColorCategory.Markup);
                formatWriter.WriteText(s.Replace("\"", "\"\""), ColorCategory.Detail);
                formatWriter.WriteText("\"", ColorCategory.Markup);
            }
            else
            {
                formatWriter.WriteText(s, ColorCategory.Detail);
            }
        }

        /// <summary>
        /// Formats <paramref name="o"/> as an object.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="formatWriter"></param>
        /// <param name="recursionLevel"></param>
        /// <param name="lineage"></param>
        /// <param name="parentPropertyType">The type of the property set to value <paramref name="o"/>. May be <c>null</c> if not applicable or unknown.</param>
        /// <returns>The max <paramref name="recursionLevel"/> reached when <paramref name="o"/> was formatted.</returns>
        private int FormatAsObject(object o, FormatWriter formatWriter, int recursionLevel, Stack<object> lineage, Type parentPropertyType)
        {
            int maxRecursionLevel = recursionLevel;

            formatWriter.WriteField("{", ColorCategory.Markup);

            if (recursionLevel > 0)
            {
                formatWriter.WriteEndLine();
            }

            formatWriter.IndentLevel += 1;

            Type objectType = o.GetType();
            if (IncludeTypeNames && (parentPropertyType != objectType))
            {
                formatWriter.WriteField("type:", ColorCategory.Debug);
                formatWriter.WriteText(_typeNameFunc(objectType), ColorCategory.Debug);
                formatWriter.WriteLine();
            }

            // Write properties
            foreach (var property in objectType.GetReadablePublicProperties())
            {
                bool colonWritten = false;

                void WriteErrorAsPropertyValue(string errorMessage)
                {
                    if (!colonWritten)
                    {
                        formatWriter.WriteText(":", ColorCategory.Markup);
                    }

                    formatWriter.WriteText("!(", ColorCategory.Markup);
                    formatWriter.WriteText(errorMessage, ColorCategory.Warning);
                    formatWriter.WriteText(")!", ColorCategory.Markup);
                }

                try
                {
                    formatWriter.WriteField(property.Name, ColorCategory.Detail);
                    Type propertyType = property.PropertyType;

                    ParameterInfo[] propertyIndexParameters = property.GetIndexParameters();
                    if (propertyIndexParameters.Length > 0)
                    {
                        // Properties with index parameters are not formatted - they act like functions, it's not possible to enumerate all contained values.
                        WriteErrorAsPropertyValue("Properties with index parameters are not formatted");
                        continue;
                    }

                    // GetValue(object) throws if the property has index parameters
                    object propertyValue = property.GetValue(o);

                    if (IncludeTypeNames)
                    {
                        // Only write the propertyType if it doesn't equal the subobject type
                        bool writePropertyType = !object.ReferenceEquals(propertyValue?.GetType(), propertyType);
                        if (writePropertyType)
                        {
                            formatWriter.WriteText("(", ColorCategory.Markup);
                            formatWriter.WriteText(_typeNameFunc(propertyType), ColorCategory.Debug);
                            formatWriter.WriteText(")", ColorCategory.Markup);
                        }
                    }

                    formatWriter.WriteText(":", ColorCategory.Markup);
                    colonWritten = true;

                    int propertyMaxRecursionLevel = InnerFormatObject(propertyValue, formatWriter, recursionLevel + 1, lineage, propertyType);
                    maxRecursionLevel = Math.Max(maxRecursionLevel, propertyMaxRecursionLevel);
                }
                catch (Exception excp)
                {   // Exception reading or formatting the property value.
                    WriteErrorAsPropertyValue(excp.ToString());
                }
            }

            // No newline for simple objects with no sub-objects
            if (maxRecursionLevel > 1)
            {
                formatWriter.WriteEndLine();
            }

            formatWriter.IndentLevel -= 1;
            formatWriter.WriteField("}", ColorCategory.Markup);

            return maxRecursionLevel;
        }

        /// <summary>
        /// Formats <paramref name="untypedDictionary"/> as dictionary with key type <typeparamref name="TKey"/> and value type <typeparamref name="TValue"/>.
        /// </summary>
        /// <param name="untypedDictionary"></param>
        /// <param name="formatWriter"></param>
        /// <param name="recursionLevel"></param>
        /// <param name="lineage"></param>
        /// <param name="parentPropertyType">The type of the property set to object <paramref name="untypedDictionary"/>. May be <c>null</c> if not applicable or unknown.</param>
        /// <returns>The max <paramref name="recursionLevel"/> reached when <paramref name="untypedDictionary"/> was formatted.</returns>
        // ReSharper disable once UnusedMember.Local
        private int FormatAsDictionary<TKey, TValue>(object untypedDictionary, FormatWriter formatWriter, int recursionLevel, Stack<object> lineage, Type parentPropertyType)
        {
            // OK that this throws on failure; this shouldn't happen
            var dictionary = (IEnumerable<KeyValuePair<TKey, TValue>>) untypedDictionary;

            int maxRecursionLevel = recursionLevel;

            if (! dictionary.Any())
            {
                formatWriter.WriteField("{}", ColorCategory.Markup);
                return recursionLevel;
            }
            else
            {
                formatWriter.WriteField("{", ColorCategory.Markup);
                formatWriter.IndentLevel += 1;

                bool onFirstKvp = true;

                Type dictionaryType = dictionary.GetType();
                if (IncludeTypeNames && (parentPropertyType != dictionaryType))
                {
                    formatWriter.WriteField("type:", ColorCategory.Debug);
                    formatWriter.WriteText(_typeNameFunc(dictionaryType), ColorCategory.Debug);
                    onFirstKvp = false;
                }

                foreach (var kvp in dictionary)
                {
                    if (! onFirstKvp)
                    {
                        formatWriter.WriteText(",", ColorCategory.Markup);
                    }
                    formatWriter.WriteEndLine();
                    onFirstKvp = false;

                    formatWriter.WriteField("{", ColorCategory.Markup);
                    formatWriter.IndentLevel += 1;

                    // If key is primitive, format on single line
                    TKey key = kvp.Key;
                    TValue value = kvp.Value;
                    int childMaxRecursionLevel;
                    if (ShouldFormatAsPrimitive(key))
                    {
                        formatWriter.WriteText(formatWriter.FieldDelimiter, ColorCategory.Markup);
                        FormatAsPrimitive(key, formatWriter);
                        formatWriter.WriteText(":", ColorCategory.Markup);
                        childMaxRecursionLevel = InnerFormatObject(value, formatWriter, recursionLevel + 1, lineage, typeof(TValue));
                    }
                    else
                    {
                        formatWriter.WriteField("Key", ColorCategory.Detail);
                        formatWriter.WriteText(":", ColorCategory.Markup);
                        childMaxRecursionLevel = InnerFormatObject(key, formatWriter, recursionLevel + 1, lineage, typeof(TKey));
                        maxRecursionLevel = Math.Max(maxRecursionLevel, childMaxRecursionLevel);

                        formatWriter.WriteText(",", ColorCategory.Markup);
                        formatWriter.WriteEndLine();
                        formatWriter.WriteField("Value", ColorCategory.Detail);
                        formatWriter.WriteText(":", ColorCategory.Markup);
                        childMaxRecursionLevel = InnerFormatObject(value, formatWriter, recursionLevel + 1, lineage, typeof(TValue));
                    }
                    maxRecursionLevel = Math.Max(maxRecursionLevel, childMaxRecursionLevel);

                    if (! ShouldFormatAsPrimitive(value))
                    {
                        formatWriter.WriteLine();
                    }

                    formatWriter.IndentLevel -= 1;
                    formatWriter.WriteField("}", ColorCategory.Markup);
                }

                formatWriter.WriteEndLine();
 
                formatWriter.IndentLevel -= 1;
                formatWriter.WriteField("}", ColorCategory.Markup);
            }

            return maxRecursionLevel;
        }

        /// <summary>
        /// Formats <paramref name="enumerable"/> as an array.
        /// </summary>
        /// <param name="enumerable"></param>
        /// <param name="formatWriter"></param>
        /// <param name="recursionLevel"></param>
        /// <param name="lineage"></param>
        /// <returns>The max <paramref name="recursionLevel"/> reached when <paramref name="enumerable"/> was formatted.</returns>
        private int FormatAsEnumerable(IEnumerable enumerable, FormatWriter formatWriter, int recursionLevel, Stack<object> lineage)
        {
            int maxRecursionLevel = recursionLevel;
            bool isEmpty = true;
            bool enumerableContainsAllPrimitives = true;

            // Determine if there are any non-primitive elements
            foreach (object element in enumerable)
            {
                isEmpty = false;
                if (! ShouldFormatAsPrimitive(element))
                {
                    enumerableContainsAllPrimitives = false;
                    break;
                }
            }

            if (isEmpty)
            {
                formatWriter.WriteField("[]", ColorCategory.Markup);
                return recursionLevel;
            }
            else
            {
                formatWriter.WriteField("[", ColorCategory.Markup);
                formatWriter.IndentLevel += 1;

                bool onFirstElement = true;
                foreach (object element in enumerable)
                {
                    if (! onFirstElement)
                    {
                        formatWriter.WriteText(",", ColorCategory.Markup);
                    }
                    if (! enumerableContainsAllPrimitives)
                    {
                        formatWriter.WriteEndLine();
                    }
                    onFirstElement = false;

                    // Format as a sub-object (indented, on a newline)
                    int propertyMaxRecursionLevel = InnerFormatObject(element, formatWriter, recursionLevel + 1, lineage, null);
                    maxRecursionLevel = Math.Max(maxRecursionLevel, propertyMaxRecursionLevel);
                }

                formatWriter.IndentLevel -= 1;
                if (!enumerableContainsAllPrimitives)
                {
                    formatWriter.WriteEndLine();
                }
                formatWriter.WriteField("]", ColorCategory.Markup);
            }

            return maxRecursionLevel;
        }

    }
}
