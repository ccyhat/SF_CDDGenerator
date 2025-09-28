using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.UtilityTools;
namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IDeviceModelKeeper
    {
        DeviceModel TargetDeviceModel { get; }
        void SetDeviceModel(DeviceModel TargetDeviceModel);
        DeviceModelCache deviceModelCache { get; }
    }
}
