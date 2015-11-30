using UnityEngine;
using System.Collections;

public class Ability_ThrowTrap : Ability {

	public Ability_ThrowTrap() : base(0.2f, 0.3f) {
		tag = "ThrowTrap";
		name = "Throw Trap";
		
		coolDownTime = 5.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
	}

	public override void Pending() {

		base.Pending();
	}
	
	public override void Activate() {
		instigator.DoMoveThrow();
		base.Activate();
	}

	public override void End() {
		instigator.EndMoveThrow();
		base.End();
	}
}
