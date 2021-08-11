using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static Cargo.Tests.UnitTest1.Stations;

namespace Cargo.Tests
{
    [TestClass]
    public class UnitTest1
    {
        public class Test
        {
            public string Text { get; set; }
        }

        public class First : Station<Test>
        {
            public override void Process()
            {
                Skip();

                Contents.Text += " then first";
            }
        }

        public class Second : Station<Test>
        {
            public override void Process()
            {
                Contents.Text += " and second";
            }
        }

        public class Third : Station<Test>
        {
            public override void Process()
            {
                Contents.Text += " and third";
            }
        }

        public class Done : Station<Test>
        {
            public override void Process()
            {
                Contents.Text += " and done!";
            }
        }

        public struct Stations
        {
            public static Type FirstStation => typeof(First);
            public static Type SecondStation => typeof(Second);
            public static Type ThirdStation => typeof(Third);
            public static Type LastStation => typeof(Done);
        }

        [TestMethod]
        public void TestMethod1()
        {
            var bus = Bus.New<Test>().WithStations(FirstStation, SecondStation, ThirdStation).WithFinalStation(LastStation);
            var package = new Test {Text = "empty"};

            bus.Go(package);

            Console.WriteLine(package.Text);

            bus.Package.Results.ForEach(p => Console.WriteLine($"{p.Station.Name}: {p.Exception?.Message ?? "no result msg"}"));
            Console.WriteLine(bus.Package.AbortedWith?.Message ?? "no abort msg");
        }
    }
}
