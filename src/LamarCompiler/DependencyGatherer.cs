using System.Collections.Generic;
using System.Linq;
using LamarCompiler.Frames;
using LamarCompiler.Model;
using LamarCompiler.Util;

namespace LamarCompiler
{
    internal class DependencyGatherer
    {
        private readonly IMethodVariables _methodVariables;

        public readonly LightweightCache<Frame, List<Frame>> Dependencies = new LightweightCache<Frame, List<Frame>>();
        public readonly LightweightCache<Variable, List<Frame>> Variables = new LightweightCache<Variable, List<Frame>>();

        public DependencyGatherer(IMethodVariables methodVariables, IList<Frame> frames)
        {
            _methodVariables = methodVariables;
            Dependencies.OnMissing = frame => new List<Frame>(findDependencies(frame).Distinct());
            Variables.OnMissing = v => new List<Frame>(findDependencies(v).Distinct());

            foreach (var frame in frames)
            {
                Dependencies.FillDefault(frame);
            }
        }



        private IEnumerable<Frame> findDependencies(Frame frame)
        {
            frame.ResolveVariables(_methodVariables);

            foreach (var dependency in frame.Dependencies)
            {
                yield return dependency;

                foreach (var child in Dependencies[dependency])
                {
                    yield return child;
                }
            }

            foreach (var variable in frame.Uses)
            {
                foreach (var dependency in Variables[variable])
                {
                    yield return dependency;
                }
            }

        }

        private IEnumerable<Frame> findDependencies(Variable variable)
        {
            if (variable.Creator != null)
            {
                yield return variable.Creator;
                foreach (var frame in Dependencies[variable.Creator])
                {
                    yield return frame;
                }
            }

            foreach (var dependency in variable.Dependencies)
            {
                foreach (var frame in Variables[dependency])
                {
                    yield return frame;
                }
            }
        }

    }
}
