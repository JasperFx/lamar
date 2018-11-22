using System;
using System.Collections.Generic;
using System.Linq;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace LamarCompiler.Frames
{
    public abstract class SyncFrame : Frame
    {
        protected SyncFrame() : base(false)
        {
        }
    }

    // SAMPLE: CommentFrame
    public class CommentFrame : SyncFrame
    {
        private readonly string _commentText;

        public CommentFrame(string commentText)
        {
            _commentText = commentText;
        }

        public override void GenerateCode(GeneratedMethod method, ISourceWriter writer)
        {
            writer.WriteComment(_commentText);
            
            // It's on you to call through to a possible next
            // frame to let it generate its code
            Next?.GenerateCode(method, writer);
        }
    }
    // ENDSAMPLE

    public abstract class AsyncFrame : Frame
    {
        protected AsyncFrame() : base(true)
        {
        }
    }

    public abstract class Frame
    {
        protected internal readonly IList<Variable> creates = new List<Variable>();
        protected readonly IList<Frame> dependencies = new List<Frame>();
        protected internal readonly IList<Variable> uses = new List<Variable>();
        private bool _hasResolved;
        private Frame _next;

        protected Frame(bool isAsync)
        {
            IsAsync = isAsync;
        }

        public bool IsAsync { get; }
        public bool Wraps { get; protected set; } = false;

        public Frame Next
        {
            get => _next;
            set
            {
                if (_next != null) throw new InvalidOperationException("Frame chain is being re-arranged");
                _next = value;
            }
        }

        public IEnumerable<Variable> Uses => uses;

        public virtual IEnumerable<Variable> Creates => creates;


        public Frame[] Dependencies => dependencies.ToArray();

        /// <summary>
        /// Creates a new variable that is marked as being
        /// "created" by this Frame
        /// </summary>
        /// <param name="variableType"></param>
        /// <returns></returns>
        public Variable Create(Type variableType)
        {
            return new Variable(variableType, this);
        }

        /// <summary>
        /// Creates a new variable that is marked as being
        /// "created" by this Frame
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Variable Create<T>()
        {
            return new Variable(typeof(T), this);
        }

        public Variable Create<T>(string name)
        {
            return new Variable(typeof(T), name, this);
        }

        public abstract void GenerateCode(GeneratedMethod method, ISourceWriter writer);

        public void ResolveVariables(IMethodVariables method)
        {
            // This has to be idempotent
            if (_hasResolved) return;

            // Filter out created variables because bad, bad Stackoverflow things happen
            // when you don't 
            var variables = FindVariables(method).Where(x => !Creates.Contains(x)).Distinct().ToArray();
            if (variables.Any(x => x == null))
                throw new InvalidOperationException($"Frame {this} could not resolve one of its variables");

            uses.AddRange(variables.Where(x => x != null));

            _hasResolved = true;
        }

        public virtual IEnumerable<Variable> FindVariables(IMethodVariables chain)
        {
            yield break;
        }

        public virtual bool CanReturnTask()
        {
            return false;
        }

        public IEnumerable<Frame> AllFrames()
        {
            var frame = this;
            while (frame != null)
            {
                yield return frame;
                frame = frame.Next;
            }
        }
    }
}