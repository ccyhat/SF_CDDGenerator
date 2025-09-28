using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.Shares.TreeNode;
using System.Collections.ObjectModel;
namespace SFTemplateGenerator.Processor.Interfaces
{
    public interface IPraseCDDProcessor
    {
        Task<SDL> Prase(string cddPath);

        Task<ObservableCollection<TreeNode>> GetTreeNodeByCdd(string cddPath);
    }
}
