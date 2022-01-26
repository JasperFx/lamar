namespace LamarCodeGeneration
{
    /// <summary>
    /// Base type for attributes that may alter code generation
    /// </summary>
    public abstract class AlterCodeGenerationAttribute
    {
        public virtual void Modify(GeneratedType generatedType)
        {
            
        }

        public virtual void Modify(GeneratedAssembly generatedAssembly)
        {
            
        }
    }
}