using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.Processor.Interfaces;

namespace SFTemplateGenerator.Processor.Moduels
{
    internal class DeviceModelKeeper : IDeviceModelKeeper
    {
        public DeviceModel TargetDeviceModel { get; private set; }

        public void SetDeviceModel(DeviceModel deviceModel)
        {
            TargetDeviceModel = deviceModel;
            SetDeviceModelCache();
        }
        public DeviceModelCache deviceModelCache { get; private set; }
        private void SetDeviceModelCache()
        {
            deviceModelCache = new DeviceModelCache();
            foreach (var LDevice in TargetDeviceModel.LDevices)
            {
                foreach (var Dataset in LDevice.Datasets)
                {
                    foreach (var data in Dataset.Datas)
                    {
                        deviceModelCache.CreateDes(data.Position, data.Name);
                        deviceModelCache.CreateId(data.Position, data.Name, Dataset.Id);
                    }
                }
            }
        }
    }
}
