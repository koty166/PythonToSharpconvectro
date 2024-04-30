
using System.Text;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;



public class AnalyzeException : Exception
{
    public  AnalyzeException ():base("Ошибка разбора") {}
    public  AnalyzeException (string message):base(message) {}
}