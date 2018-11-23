using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace LamarCompiler
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

        private AsyncMode _asyncMode = AsyncMode.None;
        private Frame _top;

        public GeneratedMethod(MethodInfo method)
        {
            ReturnType = method.ReturnType;
            Arguments = method.GetParameters().Select(x => new Argument(x)).ToArray();
            MethodName = method.Name;
        }

        public GeneratedMethod(string methodName, Type returnType, params Argument[] arguments)
        {
            ReturnType = returnType;
            Arguments = arguments;
            MethodName = methodName;
        }

        /// <summary>
        /// The return type of the method being generated
        /// </summary>
        public Type ReturnType { get; }
        
        /// <summary>
        /// The name of the method being generated
        /// </summary>
        public string MethodName { get; }
        
        
        public bool Overrides { get; set; }

        /// <summary>
        /// Is the method synchronous, returning a Task, or an async method
        /// </summary>
        public AsyncMode AsyncMode
        {
            get => _asyncMode;
            set => _asyncMode = value;
        }
        
        public InjectedField[] Fields { get; internal set; } = new InjectedField[0];
        public IList<Frame> Frames { get; } = new List<Frame>();
        public Argument[] Arguments { get; }

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

        /// <summary>
        /// Convenience method to add a method call to the GeneratedMethod Frames
        /// collection
        /// </summary>
        /// <param name="expression"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public MethodCall Call<T>(Expression<Action<T>> expression)
        {
            var @call = MethodCall.For(expression);
            Frames.Add(@call);

            return @call;
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

        public void ArrangeFrames(GeneratedType type, IServiceVariableSource services)
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

        /// <summary>
        /// Adds a ReturnFrame to the method that will return a variable of the specified type
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public ReturnFrame Return(Type returnType)
        {
            var frame = new ReturnFrame(returnType);

            Frames.Add(frame);
            return frame;
        }

        /// <summary>
        /// Adds a ReturnFrame for the specified variable
        /// </summary>
        /// <param name="returnVariable"></param>
        /// <returns></returns>
        public ReturnFrame Return(Variable returnVariable)
        {
            var frame = new ReturnFrame(returnVariable);

            Frames.Add(frame);
            return frame;
        }

        /// <summary>
        /// Adds a ConstructorFrame<T> to the method frames
        /// </summary>
        /// <param name="constructor"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public ConstructorFrame<T> CallConstructor<T>(Expression<Func<T>> constructor)
        {
            var frame = new ConstructorFrame<T>(constructor);
            Frames.Add(frame);

            return frame;
        }

        /// <summary>
        /// Add a return frame for the method's return type
        /// </summary>
        public ReturnFrame Return()
        {
            return Return(ReturnType);
        }
    }
}