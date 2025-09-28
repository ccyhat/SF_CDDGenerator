using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IUpdateRatedValue
    {
        Task UpdateRatedValueAsync(SDL sdl, Items root);
    }
}
