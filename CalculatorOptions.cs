using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalculaCore
{
    public record CalculatorOptions(
        bool EnableAssignment = true,
        bool Debug = false,
        bool StandardizeSymbols = false
        );
}
