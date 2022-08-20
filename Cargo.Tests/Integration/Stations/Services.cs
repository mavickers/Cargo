using LightPath.Cargo.Tests.Integration.Common;
using System;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Services
    {
        public interface Interface1
        {
            int AddThree(int val);
        }

        public class Implementation1 : Interface1
        {
            public int AddThree(int val)
            {
                return val + 3;
            }
        }

        public class Station1 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 1;

                return Station.Action.Next();
            }
        }

        public class Station2 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                Contents.Int1 += 2;

                return Station.Action.Next();
            }
        }

        public class Station3 : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var service = GetService<Interface1>();

                if (service == null) throw new Exception("Could not locate service");

                Contents.Int1 = service.AddThree(Contents.Int1);

                return Station.Action.Next();
            }
        }
    }
}
