using System;
using System.Threading.Tasks;
using Lamar.Codegen;
using Lamar.Codegen.Variables;
using Lamar.Util;

namespace Lamar.Compilation
{
    public static class SourceWriterExtensions
    {
        private static readonly string returnCompletedTask = $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";

        private static readonly string returnFromResult =
            $"return {typeof(Task).FullName}.{nameof(Task.FromResult)}({{0}});";

        public static void Namespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"BLOCK:namespace {@namespace}");
        }

        public static void UsingNamespace<T>(this ISourceWriter writer)
        {
            writer.Write($"using {typeof(T).Namespace};");
        }

        public static void UsingBlock(this ISourceWriter writer, string declaration, Action<ISourceWriter> inner)
        {
            writer.Write($"BLOCK:using ({declaration})");

            inner(writer);

            writer.FinishBlock();
        }

        public static void WriteReturnStatement(this ISourceWriter writer, GeneratedMethod method)
        {
            if (method.AsyncMode == AsyncMode.AsyncTask)
            {
                writer.WriteLine("return;");
            }
            else
            {
                writer.WriteLine(returnCompletedTask);
            }
        }

        public static void WriteReturnStatement(this ISourceWriter writer, GeneratedMethod method, Variable variable)
        {
            writer.WriteLine(method.AsyncMode == AsyncMode.AsyncTask
                ? $"return {variable.Usage};"
                : returnFromResult.ToFormat(variable.Usage));
        }

        public static void WriteComment(this ISourceWriter writer, string comment)
        {
            writer.Write("// " + comment);
        }


    }
}
