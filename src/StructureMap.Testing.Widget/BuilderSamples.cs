namespace StructureMap.Testing.Widget
{
    public enum Color
    {
        Red,
        Blue,
        Green
    }

    public class SetterTarget
    {
        public string Name { get; set; }
        public string Name2 { get; set; }
        public bool Active { get; set; }
        public Color Color { get; set; }
    }

}