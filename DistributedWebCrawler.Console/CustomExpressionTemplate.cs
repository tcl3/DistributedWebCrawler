using Serilog.Templates;
using Serilog.Templates.Themes;

namespace DistributedWebCrawler.Console
{
    // FIXME: This is a hack to get around a bug in serilog where
    // an ExpressionTemplate and theme cannot be set at the same time
    // in appsettings.json. This will be fixed in the next version of
    // Serilog.Settings.Configuration
    public class CustomExpressionTemplate : ExpressionTemplate
    {
        public CustomExpressionTemplate(string template)
            : base(template, theme: TemplateTheme.Literate)
        {
        }
    }
}
