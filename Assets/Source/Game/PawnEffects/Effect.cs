using UnityEngine;
using System.Collections;

public abstract class Effect {
	
	public float duration;

	/** How many times has it been applied */
	public int stacks;

	protected Pawn subject;

    protected Pawn.EffectType pawnEffectType;

	public Effect(Pawn p, Pawn.EffectType effectType) {
        subject = p;
        pawnEffectType = effectType;
	}

	/** update the effect */
    public virtual void Update() {
        duration = Mathf.Max(duration - Time.deltaTime, 0.0f);
    }
}
