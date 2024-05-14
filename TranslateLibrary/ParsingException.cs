
using System.Text;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;



public class ParsingException : Exception
{
    public string ErrorLine,ErrorText;
    public  ParsingException (string errLine):base("Ошибка разбора")  {ErrorLine = errLine;}
    public  ParsingException (string message,string errLine,string ErrorText):base(message)  {ErrorLine = errLine; this.ErrorText = ErrorText;}
}