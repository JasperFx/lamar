using System;
using System.Linq;
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

        /// <summary>
        /// Adds a "namespace [@namespace]" declaration into the code, and starts a new
        /// code block with a leading '{' character
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="namespace"></param>
        public static void Namespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"BLOCK:namespace {@namespace}");
        }

        /// <summary>
        /// Adds a "using namespace;" declaration into the code for the namespace
        /// that holds the type T
        /// </summary>
        /// <param name="writer"></param>
        /// <typeparam name="T"></typeparam>
        public static void UsingNamespace<T>(this ISourceWriter writer)
        {
            writer.Write($"using {typeof(T).Namespace};");
        }

        /// <summary>
        /// Adds a "using namespace;" declaration into the code for the namespace
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="namespace"></param>
        public static void UsingNamespace(this ISourceWriter writer, string @namespace)
        {
            writer.Write($"using {@namespace};");
        }

        /// <summary>
        /// Writes "using ([declaration])" into the code and starts a new code
        /// block with a leading '{' character
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="declaration"></param>
        /// <param name="inner"></param>
        public static void UsingBlock(this ISourceWriter writer, string declaration, Action<ISourceWriter> inner)
        {
            writer.Write($"BLOCK:using ({declaration})");

            inner(writer);

            writer.FinishBlock();
        }

        /// <summary>
        /// Writes either "return;" or "return Task.CompletedTask;" into the code
        /// for synchronous or asynchronous methods
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="method"></param>
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

        /// <summary>
        /// Writes a "return [variable.Usage];" code snippet 
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="method"></param>
        /// <param name="variable"></param>
        public static void WriteReturnStatement(this ISourceWriter writer, GeneratedMethod method, Variable variable)
        {
            writer.WriteLine(method.AsyncMode == AsyncMode.AsyncTask
                ? $"return {variable.Usage};"
                : returnFromResult.ToFormat(variable.Usage));
        }

        /// <summary>
        /// Writes the text into the code as a comment at the current
        /// block level
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="comment"></param>
        public static void WriteComment(this ISourceWriter writer, string comment)
        {
            writer.Write("// " + comment);
        }


        /// <summary>
        /// Starts an else block in code with the opening brace and indention for following lines
        /// </summary>
        /// <param name="writer"></param>
        public static void WriteElse(this ISourceWriter writer)
        {
            writer.Write("BLOCK:else");
        }

        /// <summary>
        /// Writes the declaration of a new class to the source writer
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="className"></param>
        /// <param name="inheritsOrImplements"></param>
        public static void StartClass(this ISourceWriter writer, string className, params Type[] inheritsOrImplements)
        {
            if (inheritsOrImplements.Length == 0)
            {
                writer.Write($"BLOCK:public class {className}");
            }
            else
            {
                writer.Write($"BLOCK:public class {className} : {inheritsOrImplements.Select(x => x.FullNameInCode()).Join(", ")}");
            }
        }

    }
}
