using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Lamar.Codegen.Frames;
using Lamar.Codegen.Variables;
using Lamar.Compilation;
using Lamar.Util;

namespace Lamar.Codegen
{
    public class GeneratedMethod
    {
        public static GeneratedMethod For<TReturn>(string name, params Argument[] arguments)
        {
            return new GeneratedMethod(name, typeof(TReturn), arguments);
        }

        public static GeneratedMethod ForNoArg(string name)
        {
            return new GeneratedMethod(name, typeof(void), new Argument[0]);
        }

        public static GeneratedMethod ForNoArg<TReturn>(string name)
        {
            return new GeneratedMethod(name, typeof(TReturn), new Argument[0]);
        }
        
        private readonly Argument[] _arguments;
        private AsyncMode _asyncMode = AsyncMode.None;
        private Frame _top;

        public GeneratedMethod(MethodInfo method)
        {
            ReturnType = method.ReturnType;
            _arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
            MethodName = method.Name;
        }

        public GeneratedMethod(string methodName, Type returnType, params Argument[] arguments)
        {
            ReturnType = returnType;
            _arguments = arguments;
            MethodName = methodName;
        }

        public Type ReturnType { get; }
        public string MethodName { get; }
        public bool Overrides { get; set; }

        public AsyncMode AsyncMode
        {
            get => _asyncMode;
            set => _asyncMode = value;
        }
        
        public InjectedField[] Fields { get; internal set; } = new InjectedField[0];
        public IList<Frame> Frames { get; } = new List<Frame>();
        public IEnumerable<Argument> Arguments => _arguments;
        
        public IEnumerable<Setter> Setters { get; internal set; }


        public GeneratedMethod Add<T>() where T : Frame, new()
        {
            return Add(new T());
        }

        public GeneratedMethod Add(params Frame[] frames)
        {
            Frames.AddRange(frames);
            return this;
        }
        
        
        // TODO -- need a test here. It's used within Jasper, but still
        public IList<Variable> DerivedVariables { get; } = new List<Variable>();
        
        
        public IList<IVariableSource> Sources { get; } = new List<IVariableSource>();

        public Variable ReturnVariable { get; set; }
        

        public void WriteMethod(ISourceWriter writer)
        {
            if (_top == null) throw new InvalidOperationException($"You must call {nameof(ArrangeFrames)}() before writing out the source code");
            
            var returnValue = determineReturnExpression();

            if (Overrides)
            {
                returnValue = "override " + returnValue;
            }

            var arguments = Arguments.Select(x => x.Declaration).Join(", ");

            writer.Write($"BLOCK:public {returnValue} {MethodName}({arguments})");


            _top.GenerateCode(this, writer);

            writeReturnStatement(writer);

            writer.FinishBlock();
        }

        protected void writeReturnStatement(ISourceWriter writer)
        {
            if (ReturnVariable != null)
            {
                writer.Write($"return {ReturnVariable.Usage};");
            }
            else if ((AsyncMode == AsyncMode.ReturnCompletedTask || AsyncMode == AsyncMode.None) && ReturnType == typeof(Task))
            {
                writer.Write("return Task.CompletedTask;");
            }
        }


        protected string determineReturnExpression()
        {
            return AsyncMode == AsyncMode.AsyncTask
                ? "async " + ReturnType.FullNameInCode()
                : ReturnType.FullNameInCode();
        }

        public void ArrangeFrames(GeneratedType type, ServiceGraph services)
        {
            if (!Frames.Any())
            {
                throw new ArgumentOutOfRangeException(nameof(Frames), "Cannot be an empty list");
            }
            
            var compiler = new MethodFrameArranger(this, type, services);
            compiler.Arrange(out _asyncMode, out _top);
        }

        public string ToExitStatement()
        {
            return AsyncMode == AsyncMode.AsyncTask 
                ? "return;" 
                : $"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};";
        }
    }
}