namespace CalculaCore
{
    public enum ControlTokenType
    {
        BracketOpen,
        BracketClose,
        Assign
    }

    public record ControlToken(ControlTokenType Value) : IToken;
}

