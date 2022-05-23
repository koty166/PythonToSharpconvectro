using System.Text;

namespace TranslateLibrary.CoreLib;
public class CodeGenerator
{
    public static string GenCode(Node[] NodeTree)
    {
        StringBuilder Resb = new StringBuilder(10000);

        foreach (var item in NodeTree)
        {
            Resb.Append(item.ToString()+ "\n");
        }
        return Resb.ToString();
    }
}