using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TranslateLibrary;

public class Core
{
    /// <summary>
    /// Основной метод трансляции кода. В обоих случаях сохраняется форматирование.
    /// </summary>
    /// <param name="SourceCode">Исходный код на Python</param>
    /// <returns>Код на c#</returns>
    public static string Translate(string SourceCode)
    {
        ND.Clear();
        Nodes.Clear();
        Vars.Clear();
        Funcs.Clear();
        PartsColl.Clear();

        GenNodes(SourceCode);
        return CodeGenerator.GenCode(ND.ToArray());
        
    }
    //Можеит заменить на список регулярок?
    static List<Node> ND = new List<Node>();

    static Dictionary<Int64,Node> Nodes = new Dictionary<long, Node>();
    static string[] ImportantPreferenses = new string[] {"=","+=","-=","*=","/=","==",">=","<=","<",">"," and "," or ","+","-","*","/","in","."};
    static string[] EqualsPosibilities = new string[] {"=","+=","-=","*=","/="};

    static List<MyVar> Vars = new List<MyVar>();
    static  List<MyFunc> Funcs = new List<MyFunc>();

    static List<Part> PartsColl = new List<Part>();

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
    
   
    static bool IsNewVar(string Target, Node ParentNode)
    {
        IEnumerable<MyVar> VarRes = from cur in Vars
                                    where cur.name == Target
                                    select cur;
        int VarSearchRes = VarRes.Count();
        if(VarSearchRes == 0 && ParentNode.IsBase/*(ParentNodeType == NodeTypes.EQUALS ||ParentNodeType == NodeTypes.FOR
                                        || ParentNodeType == NodeTypes.WHILE )*/)
            return true;
        else
            return false;
    }
    static NodeTypes GetNodeType(ref string Target, long ParNodeUID, Node CurrNode)
    {
        NodeTypes NodeType = NodeTypes.NONE;
        
        Node ParentNode = Nodes[ParNodeUID];
        MatchCollection ArrMatches = Regex.Matches(Target,",?([\\d]+|[\"'][\\d\\s\\w]+[\"']|[\\d\\w]+),?");
        if(Target.StartsWith('[') && Target.EndsWith(']'))
        {
            if(ArrMatches.Count == 0)
                return NodeTypes.CONST;

            CurrNode.NodeType = NodeTypes.ARRAY;
            CurrNode.ChildNodes = new Node[ArrMatches.Count];
            for (int i = 0; i < ArrMatches.Count; i++)
            {
                String BufValue = ArrMatches[i].Value.Trim(','); 
                if(BufValue.StartsWith('\'') && BufValue.EndsWith('\'') || BufValue.StartsWith('\"') && BufValue.EndsWith('\"') || BufValue.All(ch => char.IsDigit(ch)))
                    CurrNode.ChildNodes[i] = new Node(NodeTypes.CONST,BufValue);
                else
                    CurrNode.ChildNodes[i] = new Node(NodeTypes.VAR,BufValue);
            }
        }
        else if(Regex.Matches(Target,"[\"'].*[\\d\\w]*.*[\"']").Count != 0 || Regex.Match(Target,@"\d+").Value == Target)
        {
            NodeType = NodeTypes.CONST;
        }
        else if(IsNewVar(Target,ParentNode))
        {
            NodeType = NodeTypes.NVAR;
            Vars.Add(new MyVar(){name = Target}); 
        }
        else
            NodeType = NodeTypes.VAR;
        return NodeType;
    }
    
    public static void RepaceBreckets(ref string Line)
    {
        //Перенести в какйо-нибудь отдельный класс для пре-инициализации.
        Regex ReplaceEmpty = new Regex("(\"\")|(\'\')|(\\[\\])|(\\(\\))",RegexOptions.Compiled);
        Regex ReplaceNonEmptyQuotation = new Regex("(\"[^\"]+\")|('[^']+')",RegexOptions.Compiled);
        Regex ReplaceNonEmptyArrBrackets = new Regex(@"(\[[^\[\]]+\])",RegexOptions.Compiled);
        Regex ReplaceNonEmptyBrackets = new Regex(@"\(([^()]+\))",RegexOptions.Compiled);
        
        int CurrPointer = PartsColl.Count;
        
        Line =  ReplaceEmpty.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    PartsColl.Add(new Part(
                                        PartType.Constant,
                                        MValue));
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyQuotation.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    PartsColl.Add(new Part(
                                        PartType.Constant,
                                        MValue));
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyArrBrackets.Replace(Line,(Match m) => { 
                                   string MValue = m.Value;

                                    PartsColl.Add(new Part(
                                        PartType.Array,
                                        MValue.Trim("[]".ToCharArray())));
                                         
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyBrackets.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    PartsColl.Add(new Part(
                                        PartType.Brackets,
                                        MValue.Trim("()".ToCharArray())));

                                    return "#"+CurrPointer++; });
    }
    
    /// <summary>
    /// Построение древа для линейного выражения.
    /// </summary>
    /// <param name="Line"></param>
    /// <param name="IsLeft"></param>
    /// <param name="ParentUID"></param>
    /// <returns></returns>
    public static Node SeparateLinear(string Line,int CurGlobalNDPos, Int64 ParentUID, bool IsBrackets, bool IsBase)
    {   
        if(Line == String.Empty || Line == "()")
            return new Node(NodeTypes.NONE);

        string[] Sublines = Split(Line,out string? RSeparator);
        Node CurNode = new Node(ParentUID) {IsBrackets = IsBrackets, IsBase = IsBase};
        Nodes.Add(CurNode.UID,CurNode);
        
        if(Sublines.Length == 1 && Sublines[0].StartsWith('#'))
        {
            int BracketCollIndex = int.Parse(Sublines[0].TrimStart('#'));
            var t = PartsColl;
            Part CurrPart = PartsColl[BracketCollIndex];
            
            if(CurrPart.Type == PartType.Constant)
            {
                CurNode.NodeType = NodeTypes.CONST;
                CurNode.ChildNodes = null;
                CurNode.Target = CurrPart.Target;
                return CurNode;
            }
            else if(CurrPart.Type == PartType.Array)
            {
                string[] ArrItems = PartsColl[BracketCollIndex].Target.Split(',',StringSplitOptions.TrimEntries);
                CurNode.NodeType = NodeTypes.ARRAY;
                CurNode.ChildNodes = new Node[ArrItems.Length];

                for (int i = 0; i < ArrItems.Length; i++)
                    CurNode.ChildNodes[i] = SeparateLinear(ArrItems[i].Trim(),CurGlobalNDPos,CurNode.UID,false,false);
                
                return CurNode;
            }
            else
            return SeparateLinear(CurrPart.Target,CurGlobalNDPos,ParentUID,true,false);
        }

       
        
        if(IsBase)
            ND.Add(CurNode);
        if(RSeparator != null)
        {
            CurNode.NodeType = NodeTypes.OPERATOR;
            CurNode.Target = RSeparator;
        }

        Node? FirstChild = null, SecoundChild = null;
        if(Sublines.Length != 1)
        {
            FirstChild = SeparateLinear(Sublines[0].Trim(),CurGlobalNDPos,CurNode.UID,false,false);
            SecoundChild = SeparateLinear(Sublines[1].Trim(),CurGlobalNDPos, CurNode.UID,false,false);
            CurNode.ChildNodes = new Node[]{FirstChild,SecoundChild};
        }
        else
        {
            Match FuncCallRegexResault = Regex.Match(Sublines[0],@"#[\d]+");
            if(FuncCallRegexResault.Value != String.Empty)
            {
                int BracketCollIndex = int.Parse(FuncCallRegexResault.Value.TrimStart('#'));
                string[] Params = PartsColl[BracketCollIndex].Target.Split(',');

                CurNode.NodeType = NodeTypes.CALL;
                CurNode.Target = Sublines[0].Substring(0,FuncCallRegexResault.Index);
                CurNode.ChildNodes = new Node[Params.Length];

                for (int i = 0; i < Params.Length; i++)
                    CurNode.ChildNodes[i] = SeparateLinear(Params[i].Trim(),CurGlobalNDPos,CurNode.UID,false,false);
                return CurNode;
            }
            CurNode.ChildNodes = null;
            CurNode.Target = Sublines[0];
            CurNode.NodeType = GetNodeType(ref CurNode.Target,IsBase?CurNode.UID:ParentUID, CurNode);
        }
        return CurNode;
    }

    static string[] Normalazing(string[] SourceCodeLines)
    {
        List<string> OutLines = new List<string>();
        int CurTabsNum = 0;
        int Opened = 0, Closed = 0;
        for (int i = 0; i < SourceCodeLines.Length; i++)
        {
            if(SourceCodeLines[i].Contains('#'))
                continue;

            SourceCodeLines[i] = SourceCodeLines[i].Replace("    ","\t").Trim();
            int TabsNum = SourceCodeLines[i].Count((ch) => ch == '\t');
            OutLines.Add(SourceCodeLines[i]);
            if(TabsNum > CurTabsNum)
            {
                OutLines.Add(NodeTypes.START.ToString());
                Opened++;
            }
            else if(TabsNum < CurTabsNum)
            {
                OutLines.Add(NodeTypes.END.ToString());
                Closed++;
            }
            CurTabsNum = TabsNum;
            
        }
        if(Opened - Closed == 1)
            OutLines.Add(NodeTypes.END.ToString());
        return OutLines.ToArray();
    }
    
    static Node IfConstructionSeparator(string Line,int CurGlobalNDPos, Int64 ParentUID)
    {
        string NormalizedIfConstruction = Regex.Match(Line,@"(?<=if)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
        Node CurNode =  new Node(NodeTypes.IF,ParentUID);
        Nodes.Add(CurNode.UID,CurNode);
        ND.Add(CurNode);
        
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false,false);
        return CurNode;
    }
    static Node ForConstructionSeparator(string Line,bool IsFor,int CurGlobalNDPos, Int64 ParentUID)
    {
        Node CurNode =  new Node(ParentUID);
        Nodes.Add(CurNode.UID,CurNode);
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
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false,false);
        ND.Add(CurNode);

        return CurNode;
    }
    static void GenNodes(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        Lines = Normalazing(Lines);

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
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
            else if(Lines[i].StartsWith("import"))
            {
                if(Lines[i].Contains(" as "))
                    ND.Add(new Node(NodeTypes.IMPORT)
                    {
                        Target =  Regex.Split(Lines[i]," as ")[1] + " = "+ Lines[i].Split(' ')[1]
                    });
                else
                    ND.Add(new Node(NodeTypes.IMPORT){Target = Lines[i].Split(' ')[1]});
                
                continue;
            }
            else if(Lines[i].StartsWith("from"))
            {
                if(Lines[i].Contains(" as "))
                    ND.Add(new Node(NodeTypes.IMPORT)
                    {
                        Target = "static " + Regex.Split(Lines[i]," as ")[1] + " = "+ Lines[i].Split(' ')[1]
                    });
                else
                    ND.Add(new Node(NodeTypes.IMPORT){Target = "static " + Lines[i].Split(' ')[1]});
                continue;
            }
            switch(Lines[i])
            {
                case "START":
                    ND.Add(new Node(NodeTypes.START));
                    break;
                case "END":
                    ND.Add(new Node(NodeTypes.END));
                    break;
                case "continue":
                    ND.Add(new Node(NodeTypes.CONTINUE));
                    break;
                case "break":
                    ND.Add(new Node(NodeTypes.BREAK));
                    break;
                default:
                    SeparateLinear(Lines[i],i,i,false,true);
                    break;
            }            
        }
    }

}
