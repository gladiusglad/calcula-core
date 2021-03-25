using System;
using System.Diagnostics.CodeAnalysis;

namespace CalculaCore
{
    /// <summary>
    /// An <see cref="IOperation"/> implementation which represents a mathematical operation with one operand.
    /// </summary>
    public class UnaryOperation : IOperation
    {
        public enum Function
        {
            Negate,
            Sqrt,
            Log,
            Sin,
            Cos,
            Tan,
            Asin,
            Acos,
            Atan,
            Ln,
            Abs,
            Sign,
            Factorial,
            Identity
        }

        private readonly IOperation Operand;
        private readonly Function Operator;

        /// <summary>
        /// Initializes a new instance of <see cref="UnaryOperation"/> with an operand and an operator.
        /// </summary>
        /// <param name="operand">The operand.</param>
        /// <param name="op">The operator.</param>
        public UnaryOperation([NotNull] IOperation operand, [NotNull] Function op)
        {
            Operand = operand;
            Operator = op;
        }

        public decimal Resolve()
        {
            decimal i = Operand.Resolve();
            return Operator switch
            {
                Function.Negate => -i,
                Function.Sqrt => DecimalMath.DecimalEx.Sqrt(i),
                Function.Log => DecimalMath.DecimalEx.Log10(i),
                Function.Sin => DecimalMath.DecimalEx.Sin(i),
                Function.Cos => DecimalMath.DecimalEx.Cos(i),
                Function.Tan => DecimalMath.DecimalEx.Tan(i),
                Function.Asin => DecimalMath.DecimalEx.ASin(i),
                Function.Acos => DecimalMath.DecimalEx.ACos(i),
                Function.Atan => DecimalMath.DecimalEx.ATan(i),
                Function.Ln => DecimalMath.DecimalEx.Log(i),
                Function.Abs => Math.Abs(i),
                Function.Sign => Math.Sign(i),
                Function.Identity => i,
                Function.Factorial => DecimalMath.DecimalEx.Factorial(i),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
