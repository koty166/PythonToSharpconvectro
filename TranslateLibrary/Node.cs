
using System.Text;
namespace TranslateLibrary;



public class Node
{
    public NodeTypes NodeType;
    public String Target;
    public Int64 UID; 
    public Int64 ParentUID;
    public Node[]? ChildNodes;
    public bool IsBrackets;
    public Node()
    {
        ChildNodes = null;
        NodeType = NodeTypes.NONE;
        Target = String.Empty;
    }
    public Node(NodeTypes _NodeType)
    {
        ChildNodes = null;
        NodeType = _NodeType;
        Target = String.Empty;
    }
    public Node(Int64 _UID,Int64 _ParentUID)
    {
        ChildNodes = null;
        NodeType = NodeTypes.NONE;
        Target = String.Empty;
        ParentUID = _ParentUID;
        UID = _UID;
    }
    public Node(Int64 _UID,Int64 _ParentUID,NodeTypes _NodeType)
    {
        ChildNodes = null;
        NodeType = _NodeType;
        Target = String.Empty;
        ParentUID = _ParentUID;
        UID = _UID;
    }
    public override string ToString()
    {
        switch(this.NodeType)
        {
            case NodeTypes.EQUALS:
                if(ChildNodes == null)
                    return "ERROR";

                return ChildNodes[0].ToString() + Target + ChildNodes[1].ToString() + ";";

            case NodeTypes.CALL:
                if(Target == null || ChildNodes==null)
                    return "ERROR";

                StringBuilder ParamRes = new StringBuilder(100);
                foreach (var item in ChildNodes)
                    ParamRes.Append(item.ToString() + ", ");
                ParamRes.Remove(ParamRes.Length-2,2);

                return Target+"("+ParamRes.ToString() + ")";

            case NodeTypes.NVAR:
                if(Target == null)
                    return "ERROR";

                return "dynamic "+Target;

            case NodeTypes.VAR:
            case NodeTypes.CONST:
                return Target;

            case NodeTypes.OPERATOR:
                if(ChildNodes == null)
                    return "ERROR";
                if(IsBrackets)
                    return "("+ChildNodes[0].ToString() + Target+ChildNodes[1].ToString()+")";
                else
                    return ChildNodes[0].ToString() + Target+ChildNodes[1].ToString();

            case NodeTypes.NONE:
                return String.Empty;

            case NodeTypes.BREAK:
                return "break";

            case NodeTypes.CONTINUE:
                return "continue";
            


            case NodeTypes.END:
                return "}";

            case NodeTypes.START:
                return "{";



            case NodeTypes.FOR:
                if(ChildNodes == null)
                    return "ERROR";

                return  "for(" + ChildNodes[0].ToString() + ")";

            case NodeTypes.WHILE:
                if(ChildNodes == null)
                    return "ERROR";

                return  "while(" + ChildNodes[0].ToString() + ")";

            case NodeTypes.IF:
                if(ChildNodes == null)
                    return "ERROR";
                if(ChildNodes[0].IsBrackets)
                    return  "if" + ChildNodes[0].ToString();
                return  "if(" + ChildNodes[0].ToString() + ")";

            case NodeTypes.IMPORT:
                if(Target == null)
                    return "ERROR";
                
                return "using "+Target;

            default:
            return "ERROR";
        }
    }
}