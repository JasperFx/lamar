using System;

namespace LamarCodeGeneration.Model
{
    /// <summary>
    /// Models a logical method and how to find candidate variables
    /// </summary>
    public interface IMethodVariables
    {
        /// <summary>
        /// Find or create a variable with the supplied type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        Variable FindVariable(Type type);
        
        /// <summary>
        /// Find or create a variable by type and variable name
        /// </summary>
        /// <param name="dependency"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        Variable FindVariableByName(Type dependency, string name);
        
        /// <summary>
        /// Try to find a variable by both dependency type and variable name
        /// </summary>
        /// <param name="dependency"></param>
        /// <param name="name"></param>
        /// <param name="variable"></param>
        /// <returns></returns>
        bool TryFindVariableByName(Type dependency, string name, out Variable variable);
        
        /// <summary>
        /// Try to find a variable by type and variable source. Use this when
        /// you need to differentiate between variables that are resolved
        /// from an IoC container and all other kinds of variables
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        Variable TryFindVariable(Type type, VariableSource source);
    }
}
