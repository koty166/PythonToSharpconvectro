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
        Stopwatch s = Stopwatch.StartNew();
        string res = Core.Translate(data);
        Console.WriteLine(s.ElapsedMilliseconds);

        using(StreamWriter w = new StreamWriter("outTEst.txt",false))
        {
            w.Write(res);
        }
    }
}