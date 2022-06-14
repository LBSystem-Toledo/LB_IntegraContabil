using Topshelf;

namespace LB_IntegraContabil
{
    public class ConfigureService
    {
        internal static void Configure()
        {
            HostFactory.Run(configure =>
            {
                configure.Service<IntegraContabil>(service =>
                {
                    service.ConstructUsing(s => new IntegraContabil());
                    service.WhenStarted(s => s.Start());
                    service.WhenStopped(s => s.Stop());
                });
                configure.RunAsLocalSystem();
                configure.SetServiceName("LBSystem_IntegraContabil");
                configure.SetDisplayName("LBSystem_IntegraContabil");
                configure.SetDescription("Integração documentos fiscais com ONVIO/Sieg");
            });
        }
    }
}
