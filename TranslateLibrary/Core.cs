using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;

public class Core
{

    /// <summary>
    /// Основной метод трансляции кода. В обоих случаях сохраняется форматирование.
    /// </summary>
    /// <param name="SourceCode">Исходный код на Python</param>
    /// <returns>Код на c#</returns>
    public string Translate(object? SourceCode, PostGenerationOptimizingT Opt)
    {
        CurrentParsing = new Parse()
                                {RootNodes = new List<Node>(),
                                 PartsColl = new List<Part>()};
        GenNodes(SourceCode as string);
        string Resautl = CodeGenerator.GenCode(CurrentParsing.RootNodes.ToArray(),Opt);
        return Resautl;
    }
    Parse CurrentParsing;
    
    void RepaceBreckets(ref string Line)
    {
        //Перенести в какйо-нибудь отдельный класс для пре-инициализации.
        Regex ReplaceEmpty = new Regex("(\"\")|(\'\')|(\\[\\])|(\\(\\))",RegexOptions.Compiled);
        Regex ReplaceNonEmptyQuotation = new Regex("(\"[^\"]+\")|('[^']+')",RegexOptions.Compiled);
        Regex ReplaceNonEmptyArrBrackets = new Regex(@"(\[[^\[\]]+\])",RegexOptions.Compiled);
        Regex ReplaceNonEmptyBrackets = new Regex(@"\(([^()]+\))",RegexOptions.Compiled);
        
        int CurrPointer = CurrentParsing.PartsColl.Count;
        
        Line =  ReplaceEmpty.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    CurrentParsing.PartsColl.Add(new Part(
                                        (m.Value == "()" ? PartType.Brackets: PartType.Constant),
                                        MValue));
                                    return "#"+CurrPointer++; });

        Line = ReplaceNonEmptyQuotation.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

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
    
    string[] Normalazing(string[] SourceCodeLines)
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
   
    void GenNodes(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        Lines = Normalazing(Lines);

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
            RepaceBreckets(ref Lines[i]);
            if(Lines[i].StartsWith("if"))
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
