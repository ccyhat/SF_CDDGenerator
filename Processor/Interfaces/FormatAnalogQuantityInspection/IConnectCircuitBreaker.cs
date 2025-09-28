using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces.FormatAnalogQuantityInspection
{
    public interface IConnectCircuitBreaker
    {
        Task ConnectCircuitBreakerAsync(SDL sdl, Items root);
    }
}
