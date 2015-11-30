using UnityEngine;
using System.Collections;
using System.Xml.Serialization;

/** 
 * Crafting item is basically a data container, of which item recipes are build. Their defining feature should be the name.
 */ 
public class CraftingItem : Item {

    // All possible crafting item funtions
    public enum ActivateFunction {
        [XmlEnum(Name = "None")]
        None,
        [XmlEnum(Name = "Heal")]
        Heal,
        [XmlEnum(Name = "CureBurning")]
        CureBurning,
        [XmlEnum(Name = "CureBleeding")]
        CureBleeding,
        [XmlEnum(Name = "CurePoison")]
        CurePoison,
        [XmlEnum(Name = "ResistBurning")]
        ResistBurning,
        [XmlEnum(Name = "ResistBleed")]
        ResistBleed,
        [XmlEnum(Name = "ResistPoison")]
        ResistPoison,
    }

    [XmlElement("itemName")]
    public string craftingItemName;

    [XmlElement("effect")]
    public string effectText;

    [XmlElement("effectFunction")]
    public ActivateFunction activateFunction;

    /** Times out of 100 that the item will drop from an enemy that drops it */
    [XmlElement("rarity")]
    public int rarityScore;

    [XmlElement("resistancePerActivate")]
    public float resistancePerActivate;

    [XmlElement("healPerActivate")]
    public float healPerActivate;

    public CraftingItem() {
        
    }


    // Perform the activation effect of this item, on the actor who instigated the sacrifice
    public void ActivateUse(Pawn instigator) {
        switch(activateFunction) {
            case ActivateFunction.None:
                break;
            case ActivateFunction.Heal:
                instigator.HealAmount(healPerActivate);
                break;
            case ActivateFunction.CureBurning: 
                instigator.RemoveEffect(Pawn.EffectType.Burning);
                break;
            case ActivateFunction.CureBleeding: 
                instigator.RemoveEffect(Pawn.EffectType.Bleeding);
                break;
            case ActivateFunction.CurePoison:
                instigator.RemoveEffect(Pawn.EffectType.Poison);
                break;
            case ActivateFunction.ResistBurning:
                instigator.AddResistanceTo(Pawn.EffectType.Burning, resistancePerActivate);
                break;
            case ActivateFunction.ResistBleed:
                instigator.AddResistanceTo(Pawn.EffectType.Bleeding, resistancePerActivate);
                break;
            case ActivateFunction.ResistPoison:
                instigator.AddResistanceTo(Pawn.EffectType.Poison, resistancePerActivate);
                break;
        }
        
    }
}
