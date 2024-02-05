using System.Text;

namespace Common.Books.Extensions;

public static class StringBuilderExtensions
{
    public static StringBuilder AppendWithNewLine(this StringBuilder sb, string text)
    {
        sb.Append(text);
        sb.AddNewLine();
        return sb;
    }

    private static void AddNewLine(this StringBuilder sb)
    {
        sb.Append('\n');
    }
}