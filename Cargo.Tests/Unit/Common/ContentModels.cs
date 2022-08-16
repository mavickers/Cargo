namespace LightPath.Cargo.Tests.Unit.Common
{
    public interface Interface1
    {
        int TestValue();
    }

    public class Implementation1 : Interface1
    {
        public int TestValue()
        {
            return 5;
        }
    }

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
        public string String1 { get; set; }

        public ContentModel2(string string1)
        {
            String1 = string1;
        }
    }
}
