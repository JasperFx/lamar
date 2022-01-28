using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    
    
    public interface IGeneratedType
    {
        IList<Setter> Setters { get; }
        IList<InjectedField> AllInjectedFields { get; }
        GenerationRules Rules { get; }
    }

    [DebuggerDisplay("GeneratedType: {BaseType}")]
    public class GeneratedType : IVariableSource, IGeneratedType
    {
        private readonly IList<Type> _interfaces = new List<Type>();
        private readonly IList<GeneratedMethod> _methods = new List<GeneratedMethod>();

        public GeneratedType(string typeName) : this(new GenerationRules("Lamar.Generated"), typeName)
        {
        }

        public GeneratedType(GenerationRules rules, string typeName)
        {
            Rules = rules;
            TypeName = typeName;
            Namespace = rules.ApplicationNamespace;
        }

        public GeneratedType(GeneratedAssembly parent, string typeName)
        {
            Rules = parent.Rules;
            TypeName = typeName;
            Namespace = parent.Namespace;
        }
        
        /// <summary>
        /// <summary>
        /// Optional code fragment to write at the beginning of this
        /// type in code
        /// </summary>
        public ICodeFragment Header { get; set; }
        
        /// <summary>
        /// Optional code fragment to write at the end of this type in code
        /// </summary>
        public ICodeFragment Footer { get; set; }

        /// <summary>
        /// Add a single line comment as the header to this type
        /// </summary>
        /// <param name="text"></param>
        public void CommentType(string text)
        {
            Header = new OneLineComment(text);
        }

        public GenerationRules Rules { get; }
        
        public IList<Setter> Setters { get; } = new List<Setter>();

        public string TypeName { get; }
        
        public string Namespace { get; internal set; }

        public Type BaseType { get; private set; }

        public Variable[] BaseConstructorArguments { get; private set; } = new InjectedField[0];
        
        public IList<InjectedField> AllInjectedFields { get; } = new List<InjectedField>();

        public IEnumerable<Type> Interfaces => _interfaces;


        public IEnumerable<GeneratedMethod> Methods => _methods;

        public string SourceCode { get; set; }

        public Type CompiledType { get; private set; }

        bool IVariableSource.Matches(Type type)
        {
            return AllInjectedFields.Any(x => x.VariableType == type);
        }

        Variable IVariableSource.Create(Type type)
        {
            return AllInjectedFields.FirstOrDefault(x => x.ArgType == type);
        }

        public GeneratedType InheritsFrom<T>()
        {
            return InheritsFrom(typeof(T));
        }
        
        // TODO -- need ut's
        public GeneratedType InheritsFrom(Type baseType)
        {
            var ctors = baseType.GetConstructors();
            if (ctors.Length > 1)
                throw new ArgumentOutOfRangeException(nameof(baseType),
                    $"The base type for the code generation must only have one public constructor. {baseType.FullNameInCode()} has {ctors.Length}");

            if (ctors.Length == 1)
            {
                var baseArguments = ctors.Single().GetParameters()
                    .Select(x => new InjectedField(x.ParameterType, x.Name)).ToArray();

                BaseConstructorArguments = baseArguments.Select(x => x.ToBaseCtorVariable()).ToArray();
                
                AllInjectedFields.AddRange(baseArguments);
            }


            BaseType = baseType;
            foreach (var methodInfo in baseType.GetMethods().Where(x => x.DeclaringType != typeof(object))
                .Where(x => x.CanBeOverridden())) _methods.Add(new GeneratedMethod(methodInfo) {Overrides = true, ParentType = this});


            return this;
        }


        // TODO -- need ut's
        public GeneratedType Implements(Type type)
        {
            if (!type.GetTypeInfo().IsInterface)
                throw new ArgumentOutOfRangeException(nameof(type), "Must be an interface type");

            _interfaces.Add(type);

            foreach (var methodInfo in type.GetMethods().Where(x => x.DeclaringType != typeof(object)))
                _methods.Add(new GeneratedMethod(methodInfo){ParentType = this});

            return this;
        }

        // TODO -- need ut's
        public GeneratedType Implements<T>()
        {
            return Implements(typeof(T));
        }

        // TODO -- need ut's
        public void AddMethod(GeneratedMethod method)
        {
            _methods.Add(method);
        }

        public GeneratedMethod MethodFor(string methodName)
        {
            return _methods.FirstOrDefault(x => x.MethodName == methodName);
        }

        // TODO -- UT's
        public GeneratedMethod AddVoidMethod(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(name, typeof(void), args){ParentType = this};
            AddMethod(method);

            return method;
        }

        // TODO -- UT's
        public GeneratedMethod AddMethodThatReturns<TReturn>(string name, params Argument[] args)
        {
            var method = new GeneratedMethod(name, typeof(TReturn), args){ParentType = this};
            AddMethod(method);

            return method;
        }


        public void Write(ISourceWriter writer)
        {
            Header?.Write(writer);
            writeDeclaration(writer);

            if (AllInjectedFields.Any())
            {
                writeFieldDeclarations(writer, AllInjectedFields);
                writeConstructorMethod(writer, AllInjectedFields);
            }

            writeSetters(writer);


            foreach (var method in _methods.Where(x => x.WillGenerate()))
            {
                writer.BlankLine();
                method.WriteMethod(writer);
            }

            writer.FinishBlock();
            Footer?.Write(writer);
        }

        private void writeSetters(ISourceWriter writer)
        {
            foreach (var setter in Setters)
            {
                writer.BlankLine();
                setter.WriteDeclaration(writer);
            }

            writer.BlankLine();
        }



        private void writeConstructorMethod(ISourceWriter writer, IList<InjectedField> args)
        {
            var ctorArgs = args.Select(x => x.CtorArgDeclaration).Join(", ");
            var declaration = $"BLOCK:public {TypeName}({ctorArgs})";
            if (BaseConstructorArguments.Any())
                declaration = $"{declaration} : base({BaseConstructorArguments.Select(x => x.Usage).Join(", ")})";

            writer.Write(declaration);

            foreach (var field in args) field.WriteAssignment(writer);

            writer.FinishBlock();
        }

        private void writeFieldDeclarations(ISourceWriter writer, IList<InjectedField> args)
        {
            foreach (var field in args) field.WriteDeclaration(writer);

            writer.BlankLine();
        }

        private void writeDeclaration(ISourceWriter writer)
        {
            var implemented = implements().ToArray();

            if (implemented.Any())
                writer.Write(
                    $"BLOCK:public class {TypeName} : {implemented.Select(x => x.FullNameInCode()).Join(", ")}");
            else
                writer.Write($"BLOCK:public class {TypeName}");
        }

        private IEnumerable<Type> implements()
        {
            if (BaseType != null) yield return BaseType;

            foreach (var @interface in Interfaces) yield return @interface;
        }

        public string FullName => $"{Namespace}.{TypeName}";

        public Type FindType(IEnumerable<Type> types)
        {
            CompiledType = types.SingleOrDefault(x => x.FullName == FullName);
            return CompiledType;
        }

        public void ArrangeFrames(IServiceVariableSource services = null)
        {
            foreach (var method in _methods.Where(x => x.WillGenerate())) method.ArrangeFrames(this, services);
        }

        public IEnumerable<Assembly> AssemblyReferences()
        {
            if (BaseType != null) yield return BaseType.Assembly;

            foreach (var @interface in _interfaces) yield return @interface.Assembly;
        }

        public T CreateInstance<T>(params object[] arguments)
        {
            if (CompiledType == null)
                throw new InvalidOperationException("This generated assembly has not yet been successfully compiled");

            return (T) Activator.CreateInstance(CompiledType, arguments);
        }

        public void ApplySetterValues(object builtObject)
        {
            if (builtObject.GetType() != CompiledType)
                throw new ArgumentOutOfRangeException(nameof(builtObject),
                    "This can only be applied to objects of the generated type");

            foreach (var setter in Setters)
            {
                setter.SetInitialValue(builtObject);
            }
        }

        public void UseConstantForBaseCtor(Variable variable)
        {
            var index = BaseConstructorArguments.GetFirstIndex(v => v.VariableType == variable.VariableType);
            if (index < 0)
            {
                throw new InvalidOperationException("No base constructor arguments of type " + variable.VariableType.FullNameInCode());
            }

            BaseConstructorArguments[index] = variable;
            AllInjectedFields.RemoveAt(index);
        }
        
        

    }
}