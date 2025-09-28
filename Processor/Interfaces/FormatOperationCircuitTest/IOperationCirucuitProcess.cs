using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatOperationCircuitTest
{
    public interface IOperationCirucuitProcess
    {
        public Task OperationCirucuitProcessAsync(SDL sdl, Items root, List<string> NodeName);
    }
}
