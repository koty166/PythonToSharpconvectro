
using System.Text;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;



public class AnalyzeException : Exception
{

    public string ErrorLine,ErrorText;
    public  AnalyzeException (string errLine):base("Ошибка разбора") {ErrorLine = errLine;}
    public  AnalyzeException (string message,string errLine,string ErrorText):base(message) {ErrorLine = errLine; this.ErrorText = ErrorText;}
}