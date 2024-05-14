using System.Text;

namespace TranslateLibrary.CoreLib;
public class CodeGenerator
{
    static public Node[] Nodes;
    public static string GenReport(Node[] NodeTree)
    {
        Nodes = NodeTree;
        StringBuilder Resb = new StringBuilder(10000);
        string CaclRes = String.Empty;
        bool IsSkip = false;
        foreach (var item in NodeTree)
        {
            CaclRes = item.Calculate(null);
            if(CaclRes == "SKIP")
            {
                IsSkip = true;
                continue;
            }
            else if(CaclRes == "CONTINUE")
            { 
                IsSkip = false;
                continue;
            }
            if(IsSkip) continue;
            Resb.Append(CaclRes + "\n");
        }
        Node.Vars.Clear();
        return Resb.ToString();
    }
}