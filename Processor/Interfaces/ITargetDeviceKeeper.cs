using SFTemplateGenerator.Helper.Shares.SDL;

namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface ITargetDeviceKeeper
    {
        Device TargetDevice { get; }

        void SetDevice(Device device);
    }
}
