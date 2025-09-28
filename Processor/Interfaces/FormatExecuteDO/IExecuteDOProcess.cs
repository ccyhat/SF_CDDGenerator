using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatExecuteDO
{
    public interface IExecuteDOProcess
    {
        public Task ExecuteDOProcessAsync(SDL sdl, Items root, List<string> NodeName);
    }
}
