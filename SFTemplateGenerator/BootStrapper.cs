using Autofac;
using Caliburn.Micro;
using SFTemplateGenerator.Helper.Logger;
using SFTemplateGenerator.MainWindow;
using SFTemplateGenerator.MainWindow.Interfaces;
using SFTemplateGenerator.Processor;
using System.Reflection;
using System.Windows;
namespace SFTemplateGenerator
{

    /// <summary>
    /// 应用程序启动器
    /// </summary>
    public class BootStrapper : BootstrapperBase
    {
        private Autofac.IContainer _container = null!;
        public BootStrapper()
        {
            Initialize();
        }
        protected override async void OnStartup(object sender, StartupEventArgs e)
        {
            Logger.SetLogFilePath("SFTemplateGenerator.log");
#if DEBUG
            Logger.SetLogLevel(Logger.LogLevel.Debug);
#else
            Logger.SetLogLevel(Logger.LogLevel.Warning);
#endif
            Logger.ClearLog();
            await DisplayRootViewForAsync<IMainWindowViewModel>();
        }
        protected override void Configure()
        {
            var builder = new ContainerBuilder();
            // 1. 注册 Caliburn.Micro 核心服务（必须先注册）
            builder.RegisterType<WindowManager>().As<IWindowManager>().SingleInstance();
            builder.RegisterType<Caliburn.Micro.EventAggregator>().As<Caliburn.Micro.IEventAggregator>().SingleInstance();
            // 注册所有模块
            builder.RegisterModule<MainWindowModule>();
            builder.RegisterModule<ProcessorModuel>();
            ScanAndAddReferencedAssemblies();
            // 构建容器
            _container = builder.Build();

        }
        // 重写 GetInstance 方法，从 Autofac 容器中解析实例
        protected override object GetInstance(Type service, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                if (_container.TryResolve(service, out object instance))
                {
                    return instance;
                }
                throw new Exception($"Could not resolve type: {service.FullName}");
            }

            if (_container.TryResolveNamed(key, service, out object namedInstance))
            {
                return namedInstance;
            }
            throw new Exception($"Could not resolve type: {service.FullName} with key: {key}");
        }

        // 重写 GetAllInstances 方法
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            var type = typeof(IEnumerable<>).MakeGenericType(service);
            return (IEnumerable<object>)_container.Resolve(type);
        }

        // 重写 BuildUp 方法
        protected override void BuildUp(object instance)
        {
            _container.InjectProperties(instance);
        }
        private void ScanAndAddReferencedAssemblies()
        {
            var mainAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = mainAssembly.GetReferencedAssemblies();

            Console.WriteLine($"主程序集: {mainAssembly.FullName}");
            Console.WriteLine($"引用的程序集数量: {referencedAssemblies.Length}");

            foreach (var assemblyName in referencedAssemblies)
            {
                try
                {
                    var assembly = Assembly.Load(assemblyName);
                    Console.WriteLine($"加载程序集: {assembly.FullName}");
                    AssemblySource.Instance.Add(assembly);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"无法加载程序集: {assemblyName.Name}, 错误: {ex.Message}");
                }
            }
        }

    }
}