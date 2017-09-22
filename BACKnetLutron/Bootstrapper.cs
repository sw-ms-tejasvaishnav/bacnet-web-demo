using System.Web.Http;
using Microsoft.Practices.Unity;
using Unity.WebApi;
using BACKnetLutron.Services;
using BACKnetLutron.Repository;

namespace BACKnetLutron
{
    public static class Bootstrapper
    {
        public static void Initialise()
        {
            var container = BuildUnityContainer();

            GlobalConfiguration.Configuration.DependencyResolver = new UnityDependencyResolver(container);
            //DependencyResolver.SetResolver(new Unity.WebApi.UnityDependencyResolver(container));
        }


        private static IUnityContainer BuildUnityContainer()
        {
            var container = new UnityContainer();

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // e.g. container.RegisterType<ITestService, TestService>();   

            container.RegisterType<ILutronLightFloorServices, LutronLightServices>();
            container.RegisterType<ILutronLightFloorServices, LutronLightServices>("LutronLight");

            container.RegisterType<ILutronLightRepository, LutronLightRepository>();
            container.RegisterType<ILutronLightRepository, LutronLightRepository>("LutronLightR");

            return container;
        }
    }
}