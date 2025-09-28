using System.Xml.Serialization;

namespace Test
{
    public static class XmlHelper
    {
        public static T Deserialize<T>(string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(xmlPath))
            {
                return (T)serializer.Deserialize(reader)!;
            }
        }

        public static void Serialize<T>(T obj, string xmlPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(xmlPath))
            {
                serializer.Serialize(writer, obj);
            }
        }
    }

}
