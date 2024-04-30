
using System.Text;
using System.Text.RegularExpressions;

namespace TranslateLibrary.CoreLib;



public class ParsingException : Exception
{
    public  ParsingException ():base("Ошибка разбора") {}
    public  ParsingException (string message):base(message) {}
}