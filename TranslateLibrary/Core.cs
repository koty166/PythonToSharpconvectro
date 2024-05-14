using IronPython.Runtime.Operations;
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
    void RepaceBreckets(ref string Line,string srcline)
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
                                    int k = 0;
while(Line.Contains(")")||Line.Contains("(")){
        Line = ReplaceNonEmptyBrackets.Replace(Line,(Match m) => { 
                                    string MValue = m.Value;

                                    CurrentParsing.PartsColl.Add(new Part(
                                        PartType.Brackets,
                                        MValue.Trim("()".ToCharArray())));

                                    return "#"+CurrPointer++; });
                                    k++;
                                    if(k > 100) throw new ParsingException("Неверная скобочная последовательность в строке ", srcline,null);
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
            //if(!Lines[i].StartsWith("import")) Lines[i] = Lines[i].Replace(" ","");
            if(Lines[i] == "end") Lines[i] = NodeTypes.END.ToString();

        }
        return Lines.ToArray();
    }
   
    void GenNodes(string SourceCode)
    {
        string[] Lines = SourceCode.Split("\n",StringSplitOptions.RemoveEmptyEntries);
        string[] SrcLines = Lines;
        Lines = Normalazing(Lines);
        Regex Digits = new("[0-9]+\\ [0-9]+",RegexOptions.Compiled);

        

        for (int i = 0; i < Lines.Length; i++)
        {
            Lines[i] = Lines[i].Trim();
            if(Lines[i].EndsWith("+"))
                throw new ParsingException("Синтаксическая ошиибка. Оператор + на конце строки" , SrcLines[i],"+");
            else if(Lines[i].EndsWith("-"))
                throw new ParsingException("Синтаксическая ошиибка. Оператор - на конце строки" , SrcLines[i],"-");
            else if(Lines[i].EndsWith("**"))
                throw new ParsingException("Синтаксическая ошиибка. Оператор ** на конце строки" , SrcLines[i],"**");
            else if(Lines[i].EndsWith("/"))
                throw new ParsingException("Синтаксическая ошиибка. Оператор / на конце строки" , SrcLines[i],"/");
            else if(Lines[i].EndsWith("*"))
                throw new ParsingException("Синтаксическая ошиибка. Оператор * на конце строки" , SrcLines[i],"*");

            else if(Lines[i].Contains("++"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд ++" , SrcLines[i],"++");
            else if(Lines[i].Contains("--"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд --" , SrcLines[i],"--");
            else if(Lines[i].Contains("****"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд ** **" , SrcLines[i],"****");
            else if(Lines[i].Contains("//"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд //" , SrcLines[i],"//");

            else if(Lines[i].Contains("+-"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд +-" , SrcLines[i],"+-");
            else if(Lines[i].Contains("-+"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд -+" , SrcLines[i],"-+");
            else if(Lines[i].Contains("+/"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд +/" , SrcLines[i],"+/");
            else if(Lines[i].Contains("/+"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд /+" , SrcLines[i],"/+");

            else if(Lines[i].Contains("+**"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд +**" , SrcLines[i],"+**");
            else if(Lines[i].Contains("**+"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд **+" , SrcLines[i],"**+");
            else if(Lines[i].Contains("+*"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд +*" , SrcLines[i],"+*");
            else if(Lines[i].Contains("*+"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд *+" , SrcLines[i],"*+");
            
            


            else if(Lines[i].Contains("**-"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд **-" , SrcLines[i],"**-");
            else if(Lines[i].Contains("-**"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд -**" , SrcLines[i],"-**");

            else if(Lines[i].Contains("/-"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд -" , SrcLines[i],"/-");
            else if(Lines[i].Contains("-/"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд -/" , SrcLines[i],"-/");
            else if(Lines[i].Contains("-*"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд -*" , SrcLines[i],"-*");
            else if(Lines[i].Contains("*-"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд *-" , SrcLines[i],"*-");

            else if(Lines[i].Contains("**/"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд **/" , SrcLines[i],"**/");
            else if(Lines[i].Contains("/**"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд /**" , SrcLines[i],"/**");
                
            else if(Lines[i].Contains("/*"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд /*" , SrcLines[i],"/*");
            else if(Lines[i].Contains("*/"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд */" , SrcLines[i],"*/");

            else if(Lines[i].Contains("***"))
                throw new ParsingException("Синтаксическая ошиибка. Два оператора подряд ***" , SrcLines[i],"****");



            else if(Lines[i].Contains(",]"))
                throw new ParsingException("Ошибка: Запятая не может быть перед квадратной скобкой" , SrcLines[i],"]");
            else if(Lines[i].Contains("[,"))
                throw new ParsingException("Ошибка: Запятая не может быть перед квадратной скобкой" , SrcLines[i],"[");


            bool IsFound = Digits.Match(Lines[i]).Success;
            if(IsFound)
                throw new ParsingException($"Ошибка. Два числа идут подряд", SrcLines[i],Digits.Match(Lines[i]).Value);

            if(Lines[i].StartsWith("if"))
            {
                int pos = Lines[i].find("=");
                if(pos != -1 && Lines[i][pos+1]!='=')
                    throw new ParsingException("Недопустимый знак = в условии" , SrcLines[i],"=");
            }

            RepaceBreckets(ref Lines[i],SrcLines[i]);
            if(Lines[i].Contains(')') || Lines[i].Contains('(') || Lines[i].Contains('[') || Lines[i].Contains(']') )
                throw new ParsingException("Неверная скобочная последовательность в строке ", SrcLines[i],Regex.Match(Lines[i],"\\(|\\)|\\[|\\]").Value);
            else if(Lines[i].Contains('"'))
                throw new ParsingException("Некорректное завершение текста в строке " , SrcLines[i],"\"");
            else if(Lines[i].Contains('\''))
                throw new ParsingException("Недопустимый символ ' в строке " , SrcLines[i],"\'");
            else if(Lines[i].StartsWith("="))
                throw new ParsingException("Отсутствует переменная в строке " , SrcLines[i],null);
            else if(Lines[i].EndsWith("="))
                throw new ParsingException("Отсутствует правая часть " , SrcLines[i],null);
            

            

            if(Lines[i].StartsWith("if"))
            {   
                if(!Lines[i].EndsWith(":"))
                    throw new ParsingException("Отсутствует знак \":\" в строке " , SrcLines[i],null);
                CurrentParsing.IfConstructionSeparator(Lines[i],i,i,SrcLines[i]);
                continue;
            }
            else if(Lines[i].StartsWith("import"))
            {
                if(!Lines[i].Contains("as"))
                    throw new ParsingException("Отсутствует ключевое слово \"as\" в строке " , SrcLines[i],null);
                CurrentParsing.RootNodes.Add(new Node(NodeTypes.IMPORT){Target = Lines[i],SrcLine = SrcLines[i]});
                continue;
            }
            switch(Lines[i])
            {
                case "START":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.START){SrcLine = SrcLines[i]});
                    break;
                case "END":
                   CurrentParsing.RootNodes.Add(new Node(NodeTypes.END){SrcLine = SrcLines[i]});
                    break;
                default:
                    CurrentParsing.SeparateLinear(Lines[i],i,i,false,true,SrcLines[i]);
                    break;
            }            
        }
    }

}
