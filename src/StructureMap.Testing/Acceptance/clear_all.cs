using Shouldly;
using System.Diagnostics;
using Xunit;

namespace StructureMap.Testing.Acceptance
{
    public class clear_all
    {
        #region sample_ImportantClientWidget
        public class ImportantClientWidget : IWidget { }

        public class ImportantClientServices : Registry
        {
            public ImportantClientServices()
            {
                For<IWidget>().ClearAll().Use<ImportantClientWidget>();
            }
        }

        #endregion

        #region sample_clear_all_in_action
        [Fact]
        public void clear_all_in_action()
        {
            var container = new Container(_ =>
            {
                _.For<IWidget>().Use<AWidget>();

                _.IncludeRegistry<ImportantClientServices>();
            });

            container.GetInstance<IWidget>()
                .ShouldBeOfType<ImportantClientWidget>();

            Debug.WriteLine(container.WhatDoIHave(pluginType: typeof(IWidget)));
        }

        #endregion
    }
}