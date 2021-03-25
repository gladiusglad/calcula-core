using System;
using System.Collections.Generic;
using System.Globalization;
using static CalculaCore.BinaryOperation;
using static CalculaCore.UnaryOperation;

namespace CalculaCore
{
    public partial class Calculator
    {
        private static readonly Dictionary<Operator, int> operatorPrecedence = new()
        {
            [Operator.Exponentiate] = 3,
            [Operator.Multiply] = 2,
            [Operator.Divide] = 2,
            [Operator.Modulo] = 2,
            [Operator.Add] = 1,
            [Operator.Subtract] = 1
        };

        private static readonly Dictionary<string, decimal> constants = new()
        {
            ["pi"] = DecimalMath.DecimalEx.Pi,
            ["e"] = DecimalMath.DecimalEx.E,
            ["phi"] = 1.6180339887498948482045868344m
        };

        private readonly Dictionary<string, decimal> variables = new(constants);

        /// <summary>
        /// Parse a <see cref="Token"/> <c>List</c> to a root <c>IOperation</c>.
        /// <para>Order of Operations</para>
        /// <list type="number">
        /// <item>
        /// <description>Brackets/parentheses</description>
        /// </item>
        /// <item>
        /// <description>Functions/unary operations</description>
        /// </item>
        /// <item>
        /// <description>Exponents</description>
        /// </item>
        /// <item>
        /// <description>Multiplication, Division, Modulo</description>
        /// </item>
        /// <item>
        /// <description>Addition, Subtraction</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="tokens">The <see cref="Token"/> <c>List</c> to parse.</param>
        /// <param name="enableAssignment">If the assignment operator should assign to a variable.</param>
        /// <returns>The root <see cref="IOperation"/> of the expression.
        /// Returns null if the Token list is invalid.</returns>
        public IOperation Parse(List<IToken> tokens)
        {
            if (tokens[^1] is InvalidToken)
            {
                return null;
            }

            if (tokens.Count == 1 && tokens[0] is NumberToken onlyNumber)
            {
                return new Number(ParseNumberToken(onlyNumber));
            }

            int i = 0,
                openBracketIndex = -1,
                openBrackets = 0;
            IOperation result = null;
            Operator? currentOp = null;
            Function? currentFunc = null;

            void Store(IOperation operation)
            {
                // Resolve current function if it exists
                IOperation current = (currentFunc == null) ? operation : new UnaryOperation(operation, currentFunc.Value);
                currentFunc = null;

                if (i < tokens.Count - 1 && tokens[i + 1] is FunctionToken functionToken && functionToken.Value == Function.Factorial)
                {
                    current = new UnaryOperation(current, Function.Factorial);
                }

                // If not the first operand and there are no operators
                // e.g. 3pi, 5sqrt(4), (3+5)2
                //      /\   /\            /\
                if (result != null && currentOp == null)
                {
                    // Assume multiplication
                    // e.g. 3*pi, 5*sqrt(4), (3+5)*2
                    currentOp = Operator.Multiply;
                }

                if (currentOp != null)
                {
                    // If operator is the first token in expression
                    if (result == null)
                    {
                        throw new ArgumentException($"Invalid operator token '{currentOp}'");
                    }

                    // If previous operation wasn't a binary operation or current operator isn't on the order of operations
                    if (!Precedence(result, current, currentOp.Value))
                    {
                        // Put the previous operand in a binary operation with the current operand
                        result = new BinaryOperation(result, current, currentOp.Value);
                    }

                    currentOp = null;
                }
                else if (result == null)
                {
                    // Operand is the first one in expression
                    result = current;
                }
            }

            // TODO Hacky recursive solution, should come up with a better one
            bool Precedence(IOperation root, IOperation current, Operator op)
            {
                // If previous operation was a binary operation and current operator is higher on the order of operations
                if (root is BinaryOperation rootBinary && operatorPrecedence[op] > operatorPrecedence[rootBinary.Op])
                {
                    // If there wasn't a higher binary operation before it
                    if (!Precedence(rootBinary.B, current, op))
                    {
                        // Set the previous operation's 2nd operand to a new binary operation,
                        // with the old 2nd operand as the 1st operand and the current operand as the 2nd operand
                        // P.S. the deeper the operation is in the tree, the earlier it will be calculated
                        rootBinary.B = new BinaryOperation(rootBinary.B, current, currentOp.Value);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }

            void StoreBrackets(int index)
            {
                // Wrap bracket operation in an identity function to prevent operator precedence
                // TODO Hacky solution, should come up with a better one
                Store(new UnaryOperation(Parse(tokens.GetRange(openBracketIndex + 1, index - openBracketIndex)), Function.Identity));
            }

            void StoreVariable(string variable)
            {
                if (variables.ContainsKey(variable))
                {
                    Store(new Number(variables[variable]));
                }
                else
                {
                    throw new ArgumentException($"Variable or constant '{variable}' not found");
                }
            }

            for (; i < tokens.Count; i++)
            {
                IToken token = tokens[i];

                if (token is ControlToken controlToken)
                {
                    if (controlToken.Value == ControlTokenType.BracketOpen)
                    {
                        // If open bracket is outermost
                        if (openBrackets == 0)
                        {
                            // Store current index
                            openBracketIndex = i;
                        }

                        openBrackets++;
                    }
                    else if (controlToken.Value == ControlTokenType.BracketOpen)
                    {
                        // Close the last open bracket
                        openBrackets--;

                        // If that was the last outermost open bracket or the end of the expression is reached
                        if (openBrackets == 0)
                        {
                            // Parse contents of outermost bracket and store
                            StoreBrackets(i - 1);
                        }
                        // If there were no open brackets to close
                        if (openBrackets == -1 && openBracketIndex == -1)
                        {
                            // Clear current data
                            result = null;
                            currentOp = null;
                            currentFunc = null;
                            openBrackets = 0;
                            // Parse from beginning of expression to closing bracket and store
                            Store(new UnaryOperation(Parse(tokens.GetRange(0, i)), Function.Identity));
                        }
                    }
                }
                

                // If not inside brackets
                if (openBrackets == 0)
                {
                    if (token is OperatorToken operatorToken)
                    {
                        Operator newOp = operatorToken.Value;
                        if (currentOp != null)
                        {
                            throw new ArgumentException($"Two operators in a row ({currentOp}, {newOp})");
                        }
                        currentOp = newOp;
                    }
                    if (token is FunctionToken functionToken && functionToken.Value != Function.Factorial)
                    {
                        Function newFunc = functionToken.Value;
                        if (currentFunc != null)
                        {
                            throw new ArgumentException($"Two functions in a row ({currentFunc}, {newFunc})");
                        }
                        currentFunc = newFunc;
                    }
                    if (token is VariableToken variableToken)
                    {
                        if (i == 0 && tokens.Count > 1 && tokens[1] is ControlToken firstAssign && firstAssign.Value == ControlTokenType.Assign)
                        {
                            IOperation assignValue = Parse(tokens.GetRange(2, tokens.Count - 2));
                            if (Options.EnableAssignment)
                            {
                                variables[variableToken.Value] = assignValue.Resolve();
                            }
                            return assignValue;
                        }
                        else
                        {
                            StoreVariable(variableToken.Value);
                        }
                    }
                    if (token is NumberToken numberToken)
                    {
                        Store(new Number(ParseNumberToken(numberToken)));
                    }
                    if (token is ControlToken assignToken && assignToken.Value == ControlTokenType.Assign)
                    {
                        throw new ArgumentException("Invalid assignment token '='");
                    }
                }
            }

            // If there are unclosed brackets
            // 3*(1
            // tokens.Count = 4
            // index = 4 - 1 = 3
            // openBracketIndex = 2
            // getRange index = 2 + 1 = 3
            // getRange count = 3 - 2 = 1
            if (openBrackets > 0)
            {
                // Parse from first open bracket to end of expression and store
                StoreBrackets(tokens.Count - 1);
            }

            if (currentFunc != null)
            {
                throw new ArgumentException($"Expected parameters for '{currentFunc}'");
            }
            if (currentOp != null)
            {
                throw new ArgumentException($"Expected operand for '{currentOp}'");
            }

            return result;
        }

        private decimal ParseNumberToken(NumberToken numberToken)
        {
            return decimal.Parse(numberToken.Value, CultureInfo.InvariantCulture);
        }
    }
}
