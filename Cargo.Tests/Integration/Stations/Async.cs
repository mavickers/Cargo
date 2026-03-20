using System;
using System.Threading.Tasks;
using LightPath.Cargo.Tests.Integration.Common;
using Xunit;
using static LightPath.Cargo.Tests.Integration.Stations.Services;

namespace LightPath.Cargo.Tests.Integration.Stations
{
    public class Async
    {
        public class AsyncAdder : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                Package.Contents.IntVal += 2;

                return Station.Action.Next();
            }
        }

        public class AsyncAdderWithDelay : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.Delay(10);
                Package.Contents.IntVal += 4;

                return Station.Action.Next();
            }
        }

        public class AsyncThrower : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Async station error");
            }
        }

        public class AsyncAborter : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                return Station.Action.Abort();
            }
        }

        public class AsyncRepeater : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                Package.Contents.IntVal += 1;

                return Package.Contents.IntVal < 5 ? Station.Action.Repeat() : Station.Action.Next();
            }
        }

        public class AsyncServiceUser : StationAsync<ContentModel1>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                var service = GetService<Interface1>();

                if (service == null) throw new Exception("Could not locate service");

                Package.Contents.Int1 = service.AddThree(Package.Contents.Int1);

                return Station.Action.Next();
            }
        }

        public class AsyncFinalStation : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                Package.Contents.IntVal += 100;

                return Station.Action.Next();
            }
        }

        public class AsyncResultChecker : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                var lastMessage = LastResult.ActionMessage;

                Assert.Equal("sync-message", lastMessage);
                Package.Contents.IntVal += 10;

                return Station.Action.Next();
            }
        }

        public class AsyncMessageSender : StationAsync<ContentModel2>
        {
            public override async Task<Station.Action> ProcessAsync()
            {
                await Task.CompletedTask;
                Package.Contents.IntVal += 1;

                return Station.Action.Next("async-message");
            }
        }

        // Sync stations used in mixed tests (ContentModel2)

        public class SyncAdder1 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 1;
                return Station.Action.Next();
            }
        }

        public class SyncAdder3 : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 3;
                return Station.Action.Next();
            }
        }

        public class SyncMessageSender : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Next("sync-message");
            }
        }

        public class SyncResultChecker : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                var lastMessage = LastResult.ActionMessage;

                Assert.Equal("async-message", lastMessage);
                Package.Contents.IntVal += 10;

                return Station.Action.Next();
            }
        }

        public class SyncAborter : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                return Station.Action.Abort();
            }
        }

        public class SyncFinalStation : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 100;
                return Station.Action.Next();
            }
        }

        public class SyncServiceUser : Station<ContentModel1>
        {
            public override Station.Action Process()
            {
                var service = GetService<Interface1>();

                if (service == null) throw new Exception("Could not locate service");

                Package.Contents.Int1 = service.AddThree(Package.Contents.Int1);

                return Station.Action.Next();
            }
        }

        public class SyncThrower : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                throw new InvalidOperationException("Sync station error");
            }
        }

        public class SyncCanceller : Station<ContentModel2>
        {
            public override Station.Action Process()
            {
                Package.Contents.IntVal += 1;
                return Station.Action.Next();
            }
        }
    }
}
