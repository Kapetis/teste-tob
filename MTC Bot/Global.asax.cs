using System.Web.Http;
using Autofac;
using Autofac.Integration.WebApi;

namespace MTC_Bot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        public static ILifetimeScope FindContainer()
        {
            var config = GlobalConfiguration.Configuration;
            var resolver = (AutofacWebApiDependencyResolver)config.DependencyResolver;
            return resolver.Container;
        }

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
