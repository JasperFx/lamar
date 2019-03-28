using System;
using System.Linq;
using LamarCodeGeneration;
using LamarCodeGeneration.Util;

namespace LamarCompiler.Scenarios
{
    /// <summary>
    /// Helper class to quickly exercise and test out custom Frame classes
    /// </summary>
    public static class CodegenScenario
    {
        public static CodegenResult<TObject> ForBaseOf<TObject>(Action<GeneratedMethod> configuration,
            GenerationRules rules = null)
        {
            return ForBaseOf<TObject>((t, m) => configuration(m), rules);

        }
        
        /// <summary>
        /// Generate a new method for the basic base class. The base class "TObject" should
        /// only have a single declared method
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="rules"></param>
        /// <typeparam name="TObject"></typeparam>
        /// <returns></returns>
        public static CodegenResult<TObject> ForBaseOf<TObject>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            if (typeof(TObject).GetMethods().Length != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(TObject), "The supplied base type or interface can only have exactly one declared method");
            }
            
            rules = rules ?? new GenerationRules("LamarCodegenScenario");
            var assembly = new GeneratedAssembly(rules);

            if (typeof(TObject).IsGenericType)
            {
                foreach (var genericTypeArgument in typeof(TObject).GenericTypeArguments)
                {
                    rules.Assemblies.Fill(genericTypeArgument.Assembly);
                }
            }
            
            
            var generatedType = assembly.AddType("GeneratedType", typeof(TObject));

            var method = generatedType.Methods.Single();

            configuration(generatedType, method);
            
            new AssemblyGenerator().Compile(assembly);
            
            return new CodegenResult<TObject>(generatedType.CreateInstance<TObject>(), generatedType.SourceCode);
        }
        
        public static CodegenResult<IBuilds<T>> ForBuilds<T>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IBuilds<T>>(configuration, rules);
        }
        
        public static CodegenResult<IBuilds<T>> ForBuilds<T>(Action<GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IBuilds<T>>((t, m) => configuration(m), rules);
        }
        
        public static CodegenResult<IAction<T>> ForAction<T>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<T>>(configuration, rules);
        }
        
        public static CodegenResult<IAction<TResult, T1>> ForBuilds<TResult, T1>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1>>(configuration, rules);
        }
        
        public static CodegenResult<IAction<TResult, T1, T2>> ForBuilds<TResult, T1, T2>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1, T2>>(configuration, rules);
        }
        
        public static CodegenResult<IAction<TResult, T1, T2, T3>> ForBuilds<TResult, T1, T2, T3>(Action<GeneratedType, GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1, T2, T3>>(configuration, rules);
        }
        
        
        public static CodegenResult<IAction<T>> ForAction<T>(Action<GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<T>>((t, m) => configuration(m), rules);
        }
        
        public static CodegenResult<IAction<TResult, T1>> ForBuilds<TResult, T1>(Action<GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1>>((t, m) => configuration(m), rules);
        }
        
        public static CodegenResult<IAction<TResult, T1, T2>> ForBuilds<TResult, T1, T2>(Action<GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1, T2>>((t, m) => configuration(m), rules);
        }
        
        public static CodegenResult<IAction<TResult, T1, T2, T3>> ForBuilds<TResult, T1, T2, T3>(Action<GeneratedMethod> configuration, GenerationRules rules = null)
        {
            return ForBaseOf<IAction<TResult, T1, T2, T3>>((t, m) => configuration(m), rules);
        }
    }
}