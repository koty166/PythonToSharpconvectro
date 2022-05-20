namespace TranslateLibrary.CoreLib;

internal class PostGenerationReplacement
{
    internal static string PostGenerationOptimizing(string Type , PostGenerationOptimizingT Opt, out bool IsRight)
    {
        IsRight = true;
        switch(Type)
        {
            case "int":
                return "(int)";
            case "str":
                return "(string)";
            case "float":
                return "(float)";
            case "bool":
                return "(int)";
        }
        if(Opt >=  PostGenerationOptimizingT.Simple)
        {
            switch(Type)
            {
                case "chr":
                    return "(char)";
                case "ord":
                    return "(int)";
                case "dict":
                    return "new List<object>";
                case "hash":
                    IsRight = false;
                    return ".GetHashCode()";
                case "input":
                    return "Console.ReadLine";
                case "print":
                    return "Console.WriteLine";
                case "max":
                    IsRight = false;
                    return ".Max()";
                case "min":
                    IsRight = false;
                    return ".Min()";
                case "len":
                    IsRight = false;
                    return ".Count()";
                case "list":
                    IsRight = false;
                    return ".ToList<object>()";
                case "range":
                    return "new Range";
                case "sorted":
                    IsRight = false;
                    return ".ToList<object>().Sort()";
                case "sum":
                    IsRight = false;
                    return ".ToList<object>().Sum()";
                case "type":
                    return "typeof";
                case "abs":
                    return "Math.Abs";
                
            }   
        }
        return Type;
    }
}