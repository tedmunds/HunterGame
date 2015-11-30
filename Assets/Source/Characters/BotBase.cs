using UnityEngine;
using System.Collections.Generic;


public abstract class BotBase : Pawn {

    public static int MIN_SWARM_SIZE = 3;
    public static int MAX_SWARM_SIZE = 10;

    protected const int MAIN_ABILITY_INDEX = 0;
    protected const float MAX_ABILITY_Y_OFFSET = 0.5f;

    protected enum AI_Intelligence {
        None, Wild, Mob, NPC
    }

    /** State machine */
    protected FiniteStateMachine ai_stateMachine;

    /** AI main properties */
    protected bool bIsAgressive;
    protected bool bGetsScared;
    protected bool bFearPlayer;
    protected bool bIsDynamic = true;
    protected AI_Intelligence intelligenceType;
    protected Vector2 detectionRange; // x = get target, y = loose target
    protected Ability mainAbility;
    protected float wanderMoveMultiplier = 1.0f;
    protected float runningMoveMultiplier = 1.0f;
    protected Item[] dropList;

    /** Wander movment behaviour */
    private static float wanderDirInterval = 2.0f;
    private static float maxWanderDelay = 3.0f;
    private float wanderDelay;
    private Vector2 wanderDirection;
    private float lastWanderChangeTime;
    private bool bLastWanderIntervalWasMoving;

    /** State variables */
    protected bool bIsAwake;

    /** Ais knowledge */
    protected Player playerRef;
    protected bool bDetectsPlayer;
    protected bool bInAttackRange;
    protected Pawn targetPawn;
    protected bool bRecievedDamageThisFrame;
    protected float lastAbilityUseTime;

    /** Set the AI's base properties */
    protected abstract void SetDefaultAIProperties();

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }

    protected virtual void OnEnable() {
        bIsDead = false;
        health = baseHealth;
        if(abilitiesInUse != null) {
            abilitiesInUse.Clear();
            for(int i = 0; i < activeEffects.Length; i++) {
                activeEffects[i].duration = 0.0f;
            }
        }
    }


    protected override void InitializeActor() {
        base.InitializeActor();
        SetDefaultAIProperties();
        ai_stateMachine = new FiniteStateMachine(State_Idle);

        lastWanderChangeTime = Time.time;
        bLastWanderIntervalWasMoving = false;
        wanderDirection = Vector2.zero;

        abilities.Add(mainAbility);
    }

    protected override void UpdateActor() {
        base.UpdateActor();

        // AI needs to always have a reference to the player
        if(world != null && playerRef == null) {
            playerRef = world.GetActivePlayer();
        }

        // the flag turns off all state transitions and movement
        if(!bIsDynamic) {
            return;
        }

        // Gather knowledge before evaluating and performing state functions / transitions
        if(bIsAwake) {
            bDetectsPlayer = CanDetectPlayer(playerRef);
            bInAttackRange = IsInAttackRange();

            // check if they have lost teh target for whatever reason. like invisibility or they died
            if(!CanDetectPlayer(playerRef)) {
                targetPawn = null;
                bDetectsPlayer = false;
            }
        }

        ai_stateMachine.Update();
        
        // Reset some bits of knowledge
        if(bIsAwake) {
            bRecievedDamageThisFrame = false;
        }

        // Set the animation speed to match the movement speed. If an ability is being used however, then the anim should always be speed 1
        if(abilitiesInUse.Count == 0) {
            animator.speed = movementComp.GetMovementModifier();
        }
        else {
            animator.speed = 1.0f;
        }
    }

    /** 
     * Called by world when this ai goes offscreen, so that the ai will not constantly be updating
     */ 
    public void SetAsleep() {
        ai_stateMachine.bIsActive = false;
        bIsAwake = false;
    }

    public void Wakeup() {
        ai_stateMachine.bIsActive = true;
        bIsAwake = true;
    }


    protected override void Died() {
        DropItemFromList();
        
        base.Died();
    }

    public virtual bool CanDetectPlayer(Player p) {
        if(p == null) {
            return false;
        }

        float distToPlayer = (transform.position - p.transform.position).magnitude;

        // dont currently see the player, then use the minimum range
        if(!bDetectsPlayer) {
            return distToPlayer <= detectionRange.x * (p.GetVisibility() / 100.0f);
        }
        else {
            return distToPlayer <= detectionRange.y * (p.GetVisibility() / 100.0f);
        }
    }

    // Check if there is a target and if our weapon is in range
    public virtual bool IsInAttackRange() {
        if(targetPawn == null || mainAbility == null) {
            return false;
        }

        float distToTarget = (transform.position - targetPawn.transform.position).magnitude;
        return distToTarget <= GetCurrentAbility().recommendedRange;
    }

    // Most bots only have one ability, but some may have more
    public virtual Ability GetCurrentAbility() {
        return abilities[MAIN_ABILITY_INDEX];
    }

    public override void TakeDamage(float damageAmount, Pawn instigator, bool bBroadcastEvent = true) {
        base.TakeDamage(damageAmount, instigator, bBroadcastEvent);
        bRecievedDamageThisFrame = true;
    }

    // Default bot behaviour is to assume the player is their target
    protected virtual Pawn DecideTarget() {
        return playerRef;
    }

    // Spawns an item from the bots drop list, returns the item dropped
    protected void DropItemFromList() {
        if(dropList == null) {
            return;
        }
        if(dropList.Length == 0) {
            return;
        }

        // Decide on the item type
        int dropIdx = Random.Range(0, dropList.Length-1);

        // Crafting items only get dropped some percentage of the time, based on their rarity score
        if(dropList[dropIdx].GetType() == typeof(CraftingItem)) {
            CraftingItem craftingItem = (CraftingItem)dropList[dropIdx];

            int randDropProb = Random.Range(0, 100);
            if(randDropProb > craftingItem.rarityScore) {
                return;
            }
        }

        // Spawn the container object
        GameObject dropObj = world.SpawnObject("P_DroppedItem", transform.position);
        if(dropObj == null) {
            Debug.Log("ERROR! Failed to spawn a DroppedItem container from "+name);
            return;
        }

        DroppedItem droppedContainter = dropObj.GetComponent<DroppedItem>();
        if(droppedContainter == null) {
            Debug.Log("ERROR! DroppedItem object "+dropObj.name+" doesn't have a DroppedItem component!");
            return;
        }

        if(dropList[dropIdx].GetType() == typeof(CraftingItem)) {
            droppedContainter.DropCraftingItem(this, (CraftingItem)dropList[dropIdx]);
        }
        else {
            droppedContainter.Drop(this, dropList[dropIdx].GetType());
        }
    }

    
    // Move around aimlessly, with a natural mixture of random directions and standing still
    protected virtual void MoveWander() {
        if(Time.time - lastWanderChangeTime > (wanderDirInterval + wanderDelay)) {
            // Alternate between resting intervals and moving intervals for a more natural seeming behaviour
            lastWanderChangeTime = Time.time;

            if(!bLastWanderIntervalWasMoving) {
                bLastWanderIntervalWasMoving = true;
                int directionSign = Random.Range(-1, 2);
                if(Random.Range(0.0f, 1.0f) > 0.5f) {
                    wanderDirection = new Vector2(0, directionSign);
                }
                else {
                    wanderDirection = new Vector2(directionSign, 0);
                }
            }
            else {
                bLastWanderIntervalWasMoving = false;
                wanderDirection = Vector2.zero;
            }

            // add a rendaom delay for the next interval
            wanderDelay = Random.Range(0.0f, maxWanderDelay);
        }

        movementComp.SetMovementModifier(wanderMoveMultiplier);
        BotWalk(wanderDirection);
    }

    // Chase directly after the target
    protected virtual void MoveChase() {
        if(targetPawn == null) {
            movementComp.Walk(Vector2.zero);
            return;
        }

        Vector3 toTarget = targetPawn.transform.position - transform.position;

        movementComp.SetMovementModifier(runningMoveMultiplier);
        BotWalk(toTarget.normalized);
    }

    protected virtual void MoveFlee() {
        if(targetPawn == null) {
            MoveWander();
            return;
        }

        Vector3 toTarget = targetPawn.transform.position - transform.position;

        movementComp.SetMovementModifier(runningMoveMultiplier);
        BotWalk(-toTarget.normalized);
    }


    protected virtual void MoveAttack() {
        // Attack move should just try and even out the y values so that ranged attacks can be made
        if(targetPawn == null) {
            return;
        }

        Vector3 toTarget = targetPawn.transform.position - transform.position;
        if(Mathf.Abs(toTarget.y) > MAX_ABILITY_Y_OFFSET) {
            toTarget = (toTarget.y > 0.0f)? Vector3.up : -Vector3.up;
        }
        else {
            BotWalk(Vector3.zero);
            return;
        }

        movementComp.SetMovementModifier(runningMoveMultiplier);
        BotWalk(toTarget.normalized);
    }


    public int ShouldUseAbility() {
        int abilityIdx = -1;
        if(targetPawn == null) {
            return abilityIdx;
        }

        float timeSinceLastUse = Time.time - lastAbilityUseTime;
        bool bWithinYRange = Mathf.Abs((transform.position - targetPawn.transform.position).y) < MAX_ABILITY_Y_OFFSET;

        if(timeSinceLastUse > GetCurrentAbility().coolDownTime && bWithinYRange) {
            abilityIdx = MAIN_ABILITY_INDEX;
        }

        return abilityIdx;
    }

    protected virtual void BotUseAbility(int abilityIndex) {
        bool bWasUsed = UseAbility(abilityIndex);
        if(bWasUsed) {
            lastAbilityUseTime = Time.time;
        }
    }

    // Wrapper for the movement component that follows the movement rules set by ability system
    protected void BotWalk(Vector2 direction) {
        if(!bCanMove) {
            direction = Vector2.zero;
        }
        movementComp.Walk(direction);
    }


    /**
     * Each state function needs to contain movement functionality as well as transition cases to other states
     */ 
    protected virtual void State_Idle() {
        // Idle movment wanders around aimlessly
        MoveWander();
        
        // transitions: Idle state never pops itself, so that any other state can always easily return to idle by poping
        if(bRecievedDamageThisFrame && !bFearPlayer) {
            ai_stateMachine.PushState(State_Aggresive);
        }
        else if(bDetectsPlayer && !bInAttackRange) {
            if(bIsAgressive) {
                ai_stateMachine.PushState(State_Aggresive);
            }
            else if(bFearPlayer) {
                ai_stateMachine.PushState(State_Flee);
            }
        }
        else if(bInAttackRange) {
            if(bIsAgressive) {
                ai_stateMachine.PushState(State_Attacking);
            }
            else if(bFearPlayer) {
                ai_stateMachine.PushState(State_Flee);
            }
        }
    }


    protected virtual void State_Aggresive() {
        targetPawn = DecideTarget();

        // Chase movement behaviour
        MoveChase();
        
        // transitions
        if(targetPawn == null || !bDetectsPlayer) {
            // Go back to idle
            ai_stateMachine.ClearStack();
            ai_stateMachine.PushState(State_Idle);
        }
        else if(bInAttackRange) {
            // leave aggresive on the stack so that if they get out of range they stay attacking
            ai_stateMachine.PushState(State_Attacking);
        }
    }


    protected virtual void State_Attacking() {
        targetPawn = DecideTarget();

        // Try to align y positions
        MoveAttack();

        int abilityToUse = ShouldUseAbility();
        if(abilityToUse >= 0) {
            BotUseAbility(MAIN_ABILITY_INDEX);
        }

        // transitions
        if(bDetectsPlayer && !bInAttackRange) {
            ai_stateMachine.PopState();
        }
        else if(!bDetectsPlayer) {
            ai_stateMachine.ClearStack();
            ai_stateMachine.PushState(State_Idle);
        }
    }


    protected virtual void State_Flee() {
        targetPawn = DecideTarget();
        
        MoveFlee();

        // transitions
        if(!bDetectsPlayer) {
            ai_stateMachine.PopState();
            ai_stateMachine.PushState(State_Idle);
        }
    }

}

