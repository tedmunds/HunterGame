using UnityEngine;
using System.Collections;

public class Effect_Blind : Effect {

    public Effect_Blind(Pawn p, Pawn.EffectType effectType)
        : base(p, effectType) {
		
	}
	
	public override void Update() {
        base.Update();
	}
}
