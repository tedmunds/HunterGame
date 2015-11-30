using UnityEngine;
using System.Collections.Generic;

public class Ability_SpiderAttack : Ability {

    private const float slashRange = 1.0f;

    private const int baseDamage = 10;
    private const float basePoisonDuration = 1.0f; 

    public Ability_SpiderAttack() : base(0.3f, 1.0f) {
		tag = "SpiderAttack";
		name = "Venom Sting";
		
		coolDownTime = 1.0f;
		
		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;

        cameraShakeFactor = 0.0f;
	}
	
	public override void Activate() {
        instigator.DoMoveMeleeAttack();

        List<Pawn> hits = GetTargetsInFront(slashRange);
        for(int i = 0; i < hits.Count; i++ ) {
            // spiders don't damage spiders b/c they come in swarms and they kill each other a lot
            if(hits[i].GetType() != typeof(Spider)) {
                hits[i].RecieveAttack(baseDamage, instigator, Pawn.BodyPart.Torso);
                hits[i].AddPoison(basePoisonDuration);
            }
        }

		base.Activate();
	}

    public override void End() {
        instigator.EndMoveMeleeAttack();
        base.End();
    }
}
