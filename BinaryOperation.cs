using System;
using System.Diagnostics.CodeAnalysis;

namespace CalculaCore
{
    /// <summary>
    /// An <see cref="IOperation"/> implementation which represents a mathematical operation with two operands.
    /// </summary>
    class BinaryOperation : IOperation
    {
        public enum Operator
        {
            Add,
            Subtract,
            Multiply,
            Divide,
            Exponentiate,
            Modulo,
            // TODO Implement logX(Y)
            Logarithm
        }

        public IOperation A;
        public IOperation B;
        public Operator Op;

        /// <summary>
        /// Initializes a new instance of <see cref="BinaryOperation"/> with two operands and an operator.
        /// </summary>
        /// <param name="a">The first operand.</param>
        /// <param name="b">The second operand.</param>
        /// <param name="op">The operator.</param>
        public BinaryOperation([NotNull] IOperation a, [NotNull] IOperation b, [NotNull] Operator op)
        {
            A = a;
            B = b;
            Op = op;
        }

        public decimal Resolve()
        {
            decimal a = A.Resolve(),
                b = B.Resolve();
            bool isInteger = a % 1 == 0 && b % 1 == 0;

            return Op switch
            {
                Operator.Add => a + b,
                Operator.Subtract => a - b,
                Operator.Modulo => a % b,
                // If both operands are integers, use decimal implementation for better range
                // If fractional, use double implementation for (mostly) better rounding with multiplication, division and higher operations
                Operator.Multiply => isInteger ? a * b : (decimal)((double)a * (double)b),
                Operator.Divide => isInteger ? a / b : (decimal)((double)a / (double)b),
                Operator.Exponentiate => isInteger ? DecimalMath.DecimalEx.Pow(a, b) : (decimal)Math.Pow((double)a, (double)b),
                Operator.Logarithm => isInteger ? DecimalMath.DecimalEx.Log(b, a) : (decimal)Math.Log((double)b, (double)a),
                _ => throw new NotImplementedException()
            };
        }
    }
}
