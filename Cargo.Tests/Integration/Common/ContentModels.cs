namespace LightPath.Cargo.Tests.Integration.Common
{
    public class ContentModel1
    {
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public int Int1 { get; set; }

        public ContentModel1()
        {
            String2 = string.Empty;
            String3 = "TESTING";
            Int1 = 0;
        }
    }

    public class ContentModel2
    {
        public int IntVal { get; set; } = 0;
    }
}
