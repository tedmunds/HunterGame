using UnityEngine;
using System.Collections;

public class WilderBeast : BotBase {

	

    protected override void SetDefaultAIProperties() {
        bIsAgressive = true;
        bGetsScared = true;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.Wild;
        detectionRange = new Vector2(10.0f, 15.0f);
        mainAbility = new Ability_WilderBeastAttack();
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 3.0f;

        dropList = new Item[1];
        dropList[0] = world.GetCraftingItemByName("MonsterMeat");
    }

	void Start() {
		InitializeActor();
	}
	
	void Update() {
		UpdateActor();
	}

	
	protected override void  InitializeActor() {
		base.InitializeActor();
	}
	
	protected override void UpdateActor() {
		base.UpdateActor();

		ApplyCachedAnimData();
	}
}
