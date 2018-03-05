using System.Threading.Tasks;

namespace Jasper.Generated
{
    // START: Lamar_Testing_IoC_Acceptance_end_to_end_resolution_WidgetWithThing_WidgetWithThing
    public class Lamar_Testing_IoC_Acceptance_end_to_end_resolution_WidgetWithThing_WidgetWithThing : Lamar.IoC.Resolvers.TransientResolver<Lamar.Testing.TargetTypes.IWidget>
    {

        public Lamar_Testing_IoC_Acceptance_end_to_end_resolution_WidgetWithThing_WidgetWithThing()
        {
        }


        public override System.Object Build(Lamar.IoC.Scope scope)
        {
            var Thing = new Lamar.Testing.IoC.Acceptance.end_to_end_resolution.Thing();
            var WidgetWithThing = new Lamar.Testing.IoC.Acceptance.end_to_end_resolution.WidgetWithThing(Thing);
            return WidgetWithThing;
        }

    }

    // END: Lamar_Testing_IoC_Acceptance_end_to_end_resolution_WidgetWithThing_WidgetWithThing
    
    
    // START: Lamar_Testing_IoC_Acceptance_end_to_end_resolution_GuyWithWidget_GuyWithWidget
    public class Lamar_Testing_IoC_Acceptance_end_to_end_resolution_GuyWithWidget_GuyWithWidget : Lamar.IoC.Resolvers.TransientResolver<Lamar.Testing.IoC.Acceptance.end_to_end_resolution.GuyWithWidget>
    {

        public Lamar_Testing_IoC_Acceptance_end_to_end_resolution_GuyWithWidget_GuyWithWidget()
        {
        }


        public override System.Object Build(Lamar.IoC.Scope scope)
        {
            var Thing = new Lamar.Testing.IoC.Acceptance.end_to_end_resolution.Thing();
            var WidgetWithThing = new Lamar.Testing.IoC.Acceptance.end_to_end_resolution.WidgetWithThing(Thing);
            var GuyWithWidget = new Lamar.Testing.IoC.Acceptance.end_to_end_resolution.GuyWithWidget(WidgetWithThing);
            return GuyWithWidget;
        }

    }

    // END: Lamar_Testing_IoC_Acceptance_end_to_end_resolution_GuyWithWidget_GuyWithWidget
    
    
}