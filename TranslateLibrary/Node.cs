
namespace TranslateLibrary;


public class Node
{
    public NodeTypes NodeType = NodeTypes.NONE;
    public String Target;
    public Int64 UID; 
    public Int64 ParentUID;
    public Node[] ChildNodes;
}