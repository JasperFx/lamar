using System;
using LamarCodeGeneration.Model;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration.Frames
{
    /// <summary>
    /// Stand in for a Variable top be resolved later
    /// </summary>
    public class Use
    {
        internal Variable FindVariable(IMethodVariables variables)
        {
            return _variableName.IsNotEmpty() 
                ? variables.FindVariableByName(_variableType, _variableName) 
                : variables.FindVariable(_variableType);
        }

        private readonly Type _variableType;
        private readonly string _variableName;

        public Use(Type variableType)
        {
            _variableType = variableType;
        }

        public Use(Type variableType, string variableName)
        {
            _variableType = variableType;
            _variableName = variableName;
        }

        public static Use Type<T>(string variableName = null)
        {
            return new Use(typeof(T), variableName);
        }
    }
}