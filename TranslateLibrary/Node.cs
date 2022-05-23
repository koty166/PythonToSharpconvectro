
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

    static Int64 GetUID() => new Random().NextInt64();


    public override string ToString()
    {
        switch(this.NodeType)
        {
            case NodeTypes.EQUALS:
            case NodeTypes.IN:
            case NodeTypes.OPERATOR:
                if(ChildNodes == null)
                    return "ERROR";
                Target = PostGenerationReplacement.ReplaceLogicalOperators(Target);
                if(!IsBrackets)
                    return ChildNodes[0].ToString() + Target + ChildNodes[1].ToString()  + (IsBase?";":"");
                else
                    return "("+ChildNodes[0].ToString()  + Target + ChildNodes[1].ToString()  +")" + (IsBase?";":"");
            case NodeTypes.CALL:
                if(Target == null || ChildNodes==null)
                    return "ERROR";

                Target = PostGenerationReplacement.PostGenerationOptimizing(Target, out bool IsRight);
                
                StringBuilder ParamRes = new StringBuilder(100);
                foreach (var item in ChildNodes)
                    ParamRes.Append(item.ToString()   + ", ");
                ParamRes.Remove(ParamRes.Length-2,2);
                
                if(IsRight)
                    return Target+"("+ParamRes.ToString()  + ")" + (IsBase?";":"");
                else
                     return "("+ParamRes.ToString()  + ")"+Target + (IsBase?";":"");

            case NodeTypes.ARRAY:
                if(Target == null || ChildNodes==null)
                    return "ERROR";

                StringBuilder ArrValues = new StringBuilder(100);
                foreach (var item in ChildNodes)
                    ArrValues.Append(item.ToString()  + ", ");
                ArrValues.Remove(ArrValues.Length-2,2);

                string OutValue = ArrValues.ToString();

                if(Target != string.Empty)
                    return Target+"["+ArrValues.ToString() + "]";
                else if(ChildNodes.Length > 0 && OutValue != string.Empty)
                    return "new object[]"+" {"+OutValue + "}";
                else
                   return "new object[0]";
                
            case NodeTypes.NVAR:
                if(Target == null)
                    return "ERROR";
                    
                return "dynamic "+Target;

            case NodeTypes.VAR:
            case NodeTypes.CONST:
                if(!IsBrackets)
                    return Target;
                else
                    return "("+Target +")";

            case NodeTypes.NONE:
                return String.Empty;

            case NodeTypes.BREAK:
                return "break;";

            case NodeTypes.CONTINUE:
                return "continue;";
            


            case NodeTypes.END:
                return "}";

            case NodeTypes.START:
                return "{";



            case NodeTypes.FOR:
                if(ChildNodes == null)
                    return "ERROR";

                return  "foreach(" + ChildNodes[0].ToString()   + ")";

            case NodeTypes.WHILE:
                if(ChildNodes == null)
                    return "ERROR";

                return  "while(" + ChildNodes[0].ToString()   + ")";

            case NodeTypes.IF:
                if(ChildNodes == null)
                    return "ERROR";
                switch(Target)
                {
                    case "if":
                        if(ChildNodes[0].IsBrackets)
                            return  "if" + ChildNodes[0].ToString() ;
                        return  "if" +"(" + ChildNodes[0].ToString()   + ")";
                    case "elif":
                        if(ChildNodes[0].IsBrackets)
                            return  "else if" + ChildNodes[0].ToString() ;
                        return  "else if" +"(" + ChildNodes[0].ToString()   + ")";
                    case "else":
                        return "else";
                }
                return "ERROR";
                

            case NodeTypes.IMPORT:
                if(Target == null)
                    return "ERROR";
                string LibName = String.Empty;
                if(Target.Contains(" as "))
                    LibName = Regex.Split(Target," as ")[1] + " = ";
                if(Target.StartsWith("import")) 
                { 
                    LibName = LibName + Target.Split(' ')[1];
                } 
                else if(Target.StartsWith("from"))
                {
                    LibName = String.Empty;
                    LibName = "static " + LibName + Target.Split(' ')[1];
                }

                return "using "+LibName+";";

            default:
            return "ERROR";
        }
    }
}