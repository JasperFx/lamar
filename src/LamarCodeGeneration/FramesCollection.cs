using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LamarCodeGeneration.Frames;
using LamarCodeGeneration.Model;

namespace LamarCodeGeneration
{
    public class FramesCollection : List<Frame>
    {
        /// <summary>
        /// Adds a ReturnFrame to the method that will return a variable of the specified type
        /// </summary>
        /// <param name="returnType"></param>
        /// <returns></returns>
        public FramesCollection Return(Type returnType)
        {
            var frame = new ReturnFrame(returnType);

            Add(frame);
            return this;
        }

        /// <summary>
        /// Adds a ReturnFrame for the specified variable
        /// </summary>
        /// <param name="returnVariable"></param>
        /// <returns></returns>
        public FramesCollection Return(Variable returnVariable)
        {
            var frame = new ReturnFrame(returnVariable);

            Add(frame);
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
            Add(new ReturnFrame(new Variable(typeof(void), "null")));
            return this;
        }

        public FramesCollection ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        public FramesCollection ThrowNotSupportedException()
        {
            throw new NotImplementedException();
        }
    }
}