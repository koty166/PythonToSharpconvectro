namespace TranslateLibrary;

/// <summary>
/// Представляет собой часть строки, может быть комментарием, скобкой...
/// </summary>
class Part
{
    /// <summary>
    /// Тип подстроки
    /// </summary>
    public PartType Type;
    /// <summary>
    /// Значение подстроки.
    /// </summary>
    public string? Target;

    public Part()
    {
        Target = null;
    }
    public Part(PartType _Type, string _Target)
    {
        Type = _Type;
        Target = _Target;
    }
    public Part(PartType _Type)
    {
        Type = _Type;
    }
    public Part(string _Target)
    {
        Target = _Target;
    }
}