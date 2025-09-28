using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IIEDFilter
    {
        IEnumerable<Device> GetIEDDevice(SDL sdl);
        IEnumerable<Device> GetOperationBoxes(SDL context);
        Device GetFirstDevice(List<Device> devices);
    }
}
