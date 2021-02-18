using Oakton;

namespace LamarCodeGeneration.Commands
{
    public class GenerateCodeInput : NetCoreInput
    {
        [Description("Action to take ")]
        public CodeAction Action { get; set; } = CodeAction.preview;
        
        [Description("Optionally limit the preview to only one type of code generation")]
        public string TypeFlag { get; set; }
    }
}