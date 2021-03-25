namespace CalculaCore
{
    /// <summary>
    /// An <see cref="IOperation"/> implementation which represents a <c>decimal</c> number.
    /// </summary>
    public class Number : IOperation
    {
        public decimal N;

        /// <summary>
        /// Initializes a new instance of <see cref="Number"/> with a <c>decimal</c> number.
        /// </summary>
        /// <param name="n">The <c>decimal</c> number.</param>
        public Number(decimal n)
        {
            this.N = n;
        }

        public decimal Resolve()
        {
            return N;
        }
    }
}
