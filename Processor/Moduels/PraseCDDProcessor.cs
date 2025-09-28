using Newtonsoft.Json;
using SFTemplateGenerator.Helper.Shares.SDL;
using SFTemplateGenerator.Helper.Shares.TreeNode;
using SFTemplateGenerator.Helper.UtilityTools;
using SFTemplateGenerator.Processor.Interfaces;
using System.Collections.ObjectModel;
using System.Reflection;
namespace SFTemplateGenerator.Processor.Moduels
{
    public class PraseCDDProcessor : IPraseCDDProcessor
    {
        private readonly List<string> _limitType = new List<string>{
                "System.String",
                "System.Boolean",
                "System.Int32",
                "System.Int64",
                "System.DateTime",
                "SyncRoot",
                "Height",
                "Width"
            };

        public Task<SDL> Prase(string cddPath)
        {
            SDL sdl = XmlHelper.Deserialize<SDL>(cddPath);
            return Task.FromResult<SDL>(sdl);
        }
        public async Task<ObservableCollection<TreeNode>> GetTreeNodeByCdd(string cddPath)
        {
            var treeNodes = new ObservableCollection<TreeNode>();
            var cdd = await Prase(cddPath);

            if (cdd != null)
            {
                var Header = new ObservableCollection<TreeNode>();
                var Cubicle = new ObservableCollection<TreeNode>();
                treeNodes.Add(new TreeNode()
                {
                    Name = "Header"
                });
                treeNodes.Add(new TreeNode()
                {
                    Name = "Cubicle"
                });

                GetAllNodes(Header, cdd.Header);

                GetAllNodes(Cubicle, cdd.Cubicle);

                foreach (var item in treeNodes)
                {
                    if (item.Name == "Header")
                    {
                        item.Children = Header;
                    }
                    if (item.Name == "Cubicle")
                    {
                        item.Children = Cubicle;
                    }
                }
            }
            return treeNodes;
        }
        private void GetAllNodes<T>(ObservableCollection<TreeNode> treeNodes, T data) where T : class
        {

            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                var FullName = property.PropertyType.FullName;
                if (!_limitType.Contains(FullName!))
                {

                    try
                    {
                        var children = property.GetValue(data);
                        if (children != null)
                        {

                            if (property.PropertyType.IsArray)
                            {
                                Array arrayValue = (Array)property.GetValue(data);
                                for (int i = 0; i < arrayValue.Length; i++)
                                {
                                    var item = arrayValue.GetValue(i);
                                    var treeNode = new TreeNode();
                                    treeNode.Name = property.Name;
                                    treeNode.JSONData = JsonConvert.SerializeObject(item);
                                    GetChildrenNodes(ref treeNode, item);
                                    treeNodes.Add(treeNode);
                                }
                            }
                            else
                            {
                                var treeNode = new TreeNode();
                                treeNode.Name = property.Name;
                                treeNode.JSONData = JsonConvert.SerializeObject(children);
                                GetChildrenNodes(ref treeNode, children);
                                treeNodes.Add(treeNode);
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
        private void GetChildrenNodes<T>(ref TreeNode treeNode, T data, int first = 0) where T : class
        {
            Type type = data.GetType();
            PropertyInfo[] properties = type.GetProperties();

            foreach (PropertyInfo property in properties)
            {

                var FullName = property.PropertyType.FullName;
                if (!_limitType.Contains(FullName!))
                {

                    try
                    {
                        var childrenNode = new TreeNode();
                        childrenNode.Name = property.Name;

                        var children = property.GetValue(data);
                        if (children != null)
                        {
                            var length = 1;
                            if (property.Name == "HItem")
                            {
                                var item = ((List<HItem>)children);
                                length = item.Count;
                                for (int i = 0; i < length; i++)
                                {
                                    GetChildrenNodes(ref childrenNode, item[i], 2);
                                    childrenNode.JSONData = JsonConvert.SerializeObject(item[i]);
                                    treeNode.Children.Add(childrenNode);
                                }

                            }
                            else if (property.PropertyType.IsArray)
                            {
                                Array arrayValue = (Array)property.GetValue(data);
                                for (int i = 0; i < arrayValue.Length; i++)
                                {
                                    GetChildrenNodes(ref childrenNode, arrayValue.GetValue(i), 2);
                                }
                                childrenNode.JSONData = JsonConvert.SerializeObject(arrayValue);
                                treeNode.Children.Add(childrenNode);
                            }
                            else
                            {
                                GetChildrenNodes(ref childrenNode, children, 2);
                                childrenNode.JSONData = JsonConvert.SerializeObject(children);
                                treeNode.Children.Add(childrenNode);
                            }


                        }
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
