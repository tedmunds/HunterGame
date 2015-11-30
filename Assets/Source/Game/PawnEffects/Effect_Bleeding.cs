using UnityEngine;
using System.Collections;

public class Effect_Bleeding : Effect {

    /** How much damage is done on a tick */
    private int bleedDmgPerTick = 10;

    /** How often is a bleed tick applied */
    private float dmgTickInterval = 0.5f;

    private float lastTickTime;

    public Effect_Bleeding(Pawn p, Pawn.EffectType effectType)
        : base(p, effectType) {
        lastTickTime = 0.0f;
	}

	public override void Update() {
        base.Update();

        if(subject == null) {
            return;
        }

        float timeSinceTick = Time.time - lastTickTime;
        if(timeSinceTick >= dmgTickInterval) {
            lastTickTime = Time.time;

            // Force damage on the pawn directly, not through the normal pipeline
            subject.TakeDamage(bleedDmgPerTick, subject);
        }
	}
}
