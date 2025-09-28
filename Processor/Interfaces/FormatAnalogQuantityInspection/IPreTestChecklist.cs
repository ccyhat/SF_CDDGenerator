using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IPreTestChecklist
    {
        public Task PrepareAsync(SDL sdl, Items root, List<string> _nodename);
    }
}
