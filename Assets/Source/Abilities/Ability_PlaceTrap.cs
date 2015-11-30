using UnityEngine;
using System.Collections;

public class Ability_PlaceTrap : Ability {

	private static Vector3 spawnOffset = new Vector3(0.0f, 0.5f, 0.0f);

	public Ability_PlaceTrap() : base(1.0f, 0.0f) {
		tag = "PlaceTrap";
		name = "Construct Trap";
		
		coolDownTime = 5.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
	}
	
	public override void Pending() {
		instigator.DoMoveCrouch();
		
		base.Pending();
	}
	
	public override void Activate() {
		GameObject trap = instigator.AbilityWantsObject("P_BasicTrap", instigator.transform.position - spawnOffset);
		instigator.EndMoveCrouch();

        TrapController trapController = trap.GetComponent<TrapController>();
        if(trapController != null) {
            trapController.SetTrap(instigator);
        }
		
		base.Activate();
	}

}
