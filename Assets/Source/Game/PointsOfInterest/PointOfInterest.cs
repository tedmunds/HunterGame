using UnityEngine;
using System.Collections;

public class PointOfInterest : Actor {

    /** The npc to spawn that will become the owner of this POI */
    [SerializeField]
    private Pawn npcPrototype;

    [SerializeField]
    private Transform npcLocation;

    /** The npc that owns this point */
    private NPC owner;
	
	void Start () {
        InitializeActor();
	}
	
	
	void Update () {
        UpdateActor();
	}

    protected override void InitializeActor() {
        base.InitializeActor();

        SpawnOwner();
        if(owner != null) {
            owner.SetAsPOI(npcLocation.position);
        }
    }

    protected override void UpdateActor() {
        base.UpdateActor();
    }

    protected void SpawnOwner() {
        if(npcPrototype == null || npcLocation == null) {
            Debug.Log("WARNING! " + gameObject.name + " Cannot spawn an npc because there is no start position or no npc prototype!");
            return;
        }

        owner = (NPC)Instantiate(npcPrototype, npcLocation.position, Quaternion.identity);
    }
    
}
