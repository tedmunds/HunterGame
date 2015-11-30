using UnityEngine;
using System.Collections.Generic;

public class Weapon : Item {

	/** Set of attacks this weapon can do */
    //public AttackType[] attackTypes;

    ///** index into attackTypes of the currently selected attack type */
    //public int selectedAttackType;

    ///** Game time that the last attack was done at */
    //public float timeOfLastAttack;

    ///** Weapons base minimum time b/w attacks, which can be modified by the attack type */
    //public float baseAttackRate;

    ///** Unmodified damage value per attack made*/
    //public float baseDamage;

    //public AttackType CurrentAttackType() {
    //    return attackTypes[selectedAttackType];
    //}

    ///** Called when the pawn owner wants to do an attack */
    //public void DidAttack() {
    //    timeOfLastAttack = Time.time;
    //}


    //// Stab attack
    //public class AT_Stab : AttackType {
    //    public AT_Stab() {
    //        name = "Stab";
    //        offensiveStat = Pawn.StatIndex.Weapons;
    //        defensiveStat = Pawn.StatIndex.Agility;
    //        bCanBeTargeted = true;
    //        attackSpeedModifier = 0.9f;
    //        damageModifier = 0.9f;
    //        maxRange = 0.5f;
    //        possibleEffects = null;
    //    }
    //}

    //// Slash Attack
    //public class AT_Slash : AttackType {        
    //    public AT_Slash() {
    //        name = "Slash";
    //        offensiveStat = Pawn.StatIndex.Weapons;
    //        defensiveStat = Pawn.StatIndex.Agility;
    //        bCanBeTargeted = true;
    //        attackSpeedModifier = 1.0f;
    //        damageModifier = 0.9f;
    //        maxRange = 0.5f;

    //        possibleEffects = new List<Pawn.EffectType>();
    //        possibleEffects.Add(Pawn.EffectType.Bleeding);
    //    }
    //}

    //// Hack Attack
    //public class AT_Hack : AttackType {
    //    public AT_Hack() {
    //        name = "Hack";
    //        offensiveStat = Pawn.StatIndex.Weapons;
    //        defensiveStat = Pawn.StatIndex.Agility;
    //        bCanBeTargeted = true;
    //        attackSpeedModifier = 1.0f;
    //        damageModifier = 1.0f;
    //        maxRange = 0.5f;

    //        possibleEffects = new List<Pawn.EffectType>();
    //        possibleEffects.Add(Pawn.EffectType.Broken);
    //    }
    //}

    //// Shove Attack
    //public class AT_Shove : AttackType {
    //    public AT_Shove() {
    //        name = "Shove";
    //        offensiveStat = Pawn.StatIndex.Weapons;
    //        defensiveStat = Pawn.StatIndex.Agility;
    //        bCanBeTargeted = false;
    //        attackSpeedModifier = 0.5f;
    //        damageModifier = 0.1f;
    //        maxRange = 0.5f;

    //        possibleEffects = new List<Pawn.EffectType>();
    //        possibleEffects.Add(Pawn.EffectType.Knockdown);
    //    }
    //}

    //// Cleave Attack
    //public class AT_Cleave : AttackType {
    //    public AT_Cleave() {
    //        name = "Cleave";
    //        offensiveStat = Pawn.StatIndex.Weapons;
    //        defensiveStat = Pawn.StatIndex.Agility;
    //        bCanBeTargeted = true;
    //        attackSpeedModifier = 1.0f;
    //        damageModifier = 1.0f;
    //        maxRange = 0.5f;

    //        possibleEffects = new List<Pawn.EffectType>();
    //        possibleEffects.Add(Pawn.EffectType.Broken);
    //        possibleEffects.Add(Pawn.EffectType.Bleeding);
    //    }
    //}


}
