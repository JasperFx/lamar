namespace StructureMap.Docs.samples.glossary
{
    internal class glossary
    {
        public void container()
        {
#region sample_glossary-container
            var container = new Container(c => { c.AddRegistry<FooBarRegistry>(); });
#endregion
        }

        public void nested_container()
        {
            var someExistingContainer = new Container();
#region sample_glossary-nested-container
            using (var nested = someExistingContainer.GetNestedContainer())
            {
                // pull other objects from the nested container and do work with those services
                var service = nested.GetInstance<IService>();
                service.DoSomething();
            }
#endregion
        }

        public void plugintype_and_pluggedtype()
        {
#region sample_glossary-pluggintype-and-plugged-type
//For<PLUGINTYPE>().Use<PLUGGEDTYPE>()

            var container = new Container(c => { c.AddRegistry<FooRegistry>(); });

            container.GetInstance<IFoo>();

//container.GetInstance<PLUGINTYPE>()
#endregion
        }

        public void pluginfamilly()
        {
#region sample_glossary-pluginfamily
            var container = new Container(c =>
            {
                c.For<IFoo>().Use<Foo>();
                c.For<IFoo>().Add<SomeOtherFoo>();
            });
#endregion
        }
    }
}