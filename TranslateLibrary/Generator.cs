using System.Text;

namespace TranslateLibrary.CoreLib;
public class CodeGenerator
{
    public static string GenCode(Node[] NodeTree, PostGenerationOptimizingT Opt)
    {
        StringBuilder Resb = new StringBuilder(10000);

        foreach (var item in NodeTree)
        {
            Resb.Append(item.MyToString(Opt)+ "\n");
        }
        return Resb.ToString();
    }
}