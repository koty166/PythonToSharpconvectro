namespace TranslateLibrary;

public class PostGenerationReplacement
{
    public static string PostTypeCasting(string Type)
    {
        if(Type == "int" || Type == "str" || Type == "float"  || Type == "bool")
            return $"({( Type == "str" ? "string" : Type )})";
        
        return Type;
    }
}