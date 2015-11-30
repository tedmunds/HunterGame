using UnityEngine;
using System.Collections.Generic;

public class AttackType  {

	/** Name of the attack type, like Stab or Slash */
	public string name;

    /** Verbose destription of the attack for UI */
    public string description;

	/** Determines if the attack will target a specific body part or if its random */
	public bool bCanBeTargeted;

	/** Multiplier for base weapon attack speed */
	public float attackSpeedModifier;

    /** Multiplier for weapons base damage */
    public float damageModifier;

	/** List of possible effects that this attack can cause */
	public List<Pawn.EffectType> possibleEffects;

    /** Maximum range that this attack will activate in */
    public float maxRange;
}
