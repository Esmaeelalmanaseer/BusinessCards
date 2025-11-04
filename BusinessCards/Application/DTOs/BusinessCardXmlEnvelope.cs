using System.Xml.Serialization;

[XmlRoot("BusinessCards")]
public class BusinessCardXmlEnvelope
{
    [XmlElement("BusinessCard")]
    public List<BusinessCardXmlItem>? Items { get; set; }
}
public class BusinessCardXmlItem
{
    [XmlElement("Name")]
    public string? Name { get; set; }

    [XmlElement("Gender")]
    public string? Gender { get; set; }

    [XmlElement("DateOfBirth")]
    public string? DateOfBirth { get; set; }

    [XmlElement("Email")]
    public string? Email { get; set; }

    [XmlElement("Phone")]
    public string? Phone { get; set; }

    [XmlElement("Address")]
    public string? Address { get; set; }

    [XmlElement("PhotoBase64")]
    public string? PhotoBase64 { get; set; }

    [XmlElement("PhotoSizeBytes")]
    public string? PhotoSizeBytes { get; set; }
}