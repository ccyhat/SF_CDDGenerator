using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;
namespace SFTemplateGenerator.Processor.Moduels
{
    public class SDLKeeper : ISDLKeeper
    {
        public SDL SDL { get; private set; } = null!;
        public void SetSDL(SDL sdl)
        {
            SDL = sdl;
        }

    }
}
