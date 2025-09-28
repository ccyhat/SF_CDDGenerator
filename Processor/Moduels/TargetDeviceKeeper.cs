using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Processor.Interfaces;


namespace SFTemplateGenerator.Processor.Moduels
{
    public class TargetDeviceKeeper : ITargetDeviceKeeper
    {
        public Device TargetDevice { get; private set; } = null!;
        public void SetDevice(Device device)
        {
            TargetDevice = device;
        }
    }
}
