using System;
using System.Web;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Ninject;
using Ninject.Web.Common;
using Ricettario;
using ServiceStack.Data;
using ServiceStack.OrmLite;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(NinjectWebCommon), "Start")]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(NinjectWebCommon), "Stop")]

namespace Ricettario
{
    public static class NinjectWebCommon 
    {
        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();
        public static IKernel Kernel;

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
            Bootstrapper.Initialize(CreateKernel);
        }
        
        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        public static T Resolve<T>()
        {
            return Resolve<T>(typeof (T));
        }

        public static T Resolve<T>(Type type)
        {
            return (T)Kernel.Get(type);
        }
        
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            Kernel = new StandardKernel();
            try
            {
                Kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
                Kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

                RegisterServices(Kernel);
                return Kernel;
            }
            catch
            {
                Kernel.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            var data = AppDomain.CurrentDomain.GetData("DataDirectory");
            kernel.Bind<IDbConnectionFactory>().ToMethod(_ => OrmLiteConnectionFactory(data + @"\db.sqlite")).InSingletonScope();
        }

        private static OrmLiteConnectionFactory OrmLiteConnectionFactory(string sqliteFileDb)
        {
            //OrmLiteConfig.DialectProvider.NamingStrategy = new OrmLiteNamingStrategyBase();
            return new OrmLiteConnectionFactory(sqliteFileDb, SqliteDialect.Provider);
        }
    }
}
