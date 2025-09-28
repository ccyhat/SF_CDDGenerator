using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

public abstract class CDataXmlBase : IXmlSerializable
{
    [XmlText(typeof(string))]
    public string CData { get; set; } = string.Empty;

    public virtual XmlSchema GetSchema() => null!;

    public virtual void ReadXml(XmlReader reader)
    {
        var type = GetType();
        var propMap = type.GetProperties()
            .Select(p => new
            {
                Prop = p,
                XmlName = p.GetCustomAttributes(typeof(XmlAttributeAttribute), true)
                            .Cast<XmlAttributeAttribute>()
                            .FirstOrDefault()?.AttributeName ?? p.Name
            })
            .ToDictionary(x => x.XmlName.ToLower(), x => x.Prop);

        if (reader.HasAttributes)
        {
            while (reader.MoveToNextAttribute())
            {
                var xmlName = reader.Name.ToLower();
                if (propMap.TryGetValue(xmlName, out var prop) && prop.CanWrite)
                    prop.SetValue(this, reader.Value);
            }
        }
        reader.MoveToElement();

        if (reader.IsEmptyElement)
        {
            reader.Read();
            return;
        }

        reader.Read(); // 进入内容
        if (reader.NodeType == XmlNodeType.CDATA || reader.NodeType == XmlNodeType.Text)
        {
            CData = reader.Value;
            reader.Read();
        }
        else
        {
            CData = string.Empty;
        }
        if (reader.NodeType == XmlNodeType.EndElement)
            reader.Read();
    }

    public virtual void WriteXml(XmlWriter writer)
    {
        var type = GetType();
        var props = type.GetProperties();
        foreach (var prop in props)
        {
            if (prop.Name == nameof(CData) || !prop.CanRead) continue;
            var attr = prop.GetCustomAttributes(typeof(XmlAttributeAttribute), true)
                           .Cast<XmlAttributeAttribute>()
                           .FirstOrDefault();
            var xmlName = attr?.AttributeName ?? prop.Name;
            var value = prop.GetValue(this);
            if (value != null)
                writer.WriteAttributeString(xmlName, value.ToString());
        }
        writer.WriteCData(CData ?? string.Empty);
    }
}