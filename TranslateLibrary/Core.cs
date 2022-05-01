using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

using System.Diagnostics;
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
    //Можеит заменить на список регулярок?
    static string[] ImportantPreferenses = new string[] {"=","+=","-=","*=","/=","==","+","-","*","/","in","."};
    static int GetMostImportantMatchNumber(MatchCollection mc)
    {
        foreach (var item in ImportantPreferenses)
            for (int i = 0; i < mc.Count; i++)
                if(mc[i].Value == item)
                    return i;
        return 0;
    }
    static string[] Split(String s,out string? RSeparator)
    {
        Regex r = new Regex(@"[=*\/+-.]+| in ");

        string[] SubStrings =  r.Split(s);
        if(SubStrings.Length <= 1)
        {
            RSeparator = null;
            return new string[]{s};
        }
        MatchCollection Separators = r.Matches(s);
        int MostImportantSeparator = GetMostImportantMatchNumber(Separators);

        RSeparator = Separators[MostImportantSeparator].Value;

        string LeftRes = s.Substring(0,Separators[MostImportantSeparator].Index);
        string RightRes = s.Substring(Separators[MostImportantSeparator].Index + Separators[MostImportantSeparator].Length);


        return new string[] {LeftRes,RightRes};
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
    static List<string> BrecketsColl = new List<string>();
    public static void RepaceBreckets(ref string Line)
    {
        string LocalCopyOfTheLine = Line;
        int CurrBrecketsCollIndex = BrecketsColl.Count - 1;

        List<int> OpenedBrecketsIndexes = new List<int>();
        for (int i = 0; i < Line.Length; i++)
        {
            char c = Line[i];
            if(Line[i] == '(')
                OpenedBrecketsIndexes.Add(i);
            else if(Line[i]== ')')
            {
                if(OpenedBrecketsIndexes.Count == 0)
                {
                    Console.WriteLine("ERROR");
                    continue;
                }

                CurrBrecketsCollIndex++;
                string BufSubLine = Line.Substring(OpenedBrecketsIndexes.Last(),i - OpenedBrecketsIndexes.Last() +1);
                Line = Line.Replace(BufSubLine,$"#{CurrBrecketsCollIndex}");
                BrecketsColl.Add(BufSubLine);
                OpenedBrecketsIndexes.RemoveAt(OpenedBrecketsIndexes.Count-1);
                i-= BufSubLine.Length;
            }
        }
    }
    /// <summary>
    /// Построение древа для линейного выражения.
    /// </summary>
    /// <param name="Line"></param>
    /// <param name="IsLeft"></param>
    /// <param name="ParentUID"></param>
    /// <returns></returns>
    public static Node SeparateLinear(string Line,int CurGlobalNDPos, Int64 ParentUID)
    {   

        Node CurNode = new Node(GetUID(),ParentUID);

        if(Line == String.Empty)
        {
            CurNode.NodeType = NodeTypes.NONE;
            return CurNode;
        }
        string[] Sublines = Split(Line,out string? RSeparator);
        if(Sublines.Length == 1 && Sublines[0].StartsWith('#'))
        {
            int BracketCollIndex = int.Parse(Sublines[0].TrimStart('#'));
            string NormalazedLine = BrecketsColl[BracketCollIndex].Trim("()".ToCharArray());
            return SeparateLinear(NormalazedLine,CurGlobalNDPos,ParentUID);
        }

        if(RSeparator != null)
        {
            CurNode.NodeType = NodeTypes.CALL;
            CurNode.Target = RSeparator;
        }
        if(RSeparator ==  "=")
        {
            CurNode.NodeType = NodeTypes.EQUALS;
            CurNode.Target = String.Empty;
        }
        
        Node? FirstChild = null, SecoundChild = null;
        if(Sublines.Length != 1)
        {
            FirstChild = SeparateLinear(Sublines[0].Trim(),CurGlobalNDPos,CurNode.UID);
            SecoundChild = SeparateLinear(Sublines[1].Trim(),CurGlobalNDPos, CurNode.UID);
            CurNode.ChildNodes = new Node[]{FirstChild,SecoundChild};
        }
        else
        {
            Match FuncCallRegexResault = Regex.Match(Sublines[0],@"#[\d]+");
            if(FuncCallRegexResault.Value != String.Empty)
            {
                int BracketCollIndex = int.Parse(FuncCallRegexResault.Value.TrimStart('#'));
                string[] Params = BrecketsColl[BracketCollIndex].Trim("()".ToCharArray()).Split(',');

                CurNode.NodeType = NodeTypes.CALL;
                CurNode.Target = Sublines[0].Substring(0,FuncCallRegexResault.Index);
                CurNode.ChildNodes = new Node[Params.Length];

                ND[CurGlobalNDPos].Nodes.Add(CurNode);

                for (int i = 0; i < Params.Length; i++)
                    CurNode.ChildNodes[i] = SeparateLinear(Params[i].Trim(),CurGlobalNDPos,CurNode.UID);
                return CurNode;
            }
            CurNode.ChildNodes = null;
            CurNode.Target = Sublines[0];
            CurNode.NodeType = GetNodeType(ref CurNode.Target);
        }
        ND[CurGlobalNDPos].Nodes.Add(CurNode);
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
            Lines[i] = Lines[i].Trim();
        }
        return Lines.ToArray();
    }
    
    static Node IfConstructionSeparator(string Line,int CurGlobalNDPos, Int64 ParentUID)
    {
        string NormalizedIfConstruction = Regex.Match(Line,@"(?<=if)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
        Node CurNode =  new Node(GetUID(),ParentUID,NodeTypes.IF);
        ND[CurGlobalNDPos].Nodes.Add(CurNode);
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID);
        return CurNode;
    }
    static Node ForConstructionSeparator(string Line,bool IsFor,int CurGlobalNDPos, Int64 ParentUID)
    {
        Node CurNode =  new Node(GetUID(),ParentUID);
        
        string NormalizedIfConstruction = String.Empty;
        if(IsFor)
        {
            NormalizedIfConstruction = Regex.Match(Line,@"(?<=for)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
            CurNode.NodeType = NodeTypes.FOR;
        }
        else
        {
            NormalizedIfConstruction = Regex.Match(Line,@"(?<=while)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
            CurNode.NodeType = NodeTypes.WHILE;
        }
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID);
        ND[CurGlobalNDPos].Nodes.Add(CurNode);

        return CurNode;
    }
    public static void GetTokenizatedText(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        Lines = Normalazing(Lines);

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
            ND.Add(new NodeTree());
            RepaceBreckets(ref Lines[i]);
            if(Lines[i].StartsWith("if"))
            {
                IfConstructionSeparator(Lines[i],i,i);
                continue;
            }
            else if(Lines[i].StartsWith("for"))
            {
                ForConstructionSeparator(Lines[i],true,i,i);
                continue;
            }
            else if(Lines[i].StartsWith("while"))
            {
                ForConstructionSeparator(Lines[i],false,i,i);
                continue;
            }
            switch(Lines[i])
            {
                case "START":
                    ND[i].Nodes.Add(new Node(NodeTypes.START));
                    break;
                case "END":
                    ND[i].Nodes.Add(new Node(NodeTypes.END));
                    break;
                case "continue":
                    ND[i].Nodes.Add(new Node(NodeTypes.CONTINUE));
                    break;
                case "break":
                    ND[i].Nodes.Add(new Node(NodeTypes.BREAK));
                    break;
                default:
                    SeparateLinear(Lines[i],i,i);
                    break;
            }
            BrecketsColl.Clear();                
        }
    }

}
