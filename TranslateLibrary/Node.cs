
namespace TranslateLibrary;


public class Node
{
    public NodeTypes NodeType;
    public String Target;
    public Int64 UID; 
    public Int64 ParentUID;
    public Node[]? ChildNodes;

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
}