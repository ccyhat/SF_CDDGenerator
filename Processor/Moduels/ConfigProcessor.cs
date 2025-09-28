using SFTemplateGenerator.Helper.Config;
using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.Helper.Return;
using SFTemplateGenerator.Helper.Shares.DIO;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.Processor.Interfaces;
using System.IO;


namespace SFTemplateGenerator.Processor.Moduels
{
    public class ConfigProcessor : IConfigProcessor
    {


        public ReturnCodeEntity<DeviceModel> GetDeviceModel(Tuple<string, string> id_unnormal, ref Local_DB local_DB)
        {

            var path = PathSaver.Instance.Config.ConfigPath;

            if (!File.Exists(path))
            {
                return new ReturnCodeEntity<DeviceModel>() { Code = CustomeErrorCode.Error, ErrorMsg = $"未找到文件：{path}" };
            }
            local_DB = XmlHelper.Deserialize<Local_DB>(path);
            var models = local_DB.DeviceTemplates.Where(D => D.Id.Equals(id_unnormal.Item1) && D.Unnormal.Equals(id_unnormal.Item2)).ToList();
            if (models.Count == 1)
            {
                var filepath = PathSaver.Instance.Config.DeviceModelPath + "\\" + models.FirstOrDefault()!.Dvmfile;
                var deviceModel = XmlHelper.Deserialize<DeviceModel>(filepath);
                return new ReturnCodeEntity<DeviceModel>() { Code = CustomeErrorCode.Success, Entity = deviceModel };
            }
            else if (models.Count > 1)
            {
                return new ReturnCodeEntity<DeviceModel>() { Code = CustomeErrorCode.Error, ErrorMsg = $"sf-template-local-db.xml {id_unnormal.Item1}存在多个对应关系" };
            }
            else
            {
                return new ReturnCodeEntity<DeviceModel>() { Code = CustomeErrorCode.Error, ErrorMsg = $"sf-template-local-db.xml 未找到id={id_unnormal.Item1}且unormal={id_unnormal.Item2}" };
            }

        }
        public ReturnCodeEntity<PortDefine> GetDIODefine(string name)
        {
            var path = PathSaver.Instance.Config.DIODefinePath + $"\\DeviceType.xml";

            if (!File.Exists(path))
            {
                return new ReturnCodeEntity<PortDefine>() { Code = CustomeErrorCode.Error, ErrorMsg = $"未找到文件：{path}" };
            }
            var devicetype = XmlHelper.Deserialize<DeviceType>(path);
            var Models = devicetype.Models.Where(D => D.Type.Equals(name)).ToList();
            if (Models.Count == 1)
            {
                try
                {
                    var portDefine = XmlHelper.Deserialize<PortDefine>(PathSaver.Instance.Config.DIODefinePath + $"\\{Models.FirstOrDefault()!.Portfile}");
                    portDefine.GenerateTypedCollectionIds(); // 生成类型集合ID
                    return new ReturnCodeEntity<PortDefine>() { Code = CustomeErrorCode.Success, Entity = portDefine };
                }
                catch (Exception ex)
                {
                    return new ReturnCodeEntity<PortDefine>() { Code = CustomeErrorCode.Error, ErrorMsg = $"{Models.FirstOrDefault()!.Portfile}解析异常:{ex.Message}" };
                }

            }
            else if (Models.Count > 1)
            {
                return new ReturnCodeEntity<PortDefine>() { Code = CustomeErrorCode.Error, ErrorMsg = $"DeviceType.xml {name}存在多个对应关系" };
            }
            else
            {
                return new ReturnCodeEntity<PortDefine>() { Code = CustomeErrorCode.Error, ErrorMsg = $"DeviceType.xml {name}未配置" };
            }
        }

    }
}
