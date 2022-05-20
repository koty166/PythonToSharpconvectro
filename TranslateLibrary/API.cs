using System;
using System.Threading;
using TranslateLibrary;
using TranslateLibrary.CoreLib;

namespace TranslateLibrary.API;

public static class API
{
    public static bool Translate(string TextToTranslate, PostGenerationOptimizingT Opt, out string TranslatedText, out string Error)
    {
        if(TextToTranslate is null || TextToTranslate.Trim() == string.Empty)
        {
            Error = "Пустая входная строка.";
            TranslatedText = string.Empty;
            return false;
        }
        //try
        //{   
            
            TranslatedText = new Core().Translate(TextToTranslate as string, Opt);
            Error = String.Empty;
            return true;
        //}
        //catch(Exception ex)
        //{
            ///Error = ex.Message; 
            Error = "";
            TranslatedText  = String.Empty;
            return false;
       // }
    }
}