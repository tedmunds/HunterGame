using UnityEngine;
using System.Collections;

public class Ability_FlameGuyAttack : Ability {

    private static Vector3 spawnOffset = new Vector3(0.0f, 0.1f, 0.0f);

    private float tossSpeedHorz = 6.0f;
    private float baseTossSpeedVert = 0.0f;

    public Ability_FlameGuyAttack() : base(0.3f, 0.5f) {
		tag = "FlameGuyAttack";
		name = "Fireball";
		
		coolDownTime = 4.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
        recommendedRange = 6.0f;

        cameraShakeFactor = 0.0f;
	}


    public override void Pending() {
        instigator.DoMoveAttack();

		base.Pending();
	}
	


    public override void Activate() {
        GameObject spear = instigator.AbilityWantsObject("P_FireBall", instigator.transform.position + spawnOffset);

        Throwable throwable = spear.GetComponent<Throwable>();
        if(throwable != null) {
            // determine how much lift to put in the throw, based on how far the player is

            throwable.Throw(instigator, tossSpeedHorz, baseTossSpeedVert, instigator.GetFacingDirection());
        }


        

		base.Activate();
	}

    public override void End() {
        instigator.EndMoveAttack();
        base.End();
    }
}
