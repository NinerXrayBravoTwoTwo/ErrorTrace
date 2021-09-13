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

            _testOutputHelper.WriteLine("\nExpected result{0}", result);

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
            catch (ArgumentException error)
            {
                var result = ErrorTrace.DebuggingErrorFormat(error);

                _testOutputHelper.WriteLine("\nExpected trace result:\n{0}", result);

                Assert.True(Regex.IsMatch(result, @":\d+"),
                    "Expected result to contain a line number someplace");
            }

        }

        [Fact]
        public void InnerExceptionTest()
        {
            Exception inner = new ArgumentException("inner_exception");

            Exception error = new FieldAccessException("final_exception", inner);

            string result = ErrorTrace.DebuggingErrorFormat(error);

            _testOutputHelper.WriteLine("\nInnerTestNoTrace:\n{0}", error);

            Assert.False(Regex.IsMatch(result, @":\d+"), "Expected NO line number in result");
        }

        [Fact]
        public void InnerExceptionStackTraceTest()
        {
            // Verify a nested exception formats correctly
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

                    _testOutputHelper.WriteLine("\nInnerExceptionTraceTest:\n{0}", result);

                    Assert.True(Regex.IsMatch(result, @":\d+"), "Expected line number in result");

                    // Expect two errors reported, the most recent and the first error
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
        public void InnerExceptionCountTest()
        {
            // Create an error with several inner exceptions and test that the ErrorTrace counts correctly

            Assert.Equal(0, ErrorTrace.InnerExceptionCount(null)); //, " null test");

            var error = new ArgumentException("one");

            Assert.Equal(1, ErrorTrace.InnerExceptionCount(error)); //, "no inner test");

            var error2 = new ArgumentException("two", error);

            Assert.Equal(2, ErrorTrace.InnerExceptionCount(error2)); //, "one inner test");

            var error3 = new ArgumentException("three", error2);

            Assert.Equal(3, ErrorTrace.InnerExceptionCount(error3)); //, "two inner test");

        }

        private static void RecursiveError(Exception error, int limit)
        {
            try
            {
                var depth = ErrorTrace.InnerExceptionCount(error);

                if (0 <= limit) // Limit of my nest test
                    if (limit % 2 == 0)
                        throw new ArgumentException($"level-{depth}", error);
                    else
                        throw new FieldAccessException($"level-{depth}", error);
            }
            catch (Exception newException)
            {
                RecursiveError(newException, limit - 1);
            }

            throw error;
        }

        [Fact]
        public void DeepNestedErrorTest()
        {
            // ErrorTrace class has a recursion depth limiter
            // Create nested error until it fails and output result
            // It is supposed to stop running 'limit' nested inner errors
            const int limit = 20;
            try
            {
                RecursiveError(new NullReferenceException("Initial exception - level-0"), limit - 2); // last error has null inner, first error is level 0, so limit - 2 is the correct depth result.
                throw new ArgumentException("Expected recursive error did not occur");
            }
            catch (ArgumentException error)
            {
                string result = ErrorTrace.DebuggingErrorFormat(error);

                _testOutputHelper.WriteLine("\nInnerExceptionTraceTest:\n{0}", result);

                Assert.True(Regex.IsMatch(result, @":\d+"), "Expected line number in result");

                Assert.Equal(limit, ErrorTrace.InnerExceptionCount(error));
            }
        }
    }
}
