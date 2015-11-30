using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

// Data representation of wearable items:
/*
 * Wearables dont preovide any functionality on their own, but they are regularily polled in the inventory 
 * by other things, like abilities, since they provide some passive effects
 */ 
public class WearableItem : Item {

    public enum Slot {
        [XmlEnum(Name = "None")]
        None,
        [XmlEnum(Name = "Weapon")]
        Weapon,
        [XmlEnum(Name = "Head")]
        Head,
        [XmlEnum(Name = "Body")]
        Body,
        [XmlEnum(Name = "Boots")]
        Boots,
    }

    [XmlElement("itemName")]
    public string wearableItemName;

    [XmlElement("verboseName")]
    public string verboseName;

    [XmlElement("imageName")]
    public string imageName;

    [XmlElement("value")]
    public int value;

    [XmlElement("slot")]
    public Slot slot;

    [XmlElement("strength")]
    public int strength;

    [XmlElement("rarity")]
    public float rarity;
}