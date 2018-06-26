using System;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Eml.Mef;
using Eml.MefDependencyResolver.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Config;

namespace TenderSearch.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static Eml.Logger.ILogger Logger { get; private set; }

        protected void Application_Start()
        {
            //use ApplicationInsights
            ConfigurationItemFactory.Default.Targets.RegisterDefinition(
                "ApplicationInsightsTarget",
                typeof(Microsoft.ApplicationInsights.NLogTarget.ApplicationInsightsTarget)
            );
            var logger = LogManager.GetCurrentClassLogger();
            logger.Info("Application starting");

            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                };

                AreaRegistration.RegisterAllAreas();
                FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
                RouteConfig.RegisterRoutes(RouteTable.Routes);
                BundleConfig.RegisterBundles(BundleTable.Bundles);

                var rootFolder = System.Web.Hosting.HostingEnvironment.MapPath(@"~\bin");
                var classFactory = Bootstrapper.Init(rootFolder, new[] { "TenderSearch*.dll" });

                Logger = classFactory.GetExport<Eml.Logger.ILogger>();
                DependencyResolver.SetResolver(new MefDependencyResolver(classFactory.Container)); // mvc controllers

                logger.Info("Application started");
            }
            catch (Exception e)
            {
                logger.Fatal(e, "A fatal exception was thrown. The application cannot start.");
                throw;
            }
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            const string msg = "An unhandled exception occurred";
            var exception = Server.GetLastError();

            if (exception == null) return;

            if (Logger == null)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Info(msg);
            }
            else Logger.Log.Error(exception, msg);
        }

        protected void Application_End()
        {
            const string msg = "Application stopping";

            if (Logger == null)
            {
                var logger = LogManager.GetCurrentClassLogger();
                logger.Info(msg);
            }
            else Logger.Log.Info(msg);
        }

        private readonly EventHandler<ErrorEventArgs> _serializationErrorHandler = (sender, args) =>
        {
            var isHandled = args.ErrorContext.Error.Message.Contains("on 'System.Data.Entity.DynamicProxies.");
            if (isHandled) args.ErrorContext.Handled = isHandled;
        };
    }
}
