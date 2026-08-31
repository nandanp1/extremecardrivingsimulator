using System;
using System.Xml.Serialization;

[Serializable]
public class TSConnectorOtherPoints
{
	[XmlElement("a1")]
	public int lane = -1;

	[XmlElement("a2")]
	public int connector = -1;

	[XmlElement("a3")]
	public int pointIndex = -1;
}
