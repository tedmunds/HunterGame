using UnityEngine;
using System.Collections;

public class FlameGuy : BotBase {


    protected override void SetDefaultAIProperties() {
        bIsAgressive = true;
        bGetsScared = false;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.Wild;
        detectionRange = new Vector2(12.0f, 15.0f);
        mainAbility = new Ability_FlameGuyAttack();
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 2.0f;

        dropList = new Item[1];
        dropList[0] = world.GetCraftingItemByName("FireEssence");
    }

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }


    protected override void InitializeActor() {
        base.InitializeActor();
    }

    protected override void UpdateActor() {
        base.UpdateActor();

        ApplyCachedAnimData();
    }
	
}
