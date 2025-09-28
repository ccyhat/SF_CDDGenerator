using System.Collections.ObjectModel;

namespace SFTemplateGenerator.Helper.Shares.TreeNode
{
    public class TreeNode
    {

        public string Name { get; set; }
        public ObservableCollection<TreeNode> Children { get; set; }

        public string NameShow
        {
            get
            {
                return Children.Count == 0 ? Name : string.Format(@"{0}({1})", Name, Children.Count);
            }
        }

        public string JSONData { get; set; }


        //public List<T> children { get; set; }

        public TreeNode()
        {
            Children = new ObservableCollection<TreeNode>();
        }
    }
}
