using LightPath.Cargo.Tests.Unit.Common;
using Xunit;

namespace LightPath.Cargo.Tests.Unit
{
    public class Bus
    {
        [Fact]
        public void Instantiation()
        {
            var bus = Cargo.Bus.New<ContentModel1>();

            Assert.NotNull(bus);
            Assert.Null(bus.Package);
        }
    }
}
