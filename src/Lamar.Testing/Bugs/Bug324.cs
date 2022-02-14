using Lamar;
using Shouldly;
using Xunit;

public class Bug_324_scoping_within_on_creation
{
    public interface IWidget
    {
    }

    public class Widget : IWidget
    {
        public Widget(){}
    }


    [Fact]
    public void oncreation_should_receive_the_constructing_container()
    {
        IServiceContext containerPassedToOnCreationLambda = null;

        var rootContainer = new Container(x =>
        {
            x.For<IWidget>().Use<Widget>()
                .OnCreation((context, concrete) => {
                    containerPassedToOnCreationLambda = context;
                    return concrete;
                })
                .Scoped();

        });

        var rootWidget = rootContainer.GetInstance<IWidget>();

        // The rootContainer should have been passed to the OnCreation lambda.
        containerPassedToOnCreationLambda.ShouldBeSameAs(rootContainer);

        var nestedContainer = rootContainer.GetNestedContainer();

        var nestedWidget = nestedContainer.GetInstance<IWidget>();

        // The nestedContainer should have been passed to the OnCreation lambda.
        containerPassedToOnCreationLambda.ShouldBeSameAs(nestedContainer);
    }
}