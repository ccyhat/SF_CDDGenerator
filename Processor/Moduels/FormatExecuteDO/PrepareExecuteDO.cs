using SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO;

namespace SFTemplateGenerator.Processor.Moduels.FormatExecuteDO
{
    public class PrepareExecuteDO : IPrepareExecuteDO
    {
        public Task PrepareExecuteDOAsync(List<string> nodename)
        {
            nodename.Add("测试前准备");

            return Task.CompletedTask;
        }
    }
}
