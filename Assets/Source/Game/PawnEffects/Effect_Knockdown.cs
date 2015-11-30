using UnityEngine;
using System.Collections;

public class Effect_Knockdown : Effect {

    public Effect_Knockdown(Pawn p, Pawn.EffectType effectType)
        : base(p, effectType) {
		
	}

	public override void Update() {
        base.Update();
	}
}
