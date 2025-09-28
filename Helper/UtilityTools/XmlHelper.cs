using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace SFTemplateGenerator.Helper.UtilityTools
{
    public static class XmlHelper
    {
        public static T Deserialize<T>(string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(xmlPath))
            {
                return (T)serializer.Deserialize(reader);
            }
        }
        public static T Deserialize<T>(Stream reader)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);

        }

        public static void Serialize<T>(T obj, string xmlPath)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", ""); // 移除默认命名空间

            // 配置XmlWriter，禁用XML声明
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // 关键：不生成XML声明
                Indent = true, // 保持缩进格式
                Encoding = Encoding.UTF8
            };

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (XmlWriter writer = XmlWriter.Create(xmlPath, settings))
            {
                serializer.Serialize(writer, obj, namespaces);
            }
        }

    }

}
