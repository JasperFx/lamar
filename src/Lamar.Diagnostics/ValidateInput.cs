using Oakton;
using Oakton.AspNetCore;

namespace Lamar.Diagnostics
{
    public class ValidateInput : AspNetCoreInput
    {
        [Description("'ConfigOnly' for only testing the configuration or 'Full' to also run environment tests" )] 
        public AssertMode Mode { get; set; } = AssertMode.Full;
    }
}