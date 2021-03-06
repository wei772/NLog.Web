using System.Text;
#if !NETSTANDARD_1plus
using System.Web;
#else

#endif
using NLog.LayoutRenderers;

namespace NLog.Web.LayoutRenderers
{
    /// <summary>
    /// ASP.NET Session ID.
    /// </summary>
    [LayoutRenderer("aspnet-sessionid")]
    public class AspNetSessionIdLayoutRenderer : AspNetLayoutRendererBase
    {
        /// <summary>
        /// Renders the ASP.NET Session ID appends it to the specified <see cref="StringBuilder" />.
        /// </summary>
        /// <param name="builder">The <see cref="StringBuilder"/> to append the rendered data to.</param>
        /// <param name="logEvent">Logging event.</param>
        protected override void DoAppend(StringBuilder builder, LogEventInfo logEvent)
        {
            var context = HttpContextAccessor.HttpContext;

            if (context?.Session == null)
            {
                return;
            }
#if !NETSTANDARD_1plus
            builder.Append(context.Session.SessionID);
#else
            builder.Append(context.Session.Id);
#endif
        }
    }
}
