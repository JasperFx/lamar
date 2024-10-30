
using JasperFx.CommandLine;

namespace Lamar.Diagnostics
{
    public class ValidateInput : NetCoreInput
    {
        [Description("'ConfigOnly' for only testing the configuration or 'Full' to also run environment tests" )] 
        public AssertMode Mode { get; set; } = AssertMode.Full;
    }
}