using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog.Config;
using NLog.Layouts;
using NLog.Web.Tests.LayoutRenderers;
using Xunit;

namespace NLog.Web.AspNetCore.Tests.LayoutRenderers
{
    public class AssemblyVersionLayoutRendererTests : TestBase
    {
        [Fact]
        public void AssemblyNameVersionTest()
        {
           // ConfigurationItemFactory.Default.RegisterItemsFromAssembly(typeof(AssemblyVersionLayoutRendererTests).Assembly);

#if NETSTANDARD_1plus
            Layout l = "${assembly-version:NLog.Web.AspNetCore.Tests}";
#else
            Layout l = "${assembly-version:NLog.Web.Tests}";
#endif
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("1.2.3.0", result);
        }
    }
}
