using TranslateLibrary.API;
using System;
using System.IO;
using System.Diagnostics;

namespace CLI;

class Program
{
    
    static void Main(string[] args)
    {
        string HELP = "Программа для трансляции кода из Python в C#.\n"+"Usage: P2CSharpTransl [--help] [-h] [-o <path>] [-i <path>]\n"+"-h --help - справка\n";
        if(args.Length > 4)
        {
            Console.WriteLine(HELP);
            return;
        }
        else if(args.Length > 0 &&( args[0] == "--help" || args[0] == "-h"))
        {
            Console.WriteLine(HELP);
        }
        
        string InputFilePath = string.Empty, OutputPath = string.Empty;

        bool O = false, I = false;
        foreach (var item in args)
        {
            if(item == "-o")
            {
                O = true;
                continue;
            }
            else if(item == "-i")
            {
                I = true;
                continue;
            }
            
            if(O)
            {
                InputFilePath = item;
                continue;
            }
            if(I)
            {
                OutputPath = item;
                continue;
            }            
        }
        string InputText = string.Empty, OutputText = string.Empty;

        if(I && File.Exists(InputFilePath))
            using(StreamReader r = new StreamReader(InputFilePath))
                InputText = r.ReadToEnd();
        else
        {
            string ReadBuf;
            do
            {
                ReadBuf = Console.ReadLine() ?? "0";
                if(ReadBuf != "0")
                    InputText+= ReadBuf + "\n";
            }while(ReadBuf != "0");
        }

        if(!API.Translate(InputText,out OutputText,out string Error))
        {
            Console.WriteLine("Во время трансляции произошла следующая ошибка:\n"+Error);
            return;
        }

        if(I && File.Exists(InputFilePath))
            using(StreamWriter w = new StreamWriter(InputFilePath))
                w.Write(OutputText);
        else
            Console.WriteLine(OutputText);

    }
}