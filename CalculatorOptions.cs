namespace CalculaCore
{
    public record CalculatorOptions(
        bool EnableAssignment = true,
        bool Debug = false,
        bool StandardizeSymbols = false
        );
}
