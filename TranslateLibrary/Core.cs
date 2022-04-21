using IronPython.Hosting;
using IronPython;
using IronPython.Modules;
using Microsoft.Scripting.Hosting;

namespace TranslateLibrary;

public class Core
{
    /// <summary>
    /// Основной метод трансляции кода. В обоих случаях сохраняется форматирование.
    /// </summary>
    /// <param name="SourceCode">Исходный код на Python</param>
    /// <returns>Код на c#</returns>
    public string Translate(string SourceCode)
    {
        return string.Empty;
        
    }
    public String GetTokenizatedText(string SourceCode)
    {
        return null;
    }
}
