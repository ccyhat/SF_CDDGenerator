using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDI
{
    public class PrepareExecuteDI : IPrepareExecuteDI
    {
        public Task PrepareExecuteDIAsync(List<string> _nodename)
        {
            _nodename.Add("测试前准备");
            return Task.CompletedTask;
        }
    }
}
