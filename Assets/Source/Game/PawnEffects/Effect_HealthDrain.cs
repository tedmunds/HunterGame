using UnityEngine;
using System.Collections;

public class Effect_HealthDrain : Effect {


    /** How much damage is done on a tick */
    private int dmgPerTick;

    /** How often is a bleed tick applied */
    private float dmgTickInterval;

    private float lastDmgTime;


    public Effect_HealthDrain(Pawn p, Pawn.EffectType effectType, int dmgPerTick, float dmgTickInterval) : base(p, effectType) {
        this.dmgPerTick = dmgPerTick;
        this.dmgTickInterval = dmgTickInterval;
	}

	public override void Update() {
        base.Update();
        if(subject == null) {
            return;
        }

        float timeSinceTick = Time.time - lastDmgTime;
        if(timeSinceTick >= dmgTickInterval) {
            lastDmgTime = Time.time;

            // Adjust the damage by any resistances
            float dmg = dmgPerTick * (1.0f - subject.GetResistanceFor(pawnEffectType));

            // Force damage on the pawn directly, not through the normal pipeline
            subject.TakeDamage(dmg, subject);
        }
	}
}
