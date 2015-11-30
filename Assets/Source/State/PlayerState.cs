using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

public class PlayerState {

	[XmlAttribute("name")]
	public string name;

	[XmlAttribute("xPos")]
	public float xPos;
	
	[XmlAttribute("yPos")]
	public float yPos;

	[XmlElement("health")]
	public float health;
	
	[XmlElement("maxHealth")]
	public float maxHealth;


	public PlayerState() {

	}


}
