using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;

public class Parse
{

    public Parse()
    {
        RootNodes = new List<Node>();
        PartsColl = new List<Part>();
    }
    internal List<Node> RootNodes;
    internal List<Part> PartsColl;

    Dictionary<Int64,Node> Nodes = new Dictionary<long, Node>();
    List<MyVar> Vars = new List<MyVar>();
   
    static string[] ImportantPreferenses = new string[] {"==",">=","<=","<",">","=","+=","-=","*=","/="," and "," or ","**","+","-","*","/"};
    static string[] EqualsPosibilities = new string[] {"=","+=","-=","*=","/="};

    static string[] Keywords = new string[] {"true","false"};
    bool IsNewVar(string Target, Node ParentNode, NodeTypes ParentNodeType,string SourceLine)
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
    bool IsVar(string Target) =>
        Regex.Matches(Target,@"([A-Za-zА-Яа-я_]+\d*)+").Count == 1 && Regex.Matches(Target,@"([A-Za-zА-Яа-я_]+\d*)+")[0].Value == Target;

    NodeTypes GetNodeType(ref string Target, long ParNodeUID, Node CurrNode,string SourceLine)
    {
        Node ParentNode = Nodes[ParNodeUID];
        if(Regex.Matches(Target,"[\"'].*[\\d\\w]*.*[\"']").Count != 0 || Regex.Match(Target,@"[\d\.]+").Value == Target)
        {
            return NodeTypes.CONST;
        }
        if (IsVar(Target))
        {
            if(IsNewVar(Target,ParentNode, ParentNode.NodeType,SourceLine))
            {
                Vars.Add(new MyVar(){name = Target}); 
                return NodeTypes.NVAR;
            }
            else
                return NodeTypes.VAR;
        }
        else
            throw new ParsingException("Не удалось распознать имя переменной, функции или команды из строки " + Target,SourceLine,Target);
    }

    static string[] Split(String s,out string? RSeparator)
    {

        foreach (var i in ImportantPreferenses)
        {
            string[] SubStrings =  s.Split(i,2);

            if(SubStrings.Length == 2)
            {   
                RSeparator = i;
                return SubStrings;
            }
        }
        RSeparator = null;
        return new string[]{s};
    }

    public Node IfConstructionSeparator(string Line,int CurGlobalNDPos, Int64 ParentUID,string SourceLine)
    {
        string NormalizedIfConstruction = Regex.Match(Line,@"(?<=if)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
        Node CurNode = new Node(NodeTypes.IF,ParentUID) {SrcLine = SourceLine};
        CurNode.Target = Regex.Match(Line,"if|elif|else").Value;
        Nodes.Add(CurNode.UID,CurNode);
        RootNodes.Add(CurNode);
        
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false,false,SourceLine);
        return CurNode;
    }
    public Node SeparateLinear(string Line,int CurGlobalNDPos, Int64 ParentUID, bool IsBrackets, bool IsBase,string SourceLine)
    {   
        if(Line == String.Empty)
            return new Node(NodeTypes.NONE){SrcLine = SourceLine};


        string[] Sublines = Split(Line,out string? RSeparator);
        Node CurNode = new Node(ParentUID) {IsBrackets = IsBrackets, IsBase = IsBase,SrcLine = SourceLine};
        Nodes.Add(CurNode.UID,CurNode);
        
        if(Sublines.Length == 1 && Sublines[0].StartsWith('#'))
        {
            if(!int.TryParse(Sublines[0].TrimStart('#'),out int BracketCollIndex) || BracketCollIndex >= PartsColl.Count )
                throw new ParsingException("Не удалось распознать имя переменной, функции или команды из строки " + Sublines[0].TrimStart('#')[1..], SourceLine,Sublines[0].TrimStart('#')[1..]);
            var t = PartsColl;
            Part CurrPart = PartsColl[BracketCollIndex];
            
            if(CurrPart.Target.EndsWith("-"))
                throw new ParsingException("Ошибка: оператор - перед скобкой" , SourceLine,"-");
            else if(CurrPart.Target.EndsWith("/"))
                throw new ParsingException("Ошибка: оператор / перед скобкой" , SourceLine,"/");
            else if(CurrPart.Target.EndsWith("**"))
                throw new ParsingException("Ошибка: оператор ** перед скобкой" , SourceLine,"**");
            else if(CurrPart.Target.EndsWith("+"))
                throw new ParsingException("Ошибка: оператор + перед скобкой" , SourceLine,"+");
            else if(CurrPart.Target.EndsWith("*"))
                throw new ParsingException("Ошибка: оператор * перед скобкой" , SourceLine,"*");

            else if(CurrPart.Target.StartsWith("-"))
                throw new ParsingException("Ошибка: оператор - после скобки" , SourceLine,"-");
            else if(CurrPart.Target.StartsWith("/"))
                throw new ParsingException("Ошибка: оператор / после скобки" , SourceLine,"/");
            else if(CurrPart.Target.StartsWith("**"))
                throw new ParsingException("Ошибка: оператор ** после скобки" , SourceLine,"**");
            else if(CurrPart.Target.StartsWith("+"))
                throw new ParsingException("Ошибка: оператор + после скобки" , SourceLine,"+");
            else if(CurrPart.Target.StartsWith("*"))
                throw new ParsingException("Ошибка: оператор * после скобки" , SourceLine,"*");


            if(CurrPart.Type == PartType.Constant)
            {
                CurNode.NodeType = NodeTypes.CONST;
                CurNode.ChildNodes = null;
                CurNode.Target = CurrPart.Target;
                return CurNode;
            }
            else if(CurrPart.Type == PartType.Array)
            {
                string[] ArrItems = PartsColl[BracketCollIndex].Target.Split(',');
                if(ArrItems.Any((t) => t == String.Empty))
                    throw new ParsingException($"Массив {Sublines[0]} содержит пустые строки",SourceLine,"\"\"");
                foreach (var item in ArrItems)
                {
                    if(item == String.Empty)
                        throw new ParsingException($"Массив {Sublines[0]} содержит пустые строки",SourceLine,"\"\"");
                    else if(item.Contains('#') && !item.StartsWith("#"))
                        throw new ParsingException("Некорректный набор массива. Вероятно пропущена запятая в строке " +item.Split("#").First().ToString()+PartsColl[int.Parse(item.Split("#").Last().ToString())].Target,SourceLine,item.Split("#").First().ToString()+PartsColl[int.Parse(item.Split("#").Last().ToString())].Target);
                }

                CurNode.NodeType = NodeTypes.ARRAY;
                CurNode.ChildNodes = new Node[ArrItems.Length];

                for (int i = 0; i < ArrItems.Length; i++)
                    CurNode.ChildNodes[i] = SeparateLinear(ArrItems[i].Trim(),CurGlobalNDPos,CurNode.UID,false,false,SourceLine);
                
                return CurNode;
            }
            else
                return SeparateLinear(CurrPart.Target,CurGlobalNDPos,ParentUID,true,false,SourceLine);
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
            FirstChild = SeparateLinear(Sublines[0].Trim(),CurGlobalNDPos,CurNode.UID,false,false,SourceLine);
            SecoundChild = SeparateLinear(Sublines[1].Trim(),CurGlobalNDPos, CurNode.UID,false,false,SourceLine);
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
                    CurNode.ChildNodes[i] = SeparateLinear(Params[i].Trim(),CurGlobalNDPos,CurNode.UID,false,false,SourceLine);
                
                if(CurNode.NodeType == NodeTypes.CALL && CurNode.ChildNodes.Any((n) => n.NodeType == NodeTypes.OPERATOR && (n.Target == ">" || n.Target == "<" || n.Target == "==")))
                    throw new ParsingException("Недопустимо использование логических операторов в вызове функции " + CurNode.Target,SourceLine,Regex.Match(SourceLine,">|<|==").Value);
                return CurNode;
            }
            CurNode.ChildNodes = null;
            CurNode.Target = Sublines[0];
            CurNode.NodeType = GetNodeType(ref CurNode.Target,IsBase?CurNode.UID:ParentUID, CurNode,SourceLine);
        }
        return CurNode;
    }
}