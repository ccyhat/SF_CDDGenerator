using Castle.DynamicProxy;
using SFTemplateGenerator.Processor.Interfaces;

namespace SFTemplateGenerator.Processor
{
    public class ExceptionInterceptor : IInterceptor, IAsyncInterceptor
    {
        private readonly INotifyExceptionOccuredProcessor _processor;

        public ExceptionInterceptor(INotifyExceptionOccuredProcessor processor)
        {
            _processor = processor;
        }

        public void Intercept(IInvocation invocation)
        {
            // 同步拦截逻辑（可调用异步方法）
            InterceptSynchronous(invocation);
        }
        public void InterceptAsynchronous(IInvocation invocation)
        {
            Handle(invocation);
        }

        public void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            Handle(invocation);
        }

        public void InterceptSynchronous(IInvocation invocation)
        {
            Handle(invocation);
        }

        private void Handle(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();

                var result = invocation.ReturnValue as Task;

                if (result != null && result.Exception != null)
                {
                    foreach (var error in result.Exception.InnerExceptions)
                    {
                        AddErrorLog(invocation.InvocationTarget, error);
                        _processor.RaiseException(invocation.InvocationTarget, error.Message, exception: error);
                    }
                }
            }
            catch (Exception ex)
            {
                AddErrorLog(invocation.InvocationTarget, ex);
                _processor.RaiseException(invocation.InvocationTarget, ex.Message, exception: ex);
            }
        }

        private void AddErrorLog(object sender, Exception ex)
        {
            //string basePath = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetEntryAssembly().Location));
            //string errorFile = Path.Combine(basePath, $"{sender.GetType().Name}{DateTime.Now.ToString("yyyyMMddHHmmss")}.error");

            //if (File.Exists(errorFile))
            //{
            //    File.AppendAllLines(errorFile, new List<string>() { $"{sender.GetType().Name} Error:\n{ex}" });
            //}
            //else
            //{
            //    File.WriteAllLines(errorFile, new List<string>() { $"{sender.GetType().Name} Error:\n{ex}" });
            //}
        }
    }
}
