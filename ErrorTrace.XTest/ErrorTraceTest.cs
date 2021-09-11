using System;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Xunit.Abstractions;

namespace ErrorTrace.XTest
{
    public class ErrorTraceTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public ErrorTraceTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void NullErrorTest()
        {
            string result = ErrorTrace.DebuggingErrorFormat(null);

            Assert.True(string.IsNullOrEmpty(result));
        }

        [Fact]
        public void ErrorFormatNoInnerTest()
        {
            // ReSharper disable once NotResolvedInText
            Exception err = new ArgumentException("error_level_one", "param1", null);

            string result = ErrorTrace.DebuggingErrorFormat(err);

            _testOutputHelper.WriteLine("\nExpected result;\n{0}", result);

            Assert.False(Regex.IsMatch(result, @":\d+"), "Expected no line number in result");
        }

        [Fact]
        public void SimpleStackTraceTest()
        {
            try
            {
                // ReSharper disable once NotResolvedInText
                throw new ArgumentException("error_level_one", "param2", null);
                //Assert.Fail("Expected thrown error to be caught");
            }
            catch (ArgumentException err)
            {
                var result = ErrorTrace.DebuggingErrorFormat(err);

                _testOutputHelper.WriteLine("\nExpected trace result:\n{0}", result);

                Assert.True(Regex.IsMatch(result, @":\d+"),
                    "Expected result to contain a line number someplace");
            }

        }

        [Fact]
        public void InnerExceptionTest()
        {
            Exception inner = new ArgumentException("inner_exception");

            Exception err = new FieldAccessException("final_exception", inner);

            string result = ErrorTrace.DebuggingErrorFormat(err);

            _testOutputHelper.WriteLine("\nInnerTestNoTrace\n{0}", err);

            Assert.False(Regex.IsMatch(result, @"\(\d+\)"), "Expected NO line number in result");
        }

        [Fact]
        public void InnerExceptionStackTraceTest()
        {
            try
            {
                throw new ArgumentException("Inner-Exception-text");
            }
            catch (ArgumentException aException)
            {
                try
                {
                    throw new FieldAccessException("Final-exception-text", aException);
                }
                catch (FieldAccessException faException)
                {
                    string result = ErrorTrace.DebuggingErrorFormat(faException);

                    _testOutputHelper.WriteLine("\nInnerExceptionTraceTest;\n{0}", result);

                    Assert.True(Regex.IsMatch(result, @":\d+"), "Expected line number in result");

                    var two = result.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    _testOutputHelper.WriteLine(two.Length.ToString());

                    var test =
                        from a in two
                        where Regex.IsMatch(a, @":\d+")
                        select a;

                    Assert.True(test.Count() == 2);
                }
            }
        }


        [Fact]
        public void InnerErrorCounterTest()
        {
            // Create an error with several inner exceptions and test that the ErrorTrace counts correctly

            Assert.Equal(0, ErrorTrace.CountInnerDepth(null)); //, " null test");

            var err = new ArgumentException("one");

            Assert.Equal(1, ErrorTrace.CountInnerDepth(err)); //, "no inner test");

            var err2 = new ArgumentException("two", err);

            Assert.Equal(2, ErrorTrace.CountInnerDepth(err2)); //, "one inner test");

            var err3 = new ArgumentException("three", err2);

            Assert.Equal(3, ErrorTrace.CountInnerDepth(err3)); //, "two inner test");

        }

        private static void RecursiveError(Exception err)
        {
            try
            {
                var depth = ErrorTrace.CountInnerDepth(err);

                if (depth < 21) // Limit of my nest test
                    throw new ArgumentException($"level-{depth}", err);
            }
            catch (ArgumentException newException)
            {
                RecursiveError(newException);
            }

            throw new ArgumentException("Final-Exception", err);
        }

        [Fact]
        public void DeepNestedErrorTest()
        {
            // ErrorTrace class has a recursion depth limiter
            // This test validates that that limit is still active to help prevent incorrect configuration
            // Create nested error until it fails and output result
            //  It is supposed to stop running at 21 nested inner errors
            try
            {
                RecursiveError(null);
                throw new ArgumentException("Expected recursive error did not occur");
            }
            catch (ArgumentException err)
            {
                string result = ErrorTrace.DebuggingErrorFormat(err);

                _testOutputHelper.WriteLine("\nInnerExceptionTraceTest;\n{0}", result);

                Assert.True(Regex.IsMatch(result, @":\d+"), "Expected line number in result");

                var two = result.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                var test =
                    from a in two
                    where Regex.IsMatch(a, @":\d+")
                    select a;

                var enumerable = test as string[] ?? test.ToArray();
                var testCount = enumerable.Length;

                if (enumerable.Length != 22)
                {
                    _testOutputHelper.WriteLine($"Expected 22 fount {testCount}");
                }

                Assert.Equal(22, enumerable.Length); //, String.Format("Expected 22 found {0}", test.Count<string>()));

                Assert.True(Regex.IsMatch(result,
    @"nested\s+inner exceptions\s+are\s+not\s+shown"), "Expected not all errors shown");

            }
        }
    }
}
