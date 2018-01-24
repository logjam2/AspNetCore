// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReflectionFormatterTests.cs">
// Copyright (c) 2011-2017 https://github.com/logjam2.  
// </copyright>
// Licensed under the <a href="https://github.com/logjam2/logjam/blob/master/LICENSE.txt">Apache License, Version 2.0</a>;
// you may not use this file except in compliance with the License.
// --------------------------------------------------------------------------------------------------------------------


namespace LogJam.Microsoft.Extensions.Logging
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LogJam.Microsoft.Extensions.Logging.Format;
    using LogJam.Trace;
    using LogJam.Writer.Text;

    using Xunit;
    using Xunit.Abstractions;


    /// <summary>
    /// Validates <see cref="ReflectionFormatter"/> behavior.
    /// </summary>
    public sealed class ReflectionFormatterTests : IDisposable
    {

        private const string c_testClassFullName = "LogJam.Microsoft.Extensions.Logging.ReflectionFormatterTests";

        private readonly ITestOutputHelper _testOutput;

        private readonly SetupLog _setupLog;
        private readonly StringWriter _formatterOutput;
        private readonly TextWriterFormatWriter _formatWriter;
        private readonly ReflectionFormatter _reflectionFormatter;


        public ReflectionFormatterTests(ITestOutputHelper testOutput)
        {
            _testOutput = testOutput;

            _setupLog = new SetupLog();
            _formatterOutput = new StringWriter();
            _formatWriter = new TextWriterFormatWriter(_setupLog, _formatterOutput);
            _reflectionFormatter = new ReflectionFormatter
                                   {
                                       // Excluded for most tests, b/c type names make the output messy
                                       IncludeTypeNames = false
                                   };
        }

        public void Dispose()
        {
            Assert.All(_setupLog, entry =>
                                  {
                                      Assert.InRange(entry.TraceLevel, TraceLevel.Debug, TraceLevel.Info);
                                  });

            _testOutput.WriteLine(_formatterOutput.ToString());

            _formatWriter.Dispose();
        }

        [Fact]
        public void SimpleObjectIsFormattedOnSingleLine()
        {
            var anonymous = new
                            {
                                Three = 3,
                                DateTime.Now,
                                NotFalse = true
                            };

            _reflectionFormatter.FormatObject(anonymous, _formatWriter);

            Assert.DoesNotContain("\r\n", _formatterOutput.ToString().Trim());
        }

        public static TheoryData<object, string> CyclicReferenceTestCases()
        {
            var testCases = new TheoryData<object, string>();

            var a = new A();
            a.B = new B()
                  {
                      ParentA = a
                  };
            testCases.Add(a, "(Parent 1)");

            var c = new C();
            c.Child = c;
            testCases.Add(c, "(Parent 0)");

            return testCases;
        }

        [Theory]
        [MemberData(nameof(CyclicReferenceTestCases))]
        public void CyclicReferencesAreHandled(object objectToFormat, string expectedOutputSubstring)
        {
            _reflectionFormatter.FormatObject(objectToFormat, _formatWriter);
            Assert.Contains(expectedOutputSubstring, _formatterOutput.ToString());
        }

        [Fact]
        public void MaxDepthIsLimited()
        {
            // Build a list of Cs nested 20 deep to exceed the max limit
            var root = new C();
            var parent = root;
            for (int i = 0; i < 20; i++)
            {
                var next = new C();
                parent.Child = next;
                parent = next;
            }

            _reflectionFormatter.FormatObject(root, _formatWriter);
            Assert.Contains("(Recursion limit exceeded)", _formatterOutput.ToString());
        }

        [Fact]
        public void NonPublicPropertiesAreNotFormatted()
        {
            var c = new ClassWithNonPublicProperties();

            _reflectionFormatter.FormatObject(c, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.DoesNotContain("PrivateProperty:", output);
            Assert.DoesNotContain("ProtectedProperty:", output);
            Assert.DoesNotContain("InternalProperty:", output);
        }

        [Fact]
        public void SubObjectsAreIndentedForComprehension()
        {
            // Build a list of Cs nested 3 deep
            var root = new C();
            var parent = root;
            for (int i = 0; i < 3; i++)
            {
                var next = new C();
                parent.Child = next;
                parent = next;
            }

            _reflectionFormatter.FormatObject(root, _formatWriter);

            Assert.Equal(
@"{  Child:  {
      Child:  {
         Child:  {
            Child:(null)
         }
      }
   }
}", _formatterOutput.ToString());
        }

        [Fact]
        public void ObjectTypeIsOmittedWhenSameAsPropertyType()
        {
            var a = new A
                    {
                        // DerivedFromB != B, so should be displayed
                        B = new B()
                    };
            a.B.ParentA = a;

            _reflectionFormatter.IncludeTypeNames = true;
            _reflectionFormatter.FormatObject(a, _formatWriter);

            string output = _formatterOutput.ToString();
            // Type of top-level object is included
            Assert.Contains("type:" + c_testClassFullName + ".A", output);
            Assert.DoesNotContain("type:" + c_testClassFullName + ".B", output);
        }

        [Fact]
        public void ObjectTypeIsIncludedWhenNotSameAsPropertyType()
        {
            var a = new A
                    {
                        // DerivedFromB != B, so should be displayed
                        B = new DerivedFromB()
                    };
            a.B.ParentA = a;

            _reflectionFormatter.IncludeTypeNames = true;
            _reflectionFormatter.FormatObject(a, _formatWriter);

            Assert.Contains("type:" + c_testClassFullName + ".DerivedFromB", _formatterOutput.ToString());
        }

        [Fact]
        public void PrimitivePropertiesAreFormattedInline()
        {
            var c = new ClassWithPrimitiveProperties
                    {
                        DateTimeProperty = DateTime.Parse("12/31/2017"),
                        IntProperty = 10000,
                        StringProperty = "foo"
                    };

            _reflectionFormatter.FormatObject(c, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.DoesNotContain("\r\n", output.Trim());
            Assert.Contains("IntProperty:10000", output);
            Assert.Contains("StringProperty:foo", output);
            Assert.Contains("DateTimeProperty:\"12/31/2017 12:00:00 AM\"", output);
        }

        [Fact]
        public void QuotesInPropertyValuesAreEscaped()
        {
            var c = new ClassWithPrimitiveProperties
                    {
                        StringProperty = "She said, \"Hello!\""
                    };

            _reflectionFormatter.FormatObject(c, _formatWriter);

            Assert.Contains("StringProperty:\"She said, \"\"Hello!\"\"\"", _formatterOutput.ToString());
        }

        [Fact]
        public void ObjectPropertiesAreFormattedAsSubObjects()
        {
            var w = new Wrapper
                    {
                        SubObject = new ClassWithPrimitiveProperties
                                    {
                                        DateTimeProperty = DateTime.Parse("12/31/2017"),
                                        IntProperty = 10000,
                                        StringProperty = "foo"
                                    }
                    };

            _reflectionFormatter.FormatObject(w, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.Contains("SubObject:  {\r\n", output);
        }

        [Fact]
        public void PrimitiveKeyValuePairsAreFormattedCorrectly()
        {
            var kvps = new List<KeyValuePair<string, int>>
                       {
                           new KeyValuePair<string, int>("one", 1),
                           new KeyValuePair<string, int>("two", 2)
                       };

            _reflectionFormatter.FormatObject(kvps, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.Contains("{  one:1  },\r\n", output);
            Assert.Contains("{  two:2  }\r\n", output);
        }

        [Fact]
        public void DictionaryWithObjectKeysAreFormattedCorrectly()
        {
            var dictionary = new Dictionary<A, int>
                             {
                                 {
                                     new A { B = new B() },
                                     1
                                 },
                                 {
                                     new A { B = new B() },
                                     2
                                 }
                             };

            _reflectionFormatter.FormatObject(dictionary, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.Contains("Key:  {\r\n", output);
            Assert.Contains("Value:1  },\r\n", output);
            Assert.Contains("Value:2  }\r\n", output);
        }

        [Fact]
        public void DictionaryWithObjectValuesAreFormattedCorrectly()
        {
            var dictionary = new Dictionary<string, A>
                             {
                                 {
                                     "a1",
                                     new A { B = new B() }
                                 },
                                 {
                                     "a2",
                                     new A { B = new B() }
                                 }
                             };

            _reflectionFormatter.FormatObject(dictionary, _formatWriter);

            string output = _formatterOutput.ToString();
            Assert.Contains("{  a1:  {\r\n", output);
            Assert.Contains("{  a2:  {\r\n", output);
        }


        [Fact]
        public void CollectionsAreFormattedAsArray()
        {
            var elt = new ClassWithPrimitiveProperties
                      {
                          IntProperty = 2,
                          StringProperty = "two"
                      };
            var list = new List<ClassWithPrimitiveProperties>();
            list.Add(elt);
            list.Add(elt);

            _reflectionFormatter.FormatObject(list, _formatWriter);

            string output = _formatterOutput.ToString().Trim();
            Assert.StartsWith("[", output);
            Assert.EndsWith("]", output);
        }

        [Fact]
        public void PrimitiveArrayIsOnSingleLine()
        {
            int[] intArray = Enumerable.Range(5, 5).ToArray();

            _reflectionFormatter.FormatObject(intArray, _formatWriter);

            Assert.Contains("[5,6,7,8,9  ]", _formatterOutput.ToString());
        }

        [Fact]
        public void CollectionPropertiesAreFormattedAsArray()
        {
            var w = new Wrapper
                    {
                        SubObject = new ClassWithCollectionProperty
                                    {
                                        IntSet = new HashSet<int>(Enumerable.Range(0, 5))
                                    }
                    };

            _reflectionFormatter.FormatObject(w, _formatWriter);

            Assert.Contains("IntSet:  [0,1,2,3,4  ]", _formatterOutput.ToString());
        }

        [Fact]
        public void EmptyCollectionIsFormattedAsEmptyArray()
        {
            _reflectionFormatter.FormatObject(new A[0], _formatWriter);

            string output = _formatterOutput.ToString().Trim();
            Assert.Equal("[]", output);
        }

        [Theory]
        [InlineData(1, "1")]
        [InlineData("string with spaces", "\"string with spaces\"")]
        [InlineData("\"quoted\"", "\"\"\"quoted\"\"\"")]
        [InlineData("\"quoted with spaces\"", "\"\"\"quoted with spaces\"\"\"")]
        public void PrimitivesAreFormatted(object primitive, string expectedFormatOutput)
        {
            _reflectionFormatter.FormatObject(primitive, _formatWriter);
            Assert.Contains(expectedFormatOutput, _formatterOutput.ToString());
        }

        private class A
        {

            public B B { get; set; }

        }


        private class B
        {

            public A ParentA { get; set; }

        }

        private class DerivedFromB : B
        {

        }


        private class C
        {

            public C Child { get; set; }

        }


        private class ClassWithNonPublicProperties
        {

            private int PrivateProperty { get; set; }

            protected int ProtectedProperty { get; set; }

            internal int InternalProperty { get; set; }

        }


        private class ClassWithPrimitiveProperties
        {

            public int IntProperty { get; set; }

            public string StringProperty { get; set; }

            public DateTime DateTimeProperty { get; set; }

        }


        private class Wrapper
        {

            public object SubObject { get; set; }

        }

        private class ClassWithCollectionProperty
        {

            public ISet<int> IntSet { get; set; }

        }

    }

}
