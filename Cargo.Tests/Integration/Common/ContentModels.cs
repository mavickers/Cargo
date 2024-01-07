namespace LightPath.Cargo.Tests.Integration.Common
{
    public abstract class ContentModelAbstract1 { }
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

    public interface IContentModel3
    {
        string String1 { get; set; }
        string String2 { get; set; }
        string String3 { get; set; }
        int Int1 { get; set; }
    }

    public class ContentModel3 : IContentModel3
    {
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public string String4 { get; set; }
        public int Int1 { get; set; }
        public int Int2 { get; set; }
    }

    public class ContentModel4 : ContentModel1
    {
        public ContentModel4() : base() { }
    }

    public class ContentModel5 : ContentModelAbstract1
    {
        public ContentModel5() : base() { }
    }
}
