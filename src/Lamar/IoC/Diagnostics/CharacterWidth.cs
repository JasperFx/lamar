namespace Lamar.IoC.Diagnostics;

internal class CharacterWidth
{
    internal int Width { get; private set; }

    internal static CharacterWidth[] For(int count)
    {
        var widths = new CharacterWidth[count];
        for (var i = 0; i < widths.Length; i++)
        {
            widths[i] = new CharacterWidth();
        }

        return widths;
    }

    internal void SetWidth(int width)
    {
        if (width > Width)
        {
            Width = width;
        }
    }

    internal void Add(int add)
    {
        Width += add;
    }
}