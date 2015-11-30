using UnityEngine;
using System.Collections;

public class Ability_TossBomb : Ability {

    private static Vector3 spawnOffset = new Vector3(0.0f, 0.75f, 0.0f);

    private float tossSpeedHorz = 5.0f;
    private float tossSpeedVert = 5.0f;

	public Ability_TossBomb() : base(0.3f, 0.0f) {
		tag = "TossBomb";
		name = "Lob bomb";
		
		coolDownTime = 2.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
	}
	
	public override void Pending() {
        instigator.DoMoveThrow();

        GameObject bomb = instigator.AbilityWantsObject("P_ThrowableBomb", instigator.transform.position + spawnOffset);

        Throwable throwable = bomb.GetComponent<Throwable>();
        if(throwable != null) {
            throwable.Throw(instigator, tossSpeedHorz, tossSpeedVert, instigator.GetFacingDirection());
        }

		base.Pending();
	}
	
	public override void Activate() {
		instigator.EndMoveThrow();
		base.Activate();
	}
}
