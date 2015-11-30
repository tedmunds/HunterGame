using UnityEngine;
using System.Collections;

public class Ability_HunterAttack : Ability {

    private const int baseDamage = 100;

    private const float maxRange = 10.0f;

    private bool bHitMultipleTargts = true;

	public Ability_HunterAttack() : base(0.0f, 0.3f) {
		tag = "HunterAttack";
		name = "Deadly Arrow";
		
		coolDownTime = 0.5f;

        cameraShakeFactor = 1.0f;

        recommendedRange = maxRange;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
	}
	
	public override void Activate() {
        instigator.DoMoveMeleeAttack();
        
        // Check what was hit in a straight line
        Vector3 dir = instigator.GetFacingDirection().normalized;

        RaycastHit2D[] hits = Physics2D.RaycastAll(instigator.transform.position, dir, maxRange);
        for(int i = 0; i < hits.Length; i++ ) {
            if(hits[i].collider != null) {

                Pawn p = IsColliderPawn(hits[i].collider);
                if(p != null && p != instigator) {
                    p.RecieveAttack(baseDamage + instigator.GetDamageBonus(), instigator, Pawn.BodyPart.Torso);

                    if(!bHitMultipleTargts) {
                        break;
                    }
                }
            }
        }
        
		base.Activate();
	}

    public override void End() {
        instigator.EndMoveMeleeAttack();
        base.End();
    }
}
