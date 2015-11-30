using UnityEngine;
using System.Collections;

public class NPC_Priest : NPC {


    protected override void SetDefaultAIProperties() {
        bIsAgressive = false;
        bGetsScared = false;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.NPC;
        detectionRange = new Vector2(10.0f, 25.0f);
        mainAbility = new Ability_Slash();
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 1.0f;
        bCanBeAttackedByPlayer = false;
        bDoesWander = false;

        dropList = new Item[0];

        defaultGreetingText = "Greetings, I am the ";
    }

    void Start() {
        InitializeActor();
    }


    void Update() {
        UpdateActor();
    }

    protected override void InitializeActor() {
        base.InitializeActor();

        interactionSet.Add(new Interaction(LearnAboutGods, "Learn about the gods"));
    }

    protected override void UpdateActor() {
        base.UpdateActor();
    }

    // Interaction handlers ------------------------
    public void LearnAboutGods(UI_PlayerBase activator) {
        // present the names of the gods
    }

}
