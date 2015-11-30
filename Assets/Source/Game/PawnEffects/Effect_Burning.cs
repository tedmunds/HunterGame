using UnityEngine;
using System.Collections;

public class Effect_Burning : Effect {

    public Effect_Burning(Pawn p, Pawn.EffectType effectType)
        : base(p, effectType) {
		
	}

	public override void Update() {
        base.Update();
	}
}
