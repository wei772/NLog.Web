﻿//TODO test .NET Core
using System;
using System.Collections.Generic;
using System.Globalization;
using NLog.Web.LayoutRenderers;
using NSubstitute;
using NLog.Web.Enums;
using Xunit;

using System.Reflection;

using NLog.Config;
using NLog.Layouts;
using NLog.Targets;

#if !NETSTANDARD_1plus
using System.Web;
using System.Collections.Specialized;
using System.Web.SessionState;
#else
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using HttpContextBase = Microsoft.AspNetCore.Http.HttpContext;
#endif



namespace NLog.Web.Tests.LayoutRenderers
{
    public class AspNetCookieLayoutRendererTests : TestInvolvingAspNetHttpContext
    {
        public AspNetCookieLayoutRendererTests() : base()
        {

        }

        [Fact]
        public void NullKeyRendersEmptyString()
        {
            var httpContext = Substitute.For<HttpContextBase>();

            var renderer = new AspNetRequestCookieLayoutRenderer();
            renderer.HttpContextAccessor = new FakeHttpContextAccessor(httpContext);
            renderer.CookieNames = null;

            string result = renderer.Render(new LogEventInfo());

            Assert.Empty(result);
        }

        [Fact]
        public void KeyNotFoundRendersEmptyString_Flat_Formatting()
        {
            var renderer = CreateRenderer();
            renderer.OutputFormat = AspNetRequestLayoutOutputFormat.Flat;
            renderer.CookieNames = new List<string> { "notfound" };
            string result = renderer.Render(new LogEventInfo());

            Assert.Empty(result);
        }



        [Fact]
        public void KeyNotFoundRendersEmptyString_Json_Formatting()
        {
            var renderer = CreateRenderer();
            renderer.OutputFormat = AspNetRequestLayoutOutputFormat.Json;
            renderer.CookieNames = new List<string> { "notfound" };

            string result = renderer.Render(new LogEventInfo());

            Assert.Empty(result);
        }

        [Fact]
        public void KeyFoundRendersValue_Cookie_Mulitple_Items_Flat_Formatting()
        {
            var expectedResult = "key=TEST&Key1=TEST1";

            var renderer = CreateRenderer();

            string result = renderer.Render(new LogEventInfo());

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void KeyFoundRendersValue_Single_Item_Flat_Formatting()
        {
            var expectedResult = "key=TEST";

            var renderer = CreateRenderer(addKey: false);

            string result = renderer.Render(new LogEventInfo());

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void KeyFoundRendersValue_Cookie_Mulitple_Items_Json_Formatting()
        {
            var expectedResult = "{\"key=TEST&Key1=TEST1\"}";

            var renderer = CreateRenderer();


            renderer.OutputFormat = AspNetRequestLayoutOutputFormat.Json;

            string result = renderer.Render(new LogEventInfo());

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void KeyFoundRendersVakue_Cookie_Mulitple_Cookies_Cookie_Items_Flat_Formatting()
        {
            var expectedResult = "key=TEST&Key1=TEST1,key2=Test&key3=Test456";
            var renderer = CreateRenderer(addCookie2: true);




            renderer.CookieNames = new List<string> { "key", "key2" };

            string result = renderer.Render(new LogEventInfo());

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void KeyFoundRendersVakue_Cookie_Mulitple_Cookies_Cookie_Items_Json_Formatting()
        {
            var expectedResult = "{\"key=TEST&Key1=TEST1\"},{\"key2=Test&key3=Test456\"}";
            var renderer = CreateRenderer(addCookie2: true);
            renderer.OutputFormat = AspNetRequestLayoutOutputFormat.Json;

            string result = renderer.Render(new LogEventInfo());

            Assert.Equal(expectedResult, result);
        }

#if !NETSTANDARD_1plus

        [Fact]
        public void CommaSeperatedCookieNamesTest_Mulitple_FLAT_Formatting()
        {
            var expectedResult = "key=TEST&Key1=TEST1";

            string config = @"<nlog>
    <extensions>
        <add assembly='NLog.Web' />
    </extensions>
<targets><target name='debug' type='Debug' layout='${aspnet-request-cookie:CookieNames=key,key1}' /></targets>
    <rules>
        <logger name='*' minlevel='Debug' writeTo='debug' />
    </rules>
</nlog>";
            LogManager.Configuration = CreateConfigurationFromString(config);

            var cookie = new HttpCookie("key", "TEST");
            cookie["Key1"] = "TEST1";

            this.HttpContext.Request.Cookies.Add(cookie);
            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var renderer = ((SimpleLayout)t.Layout).Renderers[0] as AspNetRequestCookieLayoutRenderer;

            var result = renderer.Render(LogEventInfo.CreateNullEvent());

            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void CommaSeperatedCookieNamesTest_Mulitple_Json_Formatting()
        {
            var expectedResult = "{\"key=TEST&Key1=TEST1\"}";

            string config = @"<nlog>
    <extensions>
        <add assembly='NLog.Web' />
    </extensions>
<targets><target name='debug' type='Debug' layout='${aspnet-request-cookie:CookieNames=key,key1:OutputFormat=Json}' /></targets>
</nlog>";
            LogManager.Configuration = CreateConfigurationFromString(config);

            var cookie = new HttpCookie("key", "TEST");
            cookie["Key1"] = "TEST1";

            this.HttpContext.Request.Cookies.Add(cookie);
            var t = (DebugTarget)LogManager.Configuration.AllTargets[0];
            var renderer = ((SimpleLayout)t.Layout).Renderers[0] as AspNetRequestCookieLayoutRenderer;

            var result = renderer.Render(LogEventInfo.CreateNullEvent());

            Assert.Equal(expectedResult, result);
        }

#endif

        /// <summary>
        /// Create cookie renderer with mockup http context
        /// </summary>
        /// <param name="addKey">add key1 to cookie 1</param>
        /// <param name="addCookie2">add 2nd cookie</param>
        /// <returns></returns>
        private static AspNetRequestCookieLayoutRenderer CreateRenderer(bool addKey = true, bool addCookie2 = false)
        {
            var cookieNames = new List<string> { "key" };
            var httpContext = Substitute.For<HttpContextBase>();


#if NETSTANDARD_1plus
            IRequestCookieCollection cookies = Substitute.For<IRequestCookieCollection>();
            cookies["key"].Returns("TEST");

            cookies.Count.Returns(1);
            if (addKey)
            {
                cookies["key1"].Returns("TEST1");
                cookies.Count.Returns(2);
            }

            if (addCookie2)
            {
                cookies["key2"].Returns("Test");
                cookies["key3"].Returns("Test456");
                cookieNames.Add("key2");
                cookies.Count.Returns(4);
            }
#else
            var cookie1 = new HttpCookie("key", "TEST");
            if (addKey)
            {
                cookie1["Key1"] = "TEST1";
            }
            var cookies = new HttpCookieCollection { cookie1 };
           
            if (addCookie2)
            {
                var cookie2 = new HttpCookie("key2", "Test");
                cookie2["key3"] = "Test456";
                cookies.Add(cookie2);
                cookieNames.Add("key2");
            }
#endif


            httpContext.Request.Cookies.Returns(cookies);
            var renderer = new AspNetRequestCookieLayoutRenderer();
            renderer.HttpContextAccessor = new FakeHttpContextAccessor(httpContext);

            renderer.CookieNames = cookieNames;




            return renderer;
        }
    }
}
