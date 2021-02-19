using System;
using LamarCodeGeneration.Util;
using Oakton;
using Spectre.Console;

namespace Lamar.Diagnostics
{
    [Description("Runs all the Lamar container validations", Name = "lamar-validate")]
    public class LamarValidateCommand : OaktonCommand<ValidateInput>
    {
        public LamarValidateCommand()
        {
            Usage("Full Validation").Arguments();
            Usage("Validation Mode").Arguments(x => x.Mode);
        }

        public override bool Execute(ValidateInput input)
        {
            AnsiConsole.Render(new FigletText("Lamar"){Color = Color.Blue});

            using (var host = input.BuildHost())
            {
                var container = host.Services.As<IContainer>();
                
                container.AssertConfigurationIsValid(input.Mode);
                
                ConsoleWriter.Write(ConsoleColor.Green, "Lamar registrations are all good!");
            }

            return true;
        }
    }
}