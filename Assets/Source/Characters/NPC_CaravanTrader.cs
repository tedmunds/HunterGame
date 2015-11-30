using UnityEngine;
using System.Collections.Generic;

public class NPC_CaravanTrader : NPC {


    protected override void SetDefaultAIProperties() {
        bIsAgressive = false;
        bGetsScared = false;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.NPC;
        detectionRange = new Vector2(10.0f, 25.0f);
        mainAbility = new Ability_HunterAttack();
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 3.0f;
        bCanBeAttackedByPlayer = false;
        bDoesWander = false;

        dropList = new Item[0];

        defaultGreetingText = "Howdy, I am the ";
    }

    void Start() {
        InitializeActor();
    }


    void Update() {
        UpdateActor();
    }

    protected override void InitializeActor() {
        base.InitializeActor();

        interactionSet.Add(new Interaction(TradeForMonsterBait, "Trade for Monster Bait"));
    }

    protected override void UpdateActor() {
        base.UpdateActor();
    }

    // Interaction handlers ------------------------
    public void TradeForMonsterBait(UI_PlayerBase activator) {
        activator.OpenTradeMenuFor(this);
    }

}
