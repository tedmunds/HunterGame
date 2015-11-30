using UnityEngine;
using System.Collections.Generic;

public class Ability_Slash : Ability {

    private const float slashRange = 1.0f;

    private const int baseDamage = 200;

    public Ability_Slash() : base(0.0f, 0.5f) {
		tag = "Slash";
		name = "Slashing Blade";
		
		coolDownTime = 1.5f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;

        cameraShakeFactor = 0.5f;
	}
	
	public override void Activate() {
        instigator.DoMoveMeleeAttack();

        List<Pawn> hits = GetTargetsInFront(slashRange);
        for(int i = 0; i < hits.Count; i++ ) {
            hits[i].RecieveAttack(baseDamage + instigator.GetDamageBonus(), instigator, Pawn.BodyPart.Torso);
        }

		base.Activate();
	}

    public override void End() {
        instigator.EndMoveMeleeAttack();
        base.End();
    }
}
