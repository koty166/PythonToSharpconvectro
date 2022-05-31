namespace TranslateLibrary;

/// <summary>
/// Представляет собой часть строки, может быть комментарием, скобкой...
/// </summary>
public class Part
{
    /// <summary>
    /// Тип подстроки.
    /// </summary>
    public PartType Type;
    /// <summary>
    /// Значение подстроки.
    /// </summary>
    public string Target;

    public Part()
    {
        Target = String.Empty;
    }
    public Part(PartType _Type, string _Target) : this()
    {
        Type = _Type;
        Target = _Target;
    }
    public Part(PartType _Type) : this()
    {
        Type = _Type;
    }
    public Part(string _Target) : this()
    {
        Target = _Target;
    }

}