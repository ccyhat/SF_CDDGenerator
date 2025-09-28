using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatExecuteDI
{
    public interface IExecuteDIProcess
    {
        public Task ExecuteDIProcessAsync(SDL sdl, Items rootItem, List<string> NodeName);
    }
}
