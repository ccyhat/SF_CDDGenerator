using Autofac;
using System.Reflection;

namespace SFTemplateGenerator.MainWindow
{
    public class MainWindowModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {

            // 注册所有View
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .Where(t => t.Name.EndsWith("View"))
                   .AsSelf()
                   .InstancePerDependency();
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                 .Where(t => t.Name.EndsWith("ViewModel"))
                 .AsSelf()
                 .AsImplementedInterfaces()
                 .InstancePerDependency();
        }

    }
}
