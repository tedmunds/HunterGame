using UnityEngine;
using System.Collections;

public class DroppedLootBag : DroppedItem {
	
    private Inventory inventory;

	void Start() {
        InitializeActor();
	}


	void Update() {
        UpdateActor();
	}



    public void InitBag(Pawn dropper, Inventory inv) {
        bHasLanded = false;
        startDropSpeed = Random.Range(0.0f, maxVelocity);
        velocity = Random.insideUnitCircle * startDropSpeed;
        velocity.y = Mathf.Abs(velocity.y);
        groundHeight = (dropper.transform.position - dropper.GetBaseOffset()).y;

        // TODO: get inventory of pawn and init this bag with it

        droppedBy = dropper;
        inventory = inv;
        gameObject.SetActive(true);
    }

    public override void PickedUpBy(Pawn user) {
        gameObject.SetActive(false);
    }
}