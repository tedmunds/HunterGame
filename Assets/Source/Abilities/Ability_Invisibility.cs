using UnityEngine;
using System.Collections;

public class Ability_Invisibility : Ability {


    public Ability_Invisibility() : base(0.0f, 15.0f) {
		tag = "Invisibility";
		name = "Become Etheral";
		
		coolDownTime = 25.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = true;
        bBlocksUseWhenActive = false;

        cameraShakeFactor = 0.0f;
	}
	
	public override void Activate() {
        instigator.DoMoveHidden(0.5f);
		base.Activate();
	}

    public override void End() {
        instigator.EndMoveHidden();
        base.End();
    }

    public override float MutatedVisibility() {
        return 0.0f;
    }
}
