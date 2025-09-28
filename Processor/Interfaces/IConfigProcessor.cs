using SFTemplateGenerator.Helper.Config;
using SFTemplateGenerator.Helper.Return;
using SFTemplateGenerator.Helper.Shares.DIO;
using SFTemplateGenerator.Helper.Shares.GuideBook;
namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IConfigProcessor
    {
        //  Task<TypeConfigs> GetConfigsByXML();
        /// <summary>
        /// 读取Device Model
        /// </summary>
        /// <returns></returns>
        ReturnCodeEntity<DeviceModel> GetDeviceModel(Tuple<string, string> id_unnormal, ref Local_DB local_DB);
        ReturnCodeEntity<PortDefine> GetDIODefine(string name);
    }
}
