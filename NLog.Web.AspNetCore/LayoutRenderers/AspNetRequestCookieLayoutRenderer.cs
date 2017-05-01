using System.Text;
#if !NETSTANDARD_1plus
using System.Web;
using System.Collections.Specialized;
#else
using Microsoft.Extensions.Primitives;
#endif
using NLog.LayoutRenderers;
using System.Collections.Generic;
using NLog.Config;
using NLog.Web.Enums;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using NLog.Web.Internal;

namespace NLog.Web.LayoutRenderers
{
    /// <summary>
    /// ASP.NET Request Cookie
    /// </summary>
    /// <para>Example usage of ${aspnet-request-cookie}</para>
    /// <example>
    /// <code lang="NLog Layout Renderer">
    /// ${aspnet-request-cookie:OutputFormat=Flat}
    /// ${aspnet-request-cookie:OutputFormat=Json}
    /// </code>
    /// </example>
    [LayoutRenderer("aspnet-request-cookie")]
    public class AspNetRequestCookieLayoutRenderer : AspNetLayoutRendererBase
    {
        /// <summary>
        /// List Cookie Key as String to be rendered from Request.
        /// </summary>
        public List<string> CookieNames { get; set; }

        /// <summary>
        /// Determines how the output is rendered. Possible Value: FLAT, JSON. Default is FLAT.
        /// </summary>
        [DefaultParameter]
        public AspNetRequestLayoutOutputFormat OutputFormat { get; set; } = AspNetRequestLayoutOutputFormat.Flat;

        /// <summary>
        /// Renders the ASP.NET Cookie appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var httpRequest = HttpContextAccessor?.HttpContext?.TryGetRequest();

            if (httpRequest == null)
            {
                return;
            }

            var cookies = httpRequest.Cookies;

            if (this.CookieNames?.Count > 0 && cookies?.Count > 0)
            {
                var cookieValues = GetCookies(cookies);
                SerializeValues(cookieValues, builder);
            }
        }

        private IEnumerable<(string key, string value)> GetCookies(IRequestCookieCollection cookies)
        {
            var cookieNames = this.CookieNames;
            if (cookieNames != null)
            {
                foreach (var cookieName in cookieNames)
                {
                    if (cookies.TryGetValue(cookieName, out var cookieValue))
                    {
                        yield return (cookieName, cookieValue);
                    }
                }
            }
        }


        private void SerializeValues(IEnumerable<(string key, string value)> values, StringBuilder builder)
        {
            var firstItem = true;


            switch (this.OutputFormat)
            {
                case AspNetRequestLayoutOutputFormat.Flat:

                    foreach (var (key, value) in values)
                    {
                        if (!firstItem)
                        {
                            builder.Append(',');
                        }
                        firstItem = false;
                        builder.Append(key);
                        builder.Append('=');
                        builder.Append(value);
                    }


                    break;
                case AspNetRequestLayoutOutputFormat.Json:


                    var valueList = values.ToList();

                    if (valueList.Count > 0)
                    {
                        var addArray = valueList.Count > 1;

                        if (addArray)
                        {
                            builder.Append('[');
                        }

                        foreach (var (key, value) in valueList)
                        {
                            if (!firstItem)
                            {
                                builder.Append(',');
                            }
                            firstItem = false;

                            //quoted key
                            builder.Append('{');
                            builder.Append('"');
                            //todo escape quotes
                            builder.Append(key);
                            builder.Append('"');

                            builder.Append(':');

                            //quoted value;
                            builder.Append('"');
                            //todo escape quotes
                            builder.Append(value);
                            builder.Append('"');
                            builder.Append('}');
                        }
                        if (addArray)
                        {
                            builder.Append(']');
                        }
                    }
                    break;
            }
        }

#if !NETSTANDARD_1plus
        /// <summary>
        /// To Serialize the HttpCookie based on the configured output format.
        /// </summary>
        /// <param name="cookieName">Name of the cookie</param>
        /// <param name="cookie">The current cookie item.</param>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="firstItem">Whether it is first item.</param>
        private void SerializeCookie(string cookieName, HttpCookie cookie, StringBuilder builder, bool firstItem)
        {
            if (cookie != null)
            {
                this.SerializeCookie(cookieName, cookie.Value, builder, firstItem);
            }
        }

#endif
    }
}
