using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace CalculaCore
{
    public partial class Calculator
    {
        public CalculatorOptions Options;

        public Calculator(CalculatorOptions options = null)
        {
            Options = options ?? new CalculatorOptions();
        }

        /// <summary>Calculates a mathematical expression.</summary>
        /// <param name="expression">The expression to calculate.</param>
        /// <param name="options">Optional calculator options to pass. Passing also changes the <see cref="Options"/> property.</param>
        /// <returns>The result of the calculation as a <c>decimal?</c>.
        /// Returns <c>null</c> if the expression is invalid.</returns>
        public decimal? Calculate(string expression, CalculatorOptions options = null)
        {
            if (options != null) Options = options;

            if (expression == null || expression.Length == 0)
            {
                return null;
            }

            if (!Options.Debug)
            {
                // If string can be parsed outright as a decimal
                if (decimal.TryParse(StandardizeDecimal(expression), NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
                {
                    return NormalizeDecimal(result);
                }
            }

            if (Options.Debug)
            {
                DateTimeOffset startTime = DateTimeOffset.Now;
                string clean = CleanExpression(expression);
                DateTimeOffset cleanTime = DateTimeOffset.Now;

                List<IToken> tokenize = Tokenize(clean);
                DateTimeOffset tokenizeTime = DateTimeOffset.Now;

                IOperation parse = Parse(tokenize);
                DateTimeOffset parseTime = DateTimeOffset.Now;

                decimal resolve = (parse?.Resolve()).Value;
                DateTimeOffset resolveTime = DateTimeOffset.Now;
                Console.WriteLine($"Clean: {cleanTime.Subtract(startTime).TotalMilliseconds}ms");
                Console.WriteLine($"Tokenize: {tokenizeTime.Subtract(cleanTime).TotalMilliseconds}ms");
                Console.WriteLine($"Parse: {parseTime.Subtract(tokenizeTime).TotalMilliseconds}ms");
                Console.WriteLine($"Resolve: {resolveTime.Subtract(parseTime).TotalMilliseconds}ms");

                return NormalizeDecimal(resolve);
            }
            else
            {
                return NormalizeDecimal((Parse(Tokenize(CleanExpression(expression)))?.Resolve()).Value);
            }
        }

        /// <summary>Trims and standardizes an expression.</summary>
        /// <param name="expression">The expression to clean.</param>
        public string CleanExpression([NotNull] string expression)
        {
            string halfCleaned = StandardizeDecimal(ClearWhitespace(expression.ToLowerInvariant()));
            return Options.StandardizeSymbols ? StandardizeSymbols(halfCleaned) : halfCleaned;
        }

        private static string ReplaceEach(string expression, Dictionary<Regex, string> replacements)
        {
            foreach (KeyValuePair<Regex, string> entry in replacements)
            {
                expression = entry.Key.Replace(expression, entry.Value);
            }

            return expression;
        }

        // Instantiate regexes statically - they're expensive
        private static readonly Regex whitespaceRegex = new(@"\s");

        /// <summary>Removes all whitespace in the expression.</summary>
        /// <param name="expression">The expression to clean.</param>
        public static string ClearWhitespace([NotNull] string expression)
        {
            return whitespaceRegex.Replace(expression, "");
        }

        private static readonly Regex decimalRegex = new(",");

        /// <summary>Replaces all decimal separators with periods ('.').</summary>
        /// <param name="expression">The expression to standardize.</param>
        public static string StandardizeDecimal([NotNull] string expression)
        {
            return decimalRegex.Replace(expression, ".");
        }

        private static readonly Dictionary<Regex, string> symbolReplacements = new()
        {
            // Replace symbols with standard characters
            [new Regex("π")] = "pi",
            [new Regex("ℇ")] = "e",
            [new Regex("φ")] = "phi",
            [new Regex("−")] = "-",
            [new Regex("×")] = "*",
            [new Regex("÷")] = "/",
            [new Regex(@"√(?:(?:\((.+?)\))|(-?\d+(?:\.\d+)?))")] = "sqrt($1$2)",
        };

        /// <summary>Replaces all (Unicode) mathematical symbols with their standard forms.</summary>
        /// <param name="expression">The expression to standardize.</param>
        public static string StandardizeSymbols([NotNull] string expression)
        {
            return ReplaceEach(expression, symbolReplacements);
        }

        /// <summary>Trims all trailing zeros of a <c>decimal</c>.</summary>
        /// <param name="number">The decimal to normalize.</param>
        public static decimal NormalizeDecimal(decimal number)
        {
            return number / 1.000000000000000000000000000000000m;
        }
    }
}
