using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Util;

namespace LamarCodeGeneration
{
    public class FramesCollection : List<Frame>
    {
        public GeneratedMethod ParentMethod { get; }

        public FramesCollection(GeneratedMethod parentMethod)
        {
            ParentMethod = parentMethod;
        }

        public FramesCollection()
        {
        }

        // TODO -- another version that takes in variables maybe?
        
        public ICodeFrame ReturnNewGeneratedTypeObject(GeneratedType typeBeingReturned, params string[] values)
        {
            return Code(
                $"return new {ParentMethod.ParentType.ParentAssembly.Namespace}.{typeBeingReturned.TypeName}({values.Join(", ")})");
        }

        /// <summary>
        /// Adds a ReturnFrame to the method that will return a variable of the specified type
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public FramesCollection Return(Type returnType)
        {
            Code("return {0};", new Use(returnType));
            return this;
        }

        /// <summary>
        /// Adds a ReturnFrame for the specified variable
        /// </summary>
        /// <param name="returnVariable"></param>
        /// <returns></returns>
        public FramesCollection Return(object returnValue)
        {
            Code("return {0};", returnValue);
            return this;
        }

        /// <summary>
        /// Adds a ConstructorFrame<T> to the method frames
        /// </summary>
        /// <param name="constructor"></param>
        /// <param name="configure">Optional, any additional configuration for the constructor frame</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public FramesCollection CallConstructor<T>(Expression<Func<T>> constructor, Action<ConstructorFrame<T>> configure = null)
        {
            var frame = new ConstructorFrame<T>(constructor);
            configure?.Invoke(frame);
            Add(frame);

            return this;
        }

        /// <summary>
        /// Add a frame to the end by its type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FramesCollection Append<T>() where T : Frame, new()
        {
            return Append(new T());
        }

        /// <summary>
        /// Append one or more frames to the end
        /// </summary>
        /// <param name="frames"></param>
        /// <returns></returns>
        public FramesCollection Append(params Frame[] frames)
        {
            this.AddRange(frames);
            return this;
        }

        /// <summary>
        /// Convenience method to add a method call to the GeneratedMethod Frames
        /// collection
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="configure">Optional configuration of the MethodCall</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public FramesCollection Call<T>(Expression<Action<T>> expression, Action<MethodCall> configure = null)
        {
            var @call = MethodCall.For(expression);
            configure?.Invoke(@call);
            Add(@call);

            return this;
        }

        public void Write(GeneratedMethod method, ISourceWriter writer)
        {
            foreach (var frame in this)
            {
                frame.GenerateCode(method, writer);
            }
        }

        /// <summary>
        /// Add a frame that just writes out "return null;"
        /// </summary>
        public FramesCollection ReturnNull()
        {
            Code("return null;");
            return this;
        }

        public FramesCollection ThrowNotImplementedException()
        {
            return Throw<NotImplementedException>();
        }

        public FramesCollection ThrowNotSupportedException()
        {
            return Throw<NotSupportedException>();
        }
        
        public FramesCollection Throw<T>(params object[] arguments) where T : Exception
        {
            var frame = new ThrowExceptionFrame<T>(arguments);
            Add(frame);
            return this;
        }

        /// <summary>
        /// Adds templated code to the method using the string.Format()
        /// mechanism
        /// </summary>
        /// <param name="code"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public ICodeFrame Code(string code, params object[] values)
        {
            var frame = new CodeFrame(false, code, values);
            Add(frame);

            return frame;
        }
        
        /// <summary>
        /// Adds templated code to the method using the string.Format()
        /// mechanism 
        /// </summary>
        /// <param name="code"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public ICodeFrame CodeAsync(string code, params object[] values)
        {
            var frame = new CodeFrame(true, code, values);
            Add(frame);

            return frame;
        }

        /// <summary>
        /// Write out a quick "return;"
        /// </summary>
        public void Return()
        {
            Code("return;");
        }
    }
}