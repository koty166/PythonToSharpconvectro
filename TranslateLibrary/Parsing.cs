using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;

public class Parse
{

    internal List<Node> RootNodes;
    internal List<Part> PartsColl;

    Dictionary<Int64,Node> Nodes = new Dictionary<long, Node>();
    List<MyVar> Vars = new List<MyVar>();
   
    static string[] ImportantPreferenses = new string[] {"=","+=","-=","*=","/=","==",">=","<=","<",">"," and "," or ","+","-","*","/"," in ","."};
    static string[] EqualsPosibilities = new string[] {"=","+=","-=","*=","/="};

    static string[] Keywords = new string[] {"true","false"};
    bool IsNewVar(string Target, Node ParentNode, NodeTypes ParentNodeType)
    {
        IEnumerable<MyVar> VarRes = from cur in Vars
                                    where cur.name == Target
                                    select cur;
        int VarSearchRes = VarRes.Count();
        if(VarSearchRes == 0 && ((ParentNode.IsBase && ParentNodeType == NodeTypes.EQUALS) || ParentNode.Target == " in ")
            && !Keywords.Contains(Target) )
            return true;
        else 
            return false;
    }

    NodeTypes GetNodeType(ref string Target, long ParNodeUID, Node CurrNode)
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
        else if(IsNewVar(Target,ParentNode, ParentNode.NodeType))
        {
            NodeType = NodeTypes.NVAR;
            Vars.Add(new MyVar(){name = Target}); 
        }
        else
            NodeType = NodeTypes.VAR;
        return NodeType;
    }

    static string[] Split(String s,out string? RSeparator)
    {
        Regex r = new Regex(@"[=*\/+-.]+| in | and | or ");

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
    static int GetMostImportantMatchNumber(MatchCollection mc)
    {
        foreach (var item in ImportantPreferenses)
            for (int i = 0; i < mc.Count; i++)
                if(mc[i].Value == item)
                    return i;
        return 0;
    }

    public Node IfConstructionSeparator(string Line,int CurGlobalNDPos, Int64 ParentUID)
    {
        string NormalizedIfConstruction = Regex.Match(Line,@"(?<=if)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
        Node CurNode =  new Node(NodeTypes.IF,ParentUID);
        CurNode.Target = Regex.Match(Line,"if|elif|else").Value;
        Nodes.Add(CurNode.UID,CurNode);
        RootNodes.Add(CurNode);
        
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false,false);
        return CurNode;
    }
    public Node ForConstructionSeparator(string Line,bool IsFor,int CurGlobalNDPos, Int64 ParentUID)
    {
        Node CurNode =  new Node(ParentUID);
        CurNode.IsBase = true;

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
        RootNodes.Add(CurNode);

        return CurNode;
    }
    public Node SeparateLinear(string Line,int CurGlobalNDPos, Int64 ParentUID, bool IsBrackets, bool IsBase)
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
           RootNodes.Add(CurNode);
        if(RSeparator != null)
        {
            if(EqualsPosibilities.Contains(RSeparator))
                CurNode.NodeType = NodeTypes.EQUALS;
            else
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

                if(PartsColl[BracketCollIndex].Type == PartType.Brackets)
                    CurNode.NodeType = NodeTypes.CALL;
                else if(PartsColl[BracketCollIndex].Type == PartType.Array)
                    CurNode.NodeType = NodeTypes.ARRAY;
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
}