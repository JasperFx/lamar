using System.Collections.Generic;
using System.Linq;
using Lamar;
using StructureMap.Testing.Widget;

namespace LamarDiagnosticsWithNetCore3Demonstrator
{
    
    public class EngineChoice
    {
        private readonly IEnumerable<IEngine> _engines;

        public EngineChoice(IEnumerable<IEngine> engines)
        {
            _engines = engines.ToArray();
        }
    }
    
    public interface ISetter
    {

    }

    public class Setter : ISetter
    {

    }

    public class SetterHolder
    {
        [SetterProperty]
        public ISetter Setter { get; set; }
    }
    
    public interface IAutomobile
    {
    }

    public interface IEngine
    {
    }

    public class NamedEngine : IEngine
    {
        private readonly string _name;

        public NamedEngine(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }
    }

    public class VEight : IEngine
    {
    }

    public class StraightSix : IEngine
    {
    }

    public class Hemi : IEngine
    {
    }

    public class FourFiftyFour : IEngine
    {
    }

    public class VTwelve : IEngine
    {
    }

    public class Rotary : IEngine
    {
    }

    public class PluginElectric : IEngine
    {
    }

    public class InlineFour : IEngine
    {
        public override string ToString()
        {
            return "I'm an inline 4!";
        }
    }
    
    public interface IThing
    {
            
    }

    public class Thing : IThing
    {
            
    }
        

    public class DeepConstructorGuy
    {
        public DeepConstructorGuy()
        {

        }

        public DeepConstructorGuy(IWidget widget, IThing method)
        {

        }

        public DeepConstructorGuy(IWidget widget, bool nothing)
        {

        }

    }

    public class GuyWithWidgetAndRule
    {
        public IWidget Widget { get; }
        public Rule Rule { get; }

        public GuyWithWidgetAndRule(IWidget widget, Rule rule)
        {
            Widget = widget;
            Rule = rule;
        }
    }

    public class GuyThatUsesIWidget
    {
        public GuyThatUsesIWidget(IWidget widget)
        {
        }
    }

    public class WidgetWithRule : IWidget
    {
        public Rule Rule { get; }

        public WidgetWithRule(Rule rule)
        {
            Rule = rule;
        }

        public void DoSomething()
        {
            
        }
    }

}