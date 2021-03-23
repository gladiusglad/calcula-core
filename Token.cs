namespace CalculaCore
{
    public enum TokenType
    {
        BracketOpen,
        BracketClose,
        OpAdd,
        OpSubtract,
        OpMultiply,
        OpDivide,
        OpExponentiate,
        OpModulo,
        OpLogarithm,
        FuncNegate,
        FuncSqrt,
        FuncLog,
        FuncSin,
        FuncCos,
        FuncTan,
        FuncAsin,
        FuncAcos,
        FuncAtan,
        FuncLn,
        FuncAbs,
        FuncSign,
        FuncFactorial,
        Variable,
        Assign,
        Number,
        Invalid
    }

    public record Token(TokenType Type, string Value)
    {

        public Token(TokenType Type) : this(Type, null)
        {
            this.Type = Type;
        }

        public bool IsOperatorToken()
        {
            return (int)Type >= 2 && (int)Type <= 8;
        }

        public bool IsFunctionToken()
        {
            return (int)Type >= 9 && (int)Type <= 21;
        }

        public bool IsBracketToken()
        {
            return Type == TokenType.BracketOpen || Type == TokenType.BracketClose;
        }
    }
}
