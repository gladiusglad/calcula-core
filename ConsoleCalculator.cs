using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;

namespace CalculaCore
{
    class ConsoleCalculator
    {
        private static string previousResult;
        private static List<string> history = new();
        private static Calculator calculator = new Calculator();

        static int Main(string[] args)
        {
            string expression = "",
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);

            Console.WriteLine("\n" +
                $" Calcula {version}\n" +
                " Type in a mathematical expression,\n" +
                " e.g. \"2 * 100 / 5 + sqrt(6^2 - 32)\"\n" +
                " and press ENTER to calculate.\n" +
                " Hit ESCAPE at any time to quit."
                );
            do
            {
                expression = ReadLine();
                if (expression != null)
                {
                    if (expression.Length > 0)
                    {
                        history.Insert(0, expression);
                    }

                    if (history.Count > 30)
                    {
                        history.RemoveAt(history.Count - 1);
                    }

                    if (expression == "debug")
                    {
                        bool oldDebug = calculator.Options.Debug;
                        calculator.Options = new CalculatorOptions(Debug: !oldDebug);
                        WriteLineNonCalculation(oldDebug ? "Debug mode disabled." : "Debug mode enabled.");
                        continue;
                    }

                    try
                    {
                        string calculateTime = null;
                        decimal? result;
                        if (!calculator.Options.Debug)
                        {

                            DateTimeOffset startTime = DateTimeOffset.Now;
                            result = calculator.Calculate(expression);
                            calculateTime = DateTimeOffset.Now.Subtract(startTime).TotalMilliseconds.ToString("G") + "ms";
                        }
                        else
                        {
                            result = calculator.Calculate(expression);
                        }

                        if (result.HasValue)
                        {
                            previousResult = result.ToString();
                            if (!calculator.Options.Debug)
                            {
                                Console.WriteLine(previousResult + new string(' ', Console.BufferWidth - calculateTime.Length - previousResult.Length - 1) + calculateTime);
                            }
                            else
                            {
                                Console.WriteLine(previousResult);
                            }
                        }
                        else
                        {
                            WriteLineNonCalculation("Please enter a valid mathematical expression.");
                        }
                    }
                    catch (Exception _)
                    {
                        WriteLineNonCalculation("Please enter a valid mathematical expression.");
                    }
                }
                else
                {
                    Console.WriteLine("\n\nGoodbye!");
                }
            }
            while (expression != null);

            return 0;
        }

        private static void WriteLineNonCalculation(string value)
        {
            previousResult = null;
            Console.WriteLine(value);
        }

        private static string ReadLine()
        {
            string prefix = "Calcula >> ",
                compactPrefix = ">> ",
                current = null;
            int start = prefix.Length,
                historyIndex = 0;
            StringBuilder buffer = new();
            Console.Write("\n" + prefix);

            void Refresh(int cursorPos)
            {
                Console.CursorLeft = start;
                Console.Write(new string(' ', Console.BufferWidth - start - 1));
                Console.CursorLeft = start;
                Console.Write(buffer.ToString());
                Console.CursorLeft = cursorPos;
            }

            ConsoleKeyInfo key = Console.ReadKey(true);

            while (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Escape)
            {
                if (key.Key == ConsoleKey.Backspace && Console.CursorLeft > start)
                {
                    int originalPos = --Console.CursorLeft;
                    buffer.Remove(originalPos - start, 1);
                    Refresh(originalPos);
                }
                else if (key.Key == ConsoleKey.Delete && Console.CursorLeft < buffer.Length + start)
                {
                    int originalPos = Console.CursorLeft;
                    buffer.Remove(originalPos - start, 1);
                    Refresh(originalPos);
                }
                else if (key.Key == ConsoleKey.LeftArrow && Console.CursorLeft > start)
                {
                    Console.CursorLeft--;
                }
                else if (key.Key == ConsoleKey.RightArrow && Console.CursorLeft < buffer.Length + start)
                {
                    Console.CursorLeft++;
                }
                else if (key.Key == ConsoleKey.UpArrow)
                {
                    if (historyIndex < history.Count + (previousResult == null ? 0 : 1))
                    {
                        if (historyIndex == 0)
                        {
                            current = buffer.ToString();
                        }
                        historyIndex++;
                    }
                }
                else if (key.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex > 0)
                    {
                        historyIndex--;
                    }
                    else
                    {
                        current = buffer.ToString();
                    }
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    int originalPos = Console.CursorLeft;
                    if (originalPos - start == buffer.Length)
                    {
                        buffer.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                    else
                    {
                        buffer.Insert(originalPos - start, key.KeyChar);
                        Refresh(originalPos + 1);
                    }
                }
                if (key.Key == ConsoleKey.UpArrow || key.Key == ConsoleKey.DownArrow)
                {
                    if (historyIndex == 0)
                    {
                        buffer = new StringBuilder(current);
                    }
                    else
                    {
                        buffer = historyIndex == 1 && previousResult != null
                            ? new StringBuilder(previousResult)
                            : new StringBuilder(history[historyIndex - (previousResult == null ? 1 : 2)]);
                    }
                    Refresh(buffer.Length + start);
                }
                key = Console.ReadKey(true);
            }

            if (key.Key == ConsoleKey.Enter)
            {
                ChangeLine(compactPrefix + buffer.ToString(), 0);
                Console.WriteLine();
                return buffer.ToString();
            }
            return null;
        }

        private static void ChangeLine(string text, int linesAboveCursor)
        {
            if (text == null) return;
            int currentCursorLeft = Console.CursorLeft;
            int currentCursorTop = Console.CursorTop;
            Console.SetCursorPosition(0, currentCursorTop - linesAboveCursor);
            Console.WriteLine(text + new string(' ', Console.BufferWidth - text.Length));
            Console.SetCursorPosition(currentCursorLeft, currentCursorTop);
        }
    }
}
