using UnityEngine;
using System.Collections;

/**
 * Camp fire is a special interactable object that allows the player to make sacrifices using crafting items.
 * In turn, the gods will add or subtract stats based on what sacrifices they prefer.
 * Gods are handled in the world manager, since they are global and generated newly every game.
 * 
 *  It is an NPC to gain all of the interaction functionailty
 */ 
public class CampFireController : NPC {

    [SerializeField]
    private float healRate = 1.0f;

    public const float interactionRange = 1.0f;

    private Player owner;

    private bool bIsResting = false;
    private Pawn restTarget;

    protected override void SetDefaultAIProperties() {
        bIsAgressive = false;
        bGetsScared = false;
        bFearPlayer = false;
        intelligenceType = AI_Intelligence.None;
        detectionRange = new Vector2(0.0f, 0.0f);
        mainAbility = null;
        wanderMoveMultiplier = 0.0f;
        runningMoveMultiplier = 0.0f;
        bCanBeAttackedByPlayer = false;
        bDoesWander = false;
        bIsDynamic = false;

        dropList = new Item[0];

        defaultGreetingText = "";
    }


    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }

    protected override void InitializeActor() {
        base.InitializeActor();

        bDoesConditionEffects = false;
        bCanMove = false;
        bDoesConditionEffects = false;
        bTakesDamage = false;

        interactionSet[0] = new Interaction(ForceEndConversation, "Close");
        interactionSet.Add(new Interaction(BeginInteraction, "Make Sacrifice"));
        interactionSet.Add(new Interaction(RestPlayer, "Rest"));
    }


    protected override void UpdateActor() {
        base.UpdateActor();

        if(bIsResting && restTarget != null) {
            restTarget.HealAmount(healRate * Time.deltaTime);

            if(restTarget.GetHealthPercent() >= 1.0f) {
                bIsResting = false;
                restTarget = null;
            }
        }
    }


    // Called when the fire is placed by the ability
    public void PlacedBy(Player p) {
        if(p == null) {
            Debug.Log("WARNING!   Camp fire +"+name+" Should only be placed by players!");
        }

        owner = p;
    }


    // Start the interaction with the owner
    public void BeginInteraction(UI_PlayerBase activator) {
        activator.sacrificeMenu.OpenMenu();
    }

    public override void ForceEndConversation(UI_PlayerBase activator) {
        // end the healing
        bIsResting = false;
        restTarget = null;

        base.ForceEndConversation(activator);
    }


    public void RestPlayer(UI_PlayerBase activator) {
        Player player = activator.GetOwner();
        if(player != null) {
            bIsResting = true;
            restTarget = player;
        }
    }

}
