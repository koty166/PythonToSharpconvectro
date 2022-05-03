using TranslateLibrary;
using System;
using System.IO;
using System.Diagnostics;

namespace CLI;
class Program
{
    public static void Main()
    {
        string data = File.ReadAllText("Test.py");
        char[] Separators = "=+-.".ToCharArray();
        Stopwatch s = Stopwatch.StartNew();
        Core.GetTokenizatedText(data);
        Console.WriteLine(s.ElapsedMilliseconds);
    }
}