using UnityEngine;
using System.Collections.Generic;


public class NPC : BotBase {

    public delegate void InteractionHandler(UI_PlayerBase activator);

    /* Trading functionality: what is this npc will to take. strings are the names of the items, also holds the number required */
    [System.Serializable]
    public struct TradeOption {
        public string itemName;
        public int numDesired;
        public string returnItem; // what does the npc give in return
    }

    
    [SerializeField]
    private List<TradeOption> tradeOptions;

    /**
     * Interaction is  an option in an exchange b/w player and NPC. The handler is called if the player selects this interaction
     */
    public struct Interaction {
        public Interaction(InteractionHandler h, string t) {
            handler = h;
            text = t;
        }
        public InteractionHandler handler;
        public string text;
    }

    /** How close does the player have to be to interact with the npc */
    protected const float INTERACTION_RANGE = 1.0f;

    /** NPC state */
    protected bool bDoesWander = false;
    protected bool bHasBecomeAggresive;
    protected bool bInConversation;

    /** Who was the last pawn to attack this npc */
    protected Pawn aggravatorPawn;

    protected bool bCanBeAttackedByPlayer;

    protected Vector3 baseLocation;

    protected List<Interaction> interactionSet;
    protected string defaultGreetingText;

    public bool IsInConversation() { return bInConversation; }
    public TradeOption[] GetTradeOptions() { return tradeOptions.ToArray(); }

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

        defaultGreetingText = "Hello, I am ";
    }

    void Start() {
        InitializeActor();
    }


    void Update() {
        UpdateActor();
    }


    public bool GetTradeOptionForItem(string itemName, out TradeOption  selected) {
        foreach(TradeOption option in tradeOptions) {
            if(option.itemName == itemName) {
                selected = option;
                return true;
            }
        }

        selected = new TradeOption();
        return false;
    }

    public void SetAsPOI(Vector3 startLoc) {
        baseLocation = startLoc;
    }

    protected override void InitializeActor() {
        base.InitializeActor();

        // All NPC have at least the goodbye interaction that will end the conversation
        interactionSet = new List<Interaction>();
        interactionSet.Add(new Interaction(ForceEndConversation, "Goodbye"));
    }

    protected override void UpdateActor() {
        base.UpdateActor();

        // Only tell player we're nearby if we're awake. ie, on screen
        if(playerRef != null && bIsAwake) {
            float distToPlayer = (transform.position - playerRef.transform.position).magnitude;
            if(distToPlayer <= INTERACTION_RANGE && !bHasBecomeAggresive) {
                playerRef.NPCNearby(this);
            }
            else {
                playerRef.NPCOutOfRange(this);
            }
        }

        if(aggravatorPawn != null && aggravatorPawn.IsDead()) {
            aggravatorPawn = null;
        }
    }


    public override void RecieveAttack(float damageAmount, Pawn instigator, BodyPart targetLocation) {
        // Some NPC cannot be attacked by player, so ignore them completely
        if(!bCanBeAttackedByPlayer && instigator == playerRef) {
            return;
        }

        base.RecieveAttack(damageAmount, instigator, targetLocation);
    }


    public override void TakeDamage(float damageAmount, Pawn instigator, bool bBroadcastEvent = true) {
        // Some damage bypasses receive attack so this check ensures player doesn't do damage they arn't supposed to do
        if(!bCanBeAttackedByPlayer && instigator == playerRef) {
            return;
        }

        base.TakeDamage(damageAmount, instigator, bBroadcastEvent);

        if(aggravatorPawn == null || aggravatorPawn.IsDead() || !aggravatorPawn.gameObject.activeSelf) {
            aggravatorPawn = instigator;
        }
    }


    public override bool HealthIsRelevant() {
        return bHasBecomeAggresive;
    }

    protected override Pawn DecideTarget() {
        return (aggravatorPawn != null)? aggravatorPawn : null;
    }


    public void BeginExchangeWithPlayer() {
        bInConversation = true;

    }


    public void EndExchangeWithPlayer() {
        bInConversation = false;

    }

    // Move back to starting point in POI
    protected void MoveReturnToStart() {
        Vector3 toBase = baseLocation - transform.position;

        movementComp.SetMovementModifier(wanderMoveMultiplier);
        BotWalk(toBase.normalized);
    }


    // Gets a list of compatable interactions for the player: default is to return the entire set
    public virtual List<Interaction> GetInteractionList(Player p) {
        return interactionSet;
    }

    // Returns what this NPC wants to say in conversation right now
    public virtual string GetCurrentStatement() {
        // The default is just a greeting
        return defaultGreetingText + gameName;
    }

    // Interaction Handlers -------------------------------

    public virtual void ForceEndConversation(UI_PlayerBase activator) {
        playerRef.EndCurrentExchange();
    }



    // State function overrides ---------------------------

    // Most NPC do not wander around, but stand still unless there explicatly attacked
    protected override void State_Idle() {
        if(bDoesWander) {
            MoveWander();
        }
        else {
            BotWalk(Vector3.zero);
        }

        // transitions: Idle state never pops itself, so that any other state can always easily return to idle by poping
        // Stack the next state on top of returnToBase so that after the next state ends, npc will return to base
        if(bRecievedDamageThisFrame) {
            if(bGetsScared) {
                ai_stateMachine.PushState(State_ReturnToBase);
                ai_stateMachine.PushState(State_Flee);
            }
            else {
                ai_stateMachine.PushState(State_ReturnToBase);
                ai_stateMachine.PushState(State_Aggresive);
                bHasBecomeAggresive = true;
            }
        }
    }


    protected virtual void State_ReturnToBase() {
        if(!bInConversation) {
            MoveReturnToStart();
        }
        else {
            BotWalk(Vector3.zero);
        }

        // Pretty close to start, return to base state
        if((transform.position - baseLocation).magnitude < 0.1f) {
            ai_stateMachine.PopState();
        }
    }


    protected override void State_Attacking() {
        targetPawn = DecideTarget();

        // Lost target, immediatly return to base state
        if(targetPawn == null) {
            bHasBecomeAggresive = false;
            ai_stateMachine.PopState();
            return;
        }

        MoveAttack();

        int abilityToUse = ShouldUseAbility();
        if(abilityToUse >= 0) {
            BotUseAbility(MAIN_ABILITY_INDEX);
        }

        // transitions
        if(!bInAttackRange) {
            ai_stateMachine.PopState();
        }
    }

}
