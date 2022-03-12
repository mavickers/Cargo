using LightPath.Cargo.Tests.Unit.Common;
using Xunit;

namespace LightPath.Cargo.Tests.Unit
{
    public class Bus
    {
        [Fact]
        public void Instantiation()
        {
            var bus1 = Cargo.Bus.New<ContentModel1>();

            Assert.NotNull(bus1);
            Assert.Null(bus1.Package);

            var bus2 = Cargo.Bus.New<Interface1>();

            Assert.NotNull(bus2);
            Assert.Null(bus2.Package);
        }
    }
}
