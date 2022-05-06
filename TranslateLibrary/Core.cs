﻿using IronPython.Hosting;
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
        GenNodes(SourceCode);
        return CodeGenerator.GenCode(ND.ToArray());
        
    }
    //Можеит заменить на список регулярок?
    static List<Node> ND = new List<Node>();

    static Dictionary<Int64,Node> Nodes = new Dictionary<long, Node>();
    static string[] ImportantPreferenses = new string[] {"=","+=","-=","*=","/=","==","+","-","*","/","in","."};
    static string[] EqualsPosibilities = new string[] {"=","+=","-=","*=","/="};
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
   
    static bool IsNewVar(string Target, NodeTypes ParentNodeType)
    {
        IEnumerable<MyVar> VarRes = from cur in Vars
                                    where cur.name == Target
                                    select cur;
        int VarSearchRes = VarRes.Count();
        if(VarSearchRes == 0 && (ParentNodeType == NodeTypes.EQUALS ||ParentNodeType == NodeTypes.FOR
                                        || ParentNodeType == NodeTypes.WHILE ))
        {
            return true;
            
        }
        else
        {
            return false;
        }   
    }
    static NodeTypes GetNodeType(ref string Target, long ParNodeUID, Node CurrNode)
    {
        NodeTypes NodeType = NodeTypes.NONE;
        
        NodeTypes ParentNodeType = Nodes[ParNodeUID].NodeType;
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
        else if(IsNewVar(Target,ParentNodeType))
        {
            NodeType = NodeTypes.NVAR;
            Vars.Add(new MyVar(){name = Target}); 
        }
        else
            NodeType = NodeTypes.VAR;
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
            if(Line[i] == '(' || Line[i] == '[')
                OpenedBrecketsIndexes.Add(i);
            else if(Line[i]== ')' || Line[i] == ']')
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
    public static Node SeparateLinear(string Line,int CurGlobalNDPos, Int64 ParentUID, bool IsBrackets)
    {   
        if(Line == String.Empty)
            return new Node(NodeTypes.NONE);

        string[] Sublines = Split(Line,out string? RSeparator);

        if(Sublines.Length == 1 && Sublines[0].StartsWith('#'))
        {
            int BracketCollIndex = int.Parse(Sublines[0].TrimStart('#'));
            string NormalazedLine = BrecketsColl[BracketCollIndex].Trim("()".ToCharArray());
            if(NormalazedLine.StartsWith('[') && NormalazedLine.EndsWith(']'))
                return SeparateLinear(NormalazedLine,CurGlobalNDPos,ParentUID,false);
            else
                return SeparateLinear(NormalazedLine,CurGlobalNDPos,ParentUID,true);
        }

        Node CurNode = new Node(ParentUID) {IsBrackets = IsBrackets};
        Nodes.Add(CurNode.UID,CurNode);
        if(RSeparator != null)
        {
            if(EqualsPosibilities.Contains(RSeparator))
            {
                CurNode.NodeType = NodeTypes.EQUALS;
                CurNode.Target = RSeparator;
                ND.Add(CurNode);
            }
            else
            {   
                if(RSeparator == " in ")
                    CurNode.NodeType = NodeTypes.IN;
                else
                    CurNode.NodeType = NodeTypes.OPERATOR;
                CurNode.Target = RSeparator;
            }
        }
        
        
        Node? FirstChild = null, SecoundChild = null;
        if(Sublines.Length != 1)
        {
            FirstChild = SeparateLinear(Sublines[0].Trim(),CurGlobalNDPos,CurNode.UID,false);
            SecoundChild = SeparateLinear(Sublines[1].Trim(),CurGlobalNDPos, CurNode.UID,false);
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

                for (int i = 0; i < Params.Length; i++)
                    CurNode.ChildNodes[i] = SeparateLinear(Params[i].Trim(),CurGlobalNDPos,CurNode.UID,false);
                return CurNode;
            }
            CurNode.ChildNodes = null;
            CurNode.Target = Sublines[0];
            CurNode.NodeType = GetNodeType(ref CurNode.Target,ParentUID, CurNode);
        }
        return CurNode;
    }

    static string[] Normalazing(string[] SourceCodeLines)
    {
        List<string> Lines = SourceCodeLines.ToList<string>();
        int CurTabsNum = 0;
        int Opened = 0, Closed = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i] = Lines[i].Replace("    ","\t");
            int TabsNum = Lines[i].Count((ch) => ch == '\t');
            if(TabsNum > CurTabsNum)
            {
                Lines.Insert(i,NodeTypes.START.ToString());
                Opened++;
            }
            else if(TabsNum < CurTabsNum)
            {
                Lines.Insert(i,NodeTypes.END.ToString());
                Closed++;
            }
            CurTabsNum = TabsNum;
            if(Lines[i].Contains('#'))
                Lines.RemoveAt(i);
            Lines[i] = Lines[i].Trim();
        }
        if(Opened - Closed == 1)
            Lines.Add(NodeTypes.END.ToString());
        return Lines.ToArray();
    }
    
    static Node IfConstructionSeparator(string Line,int CurGlobalNDPos, Int64 ParentUID)
    {
        string NormalizedIfConstruction = Regex.Match(Line,@"(?<=if)[( ]?[\w\W]+[) ]?(?=:)").Value.Trim("() ".ToCharArray());
        Node CurNode =  new Node(NodeTypes.IF,ParentUID);
        Nodes.Add(CurNode.UID,CurNode);
        ND.Add(CurNode);
        
        CurNode.ChildNodes = new Node[1];
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false);
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
        CurNode.ChildNodes[0] = SeparateLinear(NormalizedIfConstruction,CurGlobalNDPos,CurNode.UID,false);
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
                ND.Add(new Node(NodeTypes.IMPORT){Target = Lines[i].Split(' ')[1]});
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
                    SeparateLinear(Lines[i],i,i,false);
                    break;
            }
            BrecketsColl.Clear();                
        }
    }

}
