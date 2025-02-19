using LightPath.Cargo.Tests.Integration.Common;
using System;
using Xunit;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Services
    {
        public interface Interface1
        {
            int AddThree(int val);
        }

        public interface Interface2
        {
            int AddSeven(int val);
        }

        public class Implementation1 : Interface1
        {
            public int AddThree(int val)
            {
                return val + 3;
            }
        }

        public class Implementation2 : Interface2
        {
            public int AddSeven(int val)
            {
                return val + 7;
            }
        }

        public class Implementation3
        {
            public int AddOne(int val)
            {
                return val + 1;
            }
        }

        public class Station1 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 1;

                Assert.True(TryGetService<Interface1>(out _));
                Assert.False(TryGetService<string>(out _));

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Package.Contents.Int1 += 2;

                return Station.Action.Next();
            }
        }

        public class Station3A : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var service = GetService<Interface1>();

                if (service == null) throw new Exception("Could not locate service");

                Package.Contents.Int1 = service.AddThree(Package.Contents.Int1);

                return Station.Action.Next();
            }
        }

        public class Station3B : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var service = GetService<Implementation3>();

                if (service == null) throw new Exception("Could not locate service");

                Package.Contents.Int1 = service.AddOne(Package.Contents.Int1);

                return Station.Action.Next();
            }
        }

        public class Station4A : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var result1 = TryGetService<Interface1>(out var service);

                Assert.True(result1);
                Assert.NotNull(service);

                Package.Contents.Int1 = service.AddThree(Package.Contents.Int1);

                var result2 = TryGetService<Exception>(out var exception);

                Assert.False(result2);
                Assert.Null(exception);

                return Station.Action.Next();
            }
        }

        public class Station4B : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var result1 = TryGetService<Interface2>(out var service);

                Assert.True(result1);
                Assert.NotNull(service);

                Package.Contents.Int1 = service.AddSeven(Package.Contents.Int1);

                var result2 = TryGetService<Exception>(out var exception);

                Assert.False(result2);
                Assert.Null(exception);

                return Station.Action.Next();
            }
        }

        public class Station5 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var success = TryGetService<Types>(out var type);

                Assert.True(success);
                Assert.Equal(Types.Third, type);

                return Station.Action.Next();
            }
        }

        public enum Types
        {
            None = 0,
            First = 1,
            Second = 2,
            Third = 3
        }
    }
}
