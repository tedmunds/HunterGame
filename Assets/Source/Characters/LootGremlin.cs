using UnityEngine;
using System.Collections;

public class LootGremlin : BotBase {

    /** How close must it be to grab an item from the bag */
    [SerializeField]
    private float grabDistance;

    /** How lon it takes to steal an item */
    [SerializeField]
    private float timeToSteal;

    [SerializeField]
    private float carryingSpeedMultiplier;

    /** how long to wait after spawning, before it goes to the bag */
    [SerializeField]
    private float spawnWaitDelay;

    private DroppedLootBag lootBag;

    private bool bHasGrabbedItem;

    private float reachedBagTime;
    private float spawnedTime;

    private Vector3 spawnLocation;

    private Animator spawnHole;

    protected override void SetDefaultAIProperties() {
        bIsAgressive = false;
        bGetsScared = false;
        bFearPlayer = true;
        intelligenceType = AI_Intelligence.Wild;
        detectionRange = new Vector2(3.0f, 7.0f);
        wanderMoveMultiplier = 1.0f;
        runningMoveMultiplier = 3.0f;

        dropList = new Item[0];
    }

    protected override void OnEnable() {
        base.OnEnable();

        bHasGrabbedItem = false;

        if(movementComp != null) {
            movementComp.KillAllMovement();
        }

        if(ai_stateMachine != null) {
            ai_stateMachine.ClearStack();
            ai_stateMachine.PushState(State_Idle);
        }
    }

    void Start() {
        InitializeActor();
    }

    void Update() {
        lootBag = WorldManager.instance.GetActivePlayer().GetDroppedLootBag();

        UpdateActor();
    }

    // Called when this gremlin is spawned: creates the its home base that it will return to after stealing
    public void DoSpawnEvent() {
        spawnLocation = transform.position;

        spawnedTime = Time.time;

        GameObject holeObj = WorldManager.instance.SpawnObject("P_GremlinHole", transform.position);
        spawnHole = holeObj.GetComponent<Animator>();
        if(spawnHole != null) {
            spawnHole.SetTrigger("Reset");
        }
    }

    private void DespawnWithLoot() {
        spawnHole.SetTrigger("Close");
        Actor holeActor = spawnHole.GetComponent<Actor>();
        if(holeActor != null) {
            holeActor.DeactivateDelayed(0.15f);
        }
        gameObject.SetActive(false);
    }


    protected override void Died() {
        spawnHole.SetTrigger("Close");
        Actor holeActor = spawnHole.GetComponent<Actor>();
        if(holeActor != null) {
            holeActor.DeactivateDelayed(0.15f);
        }

        base.Died();
    }

    protected void MoveAtBag() {
        if(lootBag == null) {
            return;
        }

        Vector2 directionToBag = (lootBag.transform.position - transform.position).normalized;

        movementComp.SetMovementModifier(runningMoveMultiplier);
        BotWalk(directionToBag);
    }


    protected void MoveToSpawn() {
        const float minimReturnDistance = 0.1f;
        Vector3 toSpawn = spawnLocation - transform.position;
        movementComp.SetMovementModifier(carryingSpeedMultiplier);
        BotWalk(toSpawn.normalized);

        // thay have returned to spawn, so they can despawn with their loot
        if((transform.position - spawnLocation).magnitude < minimReturnDistance) {
            DespawnWithLoot();
        }
    }

    // Gremlins immediatly go from idle to chasing the loot bag
    protected override void State_Idle() {
        float timeSinceSpawn = Time.time - spawnedTime;

        if(lootBag != null && timeSinceSpawn > spawnWaitDelay) {
            ai_stateMachine.PushState(State_GoToLootBag);
            return;
        }

        base.State_Idle();
    }


    protected void State_GoToLootBag() {
        // No longer have a bag to chase
        if(lootBag == null || WorldManager.instance.GetActivePlayer().IsCarryingLootBag()) {
            ai_stateMachine.PopState();
            return;
        }

        Vector3 toBag = lootBag.transform.position - transform.position;
        if(toBag.magnitude < grabDistance) {
            ai_stateMachine.PopState();
            ai_stateMachine.PushState(State_StealItemFromBag);
            reachedBagTime = Time.time;
            return;
        }

        MoveAtBag();
    }

    // Just waits around a bit and tris to steal an item
    private void State_StealItemFromBag() {
        if(lootBag == null || WorldManager.instance.GetActivePlayer().IsCarryingLootBag()) {
            ai_stateMachine.PopState();
            return;
        }

        float timeSinceReachedBag = Time.time - reachedBagTime;
        if(timeSinceReachedBag > timeToSteal) {
            ai_stateMachine.PopState();
            ai_stateMachine.PushState(State_ReturnToSpawn);

            bHasGrabbedItem = true;
        }

        // dont move at all in this state
        BotWalk(Vector2.zero);
    }


    // Gremlin tries to return to their spawn hole
    private void State_ReturnToSpawn() {
        MoveToSpawn();
    }


    
}