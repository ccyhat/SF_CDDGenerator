using SFTemplateGenerator.Helper.Config;
using SFTemplateGenerator.Helper.Shares.GuideBook;
namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IGbXmlProcessor
    {
        Task<GuideBook> ParseGbXmlAsync(string path);
        Task SaveGbXmlAsync(Tuple<string, string> id_unnormal, Local_DB local_DB, string path, GuideBook guideBook);

    }
}
