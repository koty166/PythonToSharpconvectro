namespace TranslateLibrary.CoreLib;

internal class PostGenerationReplacement
{
    internal static string ConditionalСonstructsReplace(string Type)
    {
        return Type;
    }
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
                    return "new List";
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
                    return ".ToList()";
                case "range":
                    return "new Range";
                case "sorted":
                    IsRight = false;
                    return ".ToList().Sort()";
                case "sum":
                    IsRight = false;
                    return ".ToList().Sum()";
                case "type":
                    return "typeof";
                case "abs":
                    return "Math.Abs";
                
            }   
        }
        return Type;
    }
    internal static string ReplaceLogicalOperators(string Target)
    {
        switch(Target)
        {
            case " AND ":
            case " and ":
                return " && ";
            case " OR ":
            case " or ":
                return " || ";
        }
        return Target;
    }
}