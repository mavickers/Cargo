﻿using Cargo.Tests.Integration.Common;
using Xunit;

namespace Cargo.Tests.Integration
{
    public class Simple
    {
        [Fact]
        public void Scenario1()
        {
            var content = new ContentModel1();
            var bus = Cargo.Bus.New<ContentModel1>()
                               .WithStation<Stations.Simple.Station1>()
                               .WithStation<Stations.Simple.Station2>()
                               .WithStation<Stations.Simple.Station3>();

            bus.Go(content);

            Assert.Equal(6, content.Int1);
            Assert.Equal("1", content.String1);
            Assert.Equal("3", content.String2);
            Assert.Equal("6", content.String3);
            Assert.Equal(3, bus.Package.Results.Count);

            bus.Go(content);

            Assert.Equal(12, content.Int1);
            Assert.Equal("7", content.String1);
            Assert.Equal("9", content.String2);
            Assert.Equal("12", content.String3);
            Assert.Equal(3, bus.Package.Results.Count);

            Assert.False(bus.Package.IsAborted);
            Assert.False(bus.Package.IsErrored);
        }
    }
}