using SFTemplateGenerator.Helper.Shares.SDL;


namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface ISDLKeeper
    {

        SDL SDL { get; }

        void SetSDL(SDL sdl);
    }
}
