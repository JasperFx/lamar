using JasperFx.CommandLine;

namespace Lamar.CommandLine
{
    public class ValidateInput : NetCoreInput
    {
        [Description("'ConfigOnly' for only testing the configuration or 'Full' to also run environment tests" )] 
        public AssertMode Mode { get; set; } = AssertMode.Full;
    }
}