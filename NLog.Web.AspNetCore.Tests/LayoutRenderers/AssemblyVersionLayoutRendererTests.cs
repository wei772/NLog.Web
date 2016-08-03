using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
#if NETSTANDARD_1plus
            Layout l = "${assembly-version:NLog.Web.AspNetCore.Tests}";
#else
            Layout l = "${assembly-version:NLog.Web.AspNetCore.Tests}";
#endif
            var result = l.Render(LogEventInfo.CreateNullEvent());
            Assert.Equal("1.2.3.0", result);
        }
    }
}
