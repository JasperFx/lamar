namespace LamarCodeGeneration.Model
{
    public class StringConstant : Variable
    {
        public StringConstant(string value) : base(typeof(string), "\"" + value + "\"")
        {
        }
    }
}