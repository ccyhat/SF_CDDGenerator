using SFTemplateGenerator.Helper.Config;
using SFTemplateGenerator.Helper.Paths;
using SFTemplateGenerator.Helper.Shares.GuideBook;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.Processor.Interfaces;

namespace SFTemplateGenerator.Processor.Moduels
{
    public class GbXmlProcessor : IGbXmlProcessor
    {
        public Task<GuideBook> ParseGbXmlAsync(string path)
        {
            GuideBook guidBoox = XmlHelper.Deserialize<GuideBook>(path); ;
            return Task.FromResult<GuideBook>(guidBoox);
        }
        public Task SaveGbXmlAsync(Tuple<string, string> id_unnormal, Local_DB local_DB, string path, GuideBook guideBook)
        {
            var item = local_DB.DeviceTemplates.FirstOrDefault(D => D.Id.Equals($"{id_unnormal.Item1}") && D.Unnormal.Equals($"{id_unnormal.Item2}"));
            item.Template = System.IO.Path.GetFileName(path);
            local_DB.DeviceTemplates = local_DB.DeviceTemplates.OrderBy(D => D.Dvinfcode).ToList();
            XmlHelper.Serialize<Local_DB>(local_DB, PathSaver.Instance.Config.ConfigPath);
            XmlHelper.Serialize<GuideBook>(guideBook, path);
            return Task.CompletedTask;
        }
    }
}
