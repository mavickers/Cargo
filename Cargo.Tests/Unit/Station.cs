namespace Cargo.Tests.Unit
{
    public class Station
    {
        public class NewPackageModel
        {
            public int Val1 { get; set; }
        }

        public class Station1 : Cargo.Station<NewPackageModel>
        {
            public override void Process()
            {
                Contents.Val1 += 1;
            }
        }

        public void Instantiation()
        {
            var packageModel = new NewPackageModel { Val1 = 0 };
            var package = new Cargo.Package<NewPackageModel>(packageModel);
            

            var bus = 
                Cargo.Bus.New<NewPackageModel>()
                    .WithStation(typeof(Station1));
        }
    }
}
