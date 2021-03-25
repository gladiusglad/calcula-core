namespace CalculaCore
{
    /// <summary>
    /// Represents an invalid token.
    /// <para>Index - The starting index of the invalid character(s) in the expression string.</para>
    /// </summary>
    public record InvalidToken(int Value) : IToken;
}

