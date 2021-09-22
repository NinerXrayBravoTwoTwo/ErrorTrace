using Xunit;
using Xunit.Abstractions;

namespace ErrorTrace.XTest
{
    public class StopWatchTest
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public StopWatchTest(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void StartStop()
        {
            var sw = new StopWatch("started timer: ");
            System.Threading.Thread.Sleep(2000);
            sw.AddComment($"Half way: {sw.Value.TotalSeconds}");
            System.Threading.Thread.Sleep(2000);
            sw.Stop($"Stopped: {sw.Value.TotalSeconds}");

            _testOutputHelper.WriteLine( $">>> {sw.ToString("\n")}");

            Assert.Equal(3, sw.CountComments);
        }

    }
}
