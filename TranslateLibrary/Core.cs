using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

using System.Text.RegularExpressions;

namespace TranslateLibrary;

public class Core
{
    public async Task<string> StartTranslate(string SourceCode)
    {
        string ResaultCode = string.Empty;
        return ResaultCode;
    }
    /// <summary>
    /// Основной метод трансляции кода. В обоих случаях сохраняется форматирование.
    /// </summary>
    /// <param name="SourceCode">Исходный код на Python</param>
    /// <returns>Код на c#</returns>
    public string Translate(string SourceCode)
    {
        return string.Empty;
        
    }
    static string[] Split(String s,char[] Separator, int MaxReturns,out char? RSeparator, StringSplitOptions SplitOption = StringSplitOptions.None)
    {
        string[] SubStrings = new string[0];
        foreach (var item in Separator)
        {
            if(!s.Contains(item))
                continue;
            SubStrings  = s.Split(item,MaxReturns,SplitOption);
            if(SubStrings.Length != 0)
            {
                RSeparator = item;
                return SubStrings;
            }
            
        }
        RSeparator = null;
        return new string[]{s};
    }
    static List<MyVar> Vars = new List<MyVar>();
    static  List<MyFunc> Funcs = new List<MyFunc>();

    static Int64 GetUID()
    {
       // byte[] UIDB = new byte[4];
        //Random.Shared.NextBytes(UIDB);

        return new Random().NextInt64();
    }
    public static List<NodeTree> ND = new List<NodeTree>();

    static NodeTypes GetNodeType(ref string Target)
    {
        string t = Target;
        NodeTypes NodeType = NodeTypes.NONE;
        IEnumerable<MyVar> VarRes = from cur in Vars
                                    where cur.name == t
                                    select cur;

        if(VarRes.Count<MyVar>() != 0)
        {
            NodeType = NodeTypes.VAR;
        }
        else if(Regex.Matches(Target,@"\(.*[\d\w]*.*\)").Count != 0)
        {
            NodeType = NodeTypes.CALL;
        }
        else if(Regex.Matches(Target,"[\"'].*[\\d\\w]*.*[\"']").Count != 0 || Regex.Matches(Target,@"\d+").Count != 0)
        {
            NodeType = NodeTypes.CONST;
        }
        else if(Target.Contains("import"))
        {
            NodeType = NodeTypes.IMPORT;
            Target = Target.Split(' ')[1];
        }
        else
        {
            NodeType = NodeTypes.NVAR;
            Vars.Add(new MyVar(){name = Target}); 
        }   
        return NodeType;
    }
    /// <summary>
    /// Построение древа для линейного выражения.
    /// </summary>
    /// <param name="Line"></param>
    /// <param name="IsLeft"></param>
    /// <param name="ParentUID"></param>
    /// <returns></returns>
    public static Node SeparateLinear(string Line, bool IsLeft,int i, Int64 ParentUID)
    {
        char[] Separators = "=*/+-.".ToCharArray();
        string[] Sublines = Split(Line,Separators,2, out char? RSeparator,StringSplitOptions.TrimEntries);


        Node CurNode = new Node();
        CurNode.ParentUID = ParentUID;
        CurNode.UID = GetUID();
        
        if(RSeparator != null)
        {
            CurNode.NodeType = NodeTypes.CALL;
            CurNode.Target = RSeparator.ToString();
        }
        if(RSeparator ==  '=')
        {
            CurNode.NodeType = NodeTypes.EQUALS;
            CurNode.Target = null;
        }
        
        Node FirstChild = null, SecoundChild = null;
        if(Sublines.Length != 1)
        {
            FirstChild = SeparateLinear(Sublines[0].Trim(),true,i,CurNode.UID);
            SecoundChild = SeparateLinear(Sublines[1].Trim(),false,i, CurNode.UID);
        }
        else
        {
            CurNode.ChildNodes = null;
            CurNode.Target = Sublines[0];
            CurNode.NodeType = GetNodeType(ref CurNode.Target);
        }
        ND[i].Nodes.Add(CurNode);
        CurNode.ChildNodes = new Node[]{FirstChild,SecoundChild};
        return CurNode;
    }
    static string[] Normalazing(string[] SourceCodeLines)
    {
        List<string> Lines = SourceCodeLines.ToList<string>();
        int CurTabsNum = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            int TabsNum = Lines[i].Count((ch) => ch == '\t');
            if(TabsNum > CurTabsNum)
                Lines.Insert(i,NodeTypes.START.ToString());
            else if(TabsNum < CurTabsNum)
                Lines.Insert(i,NodeTypes.END.ToString());
            CurTabsNum = TabsNum;
            if(Lines[i].Contains('#'))
                Lines.RemoveAt(i);
        }
        return Lines.ToArray();
    }
   
    public static String GetTokenizatedText(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        Lines = Normalazing(Lines);
        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
            ND.Add(new NodeTree());

            if
            switch(Lines[i])
            {
                case "START":
                    ND[i].Nodes.Add(new Node(){NodeType=NodeTypes.START, Target = ""});
                    break;
                case "END":
                    ND[i].Nodes.Add(new Node(){NodeType=NodeTypes.END, Target = ""});
                    break;
                case "continue":
                    ND[i].Nodes.Add(new Node(){NodeType=NodeTypes.CONTINUE, Target = ""});
                    break;
                default:
                    SeparateLinear(Lines[i],false,i,i);
                    break;
            }                
        }
        return null;
    }
    /*public static String GetTokenizatedText(string SourceCode)
    {
        List<MyVar> Vars;
        List<MyFunc> Funcs;
        SourceCode+="\n";// Костыль
        Vars = new List<MyVar>();
        Funcs = new List<MyFunc>();

        SourceCode = Regex.Replace(SourceCode,"import [\\w.]+", (Match m) =>
                                                            {
                                                                string[] words = m.Value.Split(' ');
                                                                return "IMPORT("+words[1]+")";
                                                            });

        SourceCode = Regex.Replace(SourceCode,"[\\d\\w +-\\/*]+=(?!=).+[\n\0]", (Match m) =>
                                                            {
                                                                string[] words = m.Value.Split('=');
                                                                MatchCollection paramss = Regex.Matches(words[1],"\".+[\\d\\w]+.+\"");
                                                                string bufs = Regex.Replace(words[1].Trim(),"\".+[\\d\\w]+.+\"","^");

                                                                bufs = Regex.Replace(bufs,"[\\d\\w]+",(Match m) => "CALL("+m.Value+")");
                                                                bufs = Regex.Replace(bufs,"\\.+","SUB");
                                                                int n = 0;
                                                                bufs = Regex.Replace(bufs,"\\(\\^\\)",(Match m) => "PARAMS("+paramss[n++].Value+")");
                                                                bufs =bufs.Replace("()","");
                                                                return "NVAR("+words[0].Trim()+")" + "EQUALS"+bufs+"\n";
                                                            });
        return SourceCode;
    }*/
}
// regex^
// [\d\w]+\([\S ]*\) - функция, включая if
// [\d\w +-\/*]+=(?!=) - присвавание
