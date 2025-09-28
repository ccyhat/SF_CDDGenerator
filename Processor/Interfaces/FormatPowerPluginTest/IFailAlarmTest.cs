using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatPowerPluginTest
{
    public interface IFailAlarmTest
    {
        public Task<bool> FailAlarmTestAsync(SDL sdl, Items root);
    }
}
