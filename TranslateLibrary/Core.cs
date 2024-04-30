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
        string Resautl = CodeGenerator.GenReport(CurrentParsing.RootNodes.ToArray());
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
        Regex ReplaceEmpty = new("(\"\")|(\'\')|(\\[\\])|(\\(\\))",RegexOptions.Compiled);
        Regex ReplaceNonEmptyQuotation = new("(\"[^\"]+\")|('[^']+')",RegexOptions.Compiled);
        Regex ReplaceNonEmptyArrBrackets = new(@"(\[[^\[\]]+\])",RegexOptions.Compiled);
        Regex ReplaceNonEmptyBrackets = new(@"\(([^()]+\))",RegexOptions.Compiled);
        
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
while(Line.Contains(")")||Line.Contains("()")){
        Line = ReplaceNonEmptyBrackets.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    CurrentParsing.PartsColl.Add(new Part(
                                        PartType.Brackets,
                                        MValue.Trim("()".ToCharArray())));

                                    return "#"+CurrPointer++; });
        }
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
            Lines[i] = Lines[i].Trim();
            if(!Lines[i].StartsWith("import")) Lines[i] = Lines[i].Replace(" ","");
            if(Lines[i] == "end") Lines[i] = NodeTypes.END.ToString();
            if(Lines[i].Contains('#'))
                Lines.RemoveAt(i);
        }
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
            if(Lines[i].Contains(')') || Lines[i].Contains('(') || Lines[i].Contains('[') || Lines[i].Contains(']') )
                throw new ParsingException("Неверная скобочная последовательность в строке " + Lines[i]);
            else if(Lines[i].Contains('"'))
                throw new ParsingException("Некорректное завершение текста в строке " + Lines[i]);
            else if(Lines[i].Contains('\''))
                throw new ParsingException("Недопустимый символ ' в строке " + Lines[i]);
            else if(Lines[i].StartsWith("="))
                throw new ParsingException("Отсутствует переменная в строке " + Lines[i]);
            else if(Lines[i].EndsWith("="))
                throw new ParsingException("Отсутствует правая часть " + Lines[i]);
            if(Lines[i].StartsWith("if"))
            {
                if(!Lines[i].EndsWith(":"))
                    throw new ParsingException("Отсутствует знак \":\" в строке " + Lines[i]);
                CurrentParsing.IfConstructionSeparator(Lines[i],i,i);
                continue;
            }
            else if(Lines[i].StartsWith("import"))
            {
                if(!Lines[i].Contains("as"))
                    throw new ParsingException("Отсутствует ключевое слово \"as\" в строке " + Lines[i]);
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
                default:
                    CurrentParsing.SeparateLinear(Lines[i],i,i,false,true);
                    break;
            }            
        }
    }

}
