using UnityEngine;
using System.Collections;

public class Spider : BotBase {


    private float spiderJumpVelocity = 2.0f;
	

    protected override void SetDefaultAIProperties() {
        bIsAgressive = true;
        bGetsScared = false;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.Wild;
        detectionRange = new Vector2(7.0f, 15.0f);
        mainAbility = new Ability_SpiderAttack();
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 3.0f;

        dropList = new Item[1];
        dropList[0] = world.GetCraftingItemByName("SpiderSilk");
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

    protected override void MoveWander() {
        base.MoveWander();
        if(bIsOnGround) {
            DoJump(spiderJumpVelocity);
        }
    }

}
