using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace CalculaCore
{
    public partial class Calculator
    {
        // Instantiate regexes statically - they're expensive
        // Matches floating point number with optional negative sign
        private static readonly Regex numberRegex = new(@"^-?(\d*\.)?\d+"),
            // Matches a group of letters
            wordRegex = new(@"^[a-z]+");

        /// <summary>Generate tokens from an expression string.</summary>
        /// <param name="expression">The expression to tokenize.</param>
        /// <returns>A <see cref="Token"/> <c>List</c>.
        /// Contains a <see cref="TokenType.Invalid"/> token if the expression is invalid.</returns>
        public static List<Token> Tokenize([NotNull] string expression)
        {
            List<Token> tokens = new();

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                // Single character tokens, excluding OpSubtract
                switch (c)
                {
                    case '(':
                        tokens.Add(new Token(TokenType.BracketOpen));
                        continue;
                    case ')':
                        tokens.Add(new Token(TokenType.BracketClose));
                        continue;
                    case '+':
                        tokens.Add(new Token(TokenType.OpAdd));
                        continue;
                    case '*':
                        tokens.Add(new Token(TokenType.OpMultiply));
                        continue;
                    case '/':
                        tokens.Add(new Token(TokenType.OpDivide));
                        continue;
                    case '^':
                        tokens.Add(new Token(TokenType.OpExponentiate));
                        continue;
                    case '%':
                        tokens.Add(new Token(TokenType.OpModulo));
                        continue;
                    case '=':
                        tokens.Add(new Token(TokenType.Assign));
                        continue;
                    case '!':
                        tokens.Add(new Token(TokenType.FuncFactorial));
                        continue;
                    default:
                        break;
                }

                // Handle subtract operator, negation, and positive and negative numbers 
                if (c == '-' || Char.IsNumber(c))
                {
                    // If '-' and not the first character
                    if (c == '-' && i > 0)
                    {
                        // If previous character is an operator, then '-' indicates negation OR a negative number 
                        // Else, it indicates a subtraction operator
                        char prev = expression[i - 1];
                        switch (prev)
                        {
                            case '+':
                            case '-':
                            case '*':
                            case '/':
                            case '^':
                            case '%':
                            case '(':
                            case '=':
                                break;
                            default:
                                tokens.Add(new Token(TokenType.OpSubtract));
                                continue;
                        }
                    }

                    Match numberMatch = numberRegex.Match(expression[i..]);

                    // If a number is found from this character
                    if (numberMatch.Success)
                    {
                        string numberString = numberMatch.Value;
                        tokens.Add(new Token(TokenType.Number, numberString));

                        // Skip to the end of the number and continue to next char
                        i += numberString.Length - 1;
                    }
                    // Else, '-' indicates a negation function
                    else
                    {
                        tokens.Add(new Token(TokenType.FuncNegate));
                    }

                    continue;
                }

                // Handle letters - functions and constants
                if (Char.IsLetter(c))
                {
                    Match wordMatch = wordRegex.Match(expression[i..]);
                    string wordString = wordMatch.Value;
                    bool isFunction = true;

                    // If group of letters is followed by an opening bracket
                    if (i + wordMatch.Length < expression.Length && expression[i + wordMatch.Length] == '(')
                    {
                        // Check if group of letters is function
                        switch (wordString)
                        {
                            case "sqrt":
                                tokens.Add(new Token(TokenType.FuncSqrt));
                                break;
                            case "log":
                                tokens.Add(new Token(TokenType.FuncLog));
                                break;
                            case "sin":
                                tokens.Add(new Token(TokenType.FuncSin));
                                break;
                            case "asin":
                                tokens.Add(new Token(TokenType.FuncAsin));
                                break;
                            case "cos":
                                tokens.Add(new Token(TokenType.FuncCos));
                                break;
                            case "acos":
                                tokens.Add(new Token(TokenType.FuncAcos));
                                break;
                            case "tan":
                                tokens.Add(new Token(TokenType.FuncTan));
                                break;
                            case "atan":
                                tokens.Add(new Token(TokenType.FuncAtan));
                                break;
                            case "ln":
                                tokens.Add(new Token(TokenType.FuncLn));
                                break;
                            case "abs":
                                tokens.Add(new Token(TokenType.FuncAbs));
                                break;
                            case "sign":
                                tokens.Add(new Token(TokenType.FuncSign));
                                break;
                            default:
                                isFunction = false;
                                break;
                        }
                    }
                    else
                    {
                        isFunction = false;
                    }

                    // If group of letters is a constant
                    if (!isFunction)
                    {
                        tokens.Add(new Token(TokenType.Variable, wordString));
                    }

                    // Skip to the end of the letter group and continue to next char
                    i += wordString.Length - 1;
                    continue;
                }

                // If all else fails, tokenize the char as invalid and stop tokenizing
                tokens.Add(new Token(TokenType.Invalid, c.ToString()));
                break;
            }

            return tokens;
        }
    }
}
