
using SFTemplateGenerator.Helper.Shares.SDL;
namespace SFTemplateGenerator.Helper.UtilityTools
{
    public class GenerateTestTemplateMessage
    {
        public GenerateTestTemplateMessage(params Device[] devices)
        {
            Devices = devices;
        }
        public IEnumerable<Device> Devices { get; private set; }
        public bool IsLoading { get; set; } = true;
    }
}
