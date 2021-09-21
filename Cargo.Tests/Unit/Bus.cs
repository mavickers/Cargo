using Xunit;

namespace Cargo.Tests.Unit
{
    public class Bus
    {
        [Fact]
        public void Instantiation()
        {
            var bus = new Cargo.Bus();


            Assert.NotNull(bus);

        }
    }
}
