using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;

public class Core
{

    public Core()
    {
        CurrentParsing = new Parse();
    }
    public string Translate(string SourceCode)
    {
        GenNodes(SourceCode);
        string Resautl = CodeGenerator.GenCode(CurrentParsing.RootNodes.ToArray());
        return Resautl;
    }
    Parse CurrentParsing;
    
    PartType GetPartType(string s)
    {
        switch (s)
        {
            case "()":
                return PartType.Brackets;
            case "\"\"":
            case "\'\'":
                return PartType.Constant;
            case "[]":
                return PartType.Array;
        }
        return PartType.None;
                                            
    }
    void RepaceBreckets(ref string Line)
    {
        Regex ReplaceEmpty = new Regex("(\"\")|(\'\')|(\\[\\])|(\\(\\))",RegexOptions.Compiled);
        Regex ReplaceNonEmptyQuotation = new Regex("(\"[^\"]+\")|('[^']+')",RegexOptions.Compiled);
        Regex ReplaceNonEmptyArrBrackets = new Regex(@"(\[[^\[\]]+\])",RegexOptions.Compiled);
        Regex ReplaceNonEmptyBrackets = new Regex(@"\(([^()]+\))",RegexOptions.Compiled);
        
        int CurrPointer = CurrentParsing.PartsColl.Count;
        
        Line =  ReplaceEmpty.Replace(Line,(Match m) => { 
                                    string MValue = m.Value.Trim("()[]".ToCharArray());

                                    CurrentParsing.PartsColl.Add(new Part( 
                                        GetPartType(m.Value),
                                        MValue));
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyQuotation.Replace(Line,(Match m) => { 
                                    string MValue = "\""+(m.Value.Trim("\'\"".ToArray())) + "\"";

                                    CurrentParsing.PartsColl.Add(new Part(
                                        PartType.Constant,
                                        MValue));
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyArrBrackets.Replace(Line,(Match m) => { 
                                   string MValue = m.Value;

                                    CurrentParsing.PartsColl.Add(new Part(
                                        PartType.Array,
                                        MValue.Trim("[]".ToCharArray())));
                                         
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyBrackets.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    CurrentParsing.PartsColl.Add(new Part(
                                        PartType.Brackets,
                                        MValue.Trim("()".ToCharArray())));

                                    return "#"+CurrPointer++; });
    }
    string ReplaceSomeConsts(string Line) =>
        Line.Replace("True","true").Replace("False","false");
    string[] Normalazing(string[] SourceCodeLines)
    {
        List<string> Lines = SourceCodeLines.ToList<string>();
        int CurTabsNum = 0;
        int Opened = 0, Closed = 0;
        for (int i = 0; i < Lines.Count; i++)
        {
            Lines[i] = ReplaceSomeConsts(Lines[i]);
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
        }
        if(Opened - Closed == 1)
            Lines.Add(NodeTypes.END.ToString());
        return Lines.ToArray();
    }
   
    void GenNodes(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        Lines = Normalazing(Lines);

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
            RepaceBreckets(ref Lines[i]);
            if(Lines[i].StartsWith("if") || Lines[i].StartsWith("elif") || Lines[i].StartsWith("else"))
            {
                CurrentParsing.IfConstructionSeparator(Lines[i],i,i);
                continue;
            }
            else if(Lines[i].StartsWith("for"))
            {
                CurrentParsing.ForConstructionSeparator(Lines[i],true,i,i);
                continue;
            }
            else if(Lines[i].StartsWith("while"))
            {
                CurrentParsing.ForConstructionSeparator(Lines[i],false,i,i);
                continue;
            }
            else if(Lines[i].StartsWith("import") || Lines[i].StartsWith("from"))
            {
               CurrentParsing.RootNodes.Add(new Node(NodeTypes.IMPORT){Target = Lines[i]});
                continue;
            }
            switch(Lines[i])
            {
                case "START":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.START));
                    break;
                case "END":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.END));
                    break;
                case "continue":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.CONTINUE));
                    break;
                case "break":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.BREAK));
                    break;
                default:
                    CurrentParsing.SeparateLinear(Lines[i],i,i,false,true);
                    break;
            }            
        }
    }

}
