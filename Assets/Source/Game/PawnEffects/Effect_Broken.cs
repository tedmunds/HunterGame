using UnityEngine;
using System.Collections;

public class Effect_Broken : Effect {

	// Indexes correspond to the pawn body parts enum
	public bool[] brokenLimbs;

    public Effect_Broken(Pawn p, Pawn.EffectType effectType)
        : base(p, effectType) {
		brokenLimbs = new bool[Pawn.NUM_BODYPARTS];
	}

	public override void Update() {
        base.Update();
	}

	public int GetNumBroken() {
		int num = 0;
		for(int i = 0; i < brokenLimbs.Length; i++) {
			num += brokenLimbs[i]? 1 : 0;
		}

		return num;
	}
}
