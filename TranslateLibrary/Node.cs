
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;



public class Node
{
    public NodeTypes NodeType;
    public String Target;
    public Int64 UID {get; private set;}
    Int64 ParentUID;
    public Node[]? ChildNodes;
    public bool IsBrackets;
    public bool IsBase;
    public string SrcLine;
    public string ErrorPart;

    public static Dictionary<string,string> Vars = new Dictionary<string, string>();
    public Node()
    {
        ChildNodes = null;
        NodeType = NodeTypes.NONE;
        Target = String.Empty;
        UID = GetUID();
    }
    public Node(NodeTypes _NodeType) : this()
    {
        NodeType = _NodeType;
    }
    public Node(Int64 _ParentUID) : this()
    {
        ParentUID = _ParentUID;
    }
    public Node(NodeTypes _NodeType, string _Target) : this()
    {
        NodeType = _NodeType;
        Target = _Target;
    }
    public Node(NodeTypes _NodeType, Int64 _UID) : this()
    {
        NodeType = _NodeType;
        UID = _UID;
    }
    public Node(Int64 _UID,Int64 _ParentUID) : this()
    {
        ParentUID = _ParentUID;
        UID = _UID;
    }
    public Node(NodeTypes _NodeType , Int64 _UID,Int64 _ParentUID) : this()
    {
        NodeType = _NodeType;
        ParentUID = _ParentUID;
        UID = _UID;
    }
    public Node(NodeTypes _NodeType , Int64 _UID,Int64 _ParentUID, string _Target)
    {
        ChildNodes = null;
        NodeType = _NodeType;
        ParentUID = _ParentUID;
        UID = _UID;
        Target = _Target;
    }
    public Node(NodeTypes _NodeType , Int64 _UID,Int64 _ParentUID, string _Target, Node[] _ChildNodes)
    {
        ChildNodes = _ChildNodes;
        NodeType = _NodeType;
        ParentUID = _ParentUID;
        UID = _UID;
        Target = _Target;
    }
    public static bool Check(Node Node, Node Parent)
    {   
        if(Node.NodeType == NodeTypes.OPERATOR && (Node.ChildNodes[0].NodeType == NodeTypes.NONE || Node.ChildNodes[1].NodeType == NodeTypes.NONE ))
            throw new ParsingException("Синтаксическая ошибка. Некорректное применение оператора " + Node.Target,Node.SrcLine, Node.Target);
        else if(Node.NodeType == NodeTypes.EQUALS && (Node.ChildNodes[0].NodeType == NodeTypes.EQUALS || Node.ChildNodes[1].NodeType == NodeTypes.EQUALS ))
            throw new ParsingException("Некоректное применение знака равенства" ,Node.SrcLine,"=");
        return true;
    }
    static Int64 GetUID() => new Random().NextInt64();

    public override string ToString()
    {
        return Target;
    }
    public static double Calculate(string left, string right, string oper)
    {
        double dleft = double.Parse(left) , dright = double.Parse(right);
        return oper.Trim() switch
        {
            "+" => dleft + dright,
            "-" => dleft - dright,
            "/" => dleft / dright,
            "*" => dleft * dright,
            "**" => Math.Pow(dleft,dright),
            _ => throw new NotImplementedException(),
        };
    }
    public string Calculate(Node Parent)
    {
         switch(this.NodeType)
        {
            case NodeTypes.EQUALS:
                Check(this,Parent);
                if(ChildNodes == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);
                string LeftRes = ChildNodes[0].ToString(),RightRes = ChildNodes[1].Calculate(this);
                Node.Vars[LeftRes] = RightRes; 
                return  LeftRes+ " = " +RightRes ;
                
            case NodeTypes.OPERATOR:

               Check(this,Parent);
                if(ChildNodes == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);

                if(Target == "==")
                {
                    return double.Parse(ChildNodes[0].Calculate(this)) == double.Parse(ChildNodes[1].Calculate(this)) ? "TRUE" : "FALSE";
                }
                else if(Target == ">")
                {
                    return double.Parse(ChildNodes[0].Calculate(this)) > double.Parse(ChildNodes[1].Calculate(this)) ? "TRUE" : "FALSE";
                }
                else if(Target == "<")
                {
                    return double.Parse(ChildNodes[0].Calculate(this)) < double.Parse(ChildNodes[1].Calculate(this)) ? "TRUE" : "FALSE";
                }

                string res;

                try{ res = Node.Calculate(ChildNodes[0].Calculate(this),ChildNodes[1].Calculate(this),Target).ToString();}
                catch { res =  $"{ChildNodes[0].Calculate(this)} {Target} {ChildNodes[1].Calculate(this)}"; }
                
                return res;
            case NodeTypes.CALL:
               Check(this,Parent);
                if(Target == null || ChildNodes==null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);
                
                StringBuilder ParamRes = new StringBuilder(100);
                foreach (var item in ChildNodes)
                    ParamRes.Append(item.Calculate(this)   + ", ");
                ParamRes.Remove(ParamRes.Length-2,2);
                
                return $"Вызов функции {Target} с аргументами ({ParamRes})";

            case NodeTypes.ARRAY:
               Check(this,Parent);
                if(Target == null || ChildNodes==null)
                   throw new AnalyzeException("Объявлен пустой массив",SrcLine,null);

                StringBuilder ArrValues = new StringBuilder(100);
                foreach (var item in ChildNodes)
                    ArrValues.Append(item.ToString()  + ", ");
                ArrValues.Remove(ArrValues.Length-2,2);

                string OutValue = ArrValues.ToString();

                if(Target != string.Empty)
                    return Target+"["+ArrValues.ToString() + "]";
                else if(ChildNodes.Length > 0 && OutValue != string.Empty)
                    return $"[{OutValue}]";
                else
                   throw new AnalyzeException("Объявлен пустой массив",SrcLine,null);
                
            case NodeTypes.NVAR:
               Check(this,Parent);
                if(Target == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);
                Node.Vars.Add(Target,"0");
                return Target;

            case NodeTypes.VAR:
               Check(this,Parent);
                if(!Node.Vars.ContainsKey(Target))
                    throw new AnalyzeException("Неизвестная переменная " + Target,SrcLine,Target);
                return Node.Vars[Target];
            case NodeTypes.CONST:
                return Target;

            case NodeTypes.NONE:
                return String.Empty;

            case NodeTypes.BREAK:
                return "break;";

            case NodeTypes.CONTINUE:
                return "continue;";
            


            case NodeTypes.END:
                return "CONTINUE";

            case NodeTypes.FOR:
                if(ChildNodes == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);

                return  "foreach(" + ChildNodes[0].ToString()   + ")";

            case NodeTypes.WHILE:
                if(ChildNodes == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);

                return  "while(" + ChildNodes[0].ToString()   + ")";

            case NodeTypes.IF:
                if(ChildNodes == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);

                if(ChildNodes[0].Calculate(this) == "FALSE")
                    return "SKIP";
                return "CONTINUE";

            case NodeTypes.IMPORT:
               Check(this,Parent);
                if(Target == null)
                    throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);
                string LibName = String.Empty;
                if(Target.Contains(" as "))
                    LibName = Regex.Split(Target," as ")[1] + " как ";
                if(Target.StartsWith("import")) 
                { 
                    LibName += Target.Split(' ')[1];
                } 

                return $"Импортирована библиотека {LibName}";

            default:
            throw new AnalyzeException("Нет дочерних элементов",SrcLine,null);
        }
    }
}