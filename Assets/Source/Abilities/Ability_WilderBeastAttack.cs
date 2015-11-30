using UnityEngine;
using System.Collections.Generic;

public class Ability_WilderBeastAttack : Ability {

    private static Vector3 spawnOffset = new Vector3(0.0f, 0.2f, 0.0f);

    private float tossSpeedHorz = 8.0f;
    private float baseTossSpeedVert = 0.5f;

    public Ability_WilderBeastAttack() : base(0.3f, 1.0f) {
		tag = "WilderBeastAttack";
		name = "WilderBeast Smash";
		
		coolDownTime = 4.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
        recommendedRange = 5.0f;

        cameraShakeFactor = 0.0f;
	}
	
	public override void Activate() {
        instigator.DoMoveMeleeAttack();


        GameObject spear = instigator.AbilityWantsObject("P_Spear", instigator.transform.position + spawnOffset);

        Throwable throwable = spear.GetComponent<Throwable>();
        if(throwable != null) {
            // determine how much lift to put in the throw, based on how far the player is

            throwable.Throw(instigator, tossSpeedHorz, baseTossSpeedVert, instigator.GetFacingDirection());
        }


		base.Activate();
	}

    public override void End() {
        instigator.EndMoveMeleeAttack();
        base.End();
    }
}
