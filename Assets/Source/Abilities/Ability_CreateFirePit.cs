using UnityEngine;
using System.Collections;

public class Ability_CreateFirePit : Ability {

	private static Vector3 spawnOffset = new Vector3(0.0f, 0.5f, 0.0f);

    private GameObject fireObj;

	public Ability_CreateFirePit() : base(1.0f, 0.0f) {
		tag = "CreateFirePit";
		name = "Construct Fire";

		coolDownTime = 5.0f;

		bCanAlwaysUse = false;
		bCanMoveWhilePending = false;
	}

	public override void Pending() {
		instigator.DoMoveCrouch();

        if(fireObj != null) {
            fireObj.SetActive(false);
        }

		base.Pending();
	}

	public override void Activate() {
        // There can only be one cap fire at a time
        if(fireObj == null) {
            fireObj = instigator.AbilityWantsObject("P_FirePit", instigator.transform.position - spawnOffset);
        }
        else {
            fireObj.transform.position = instigator.transform.position - spawnOffset;
            fireObj.SetActive(true);
        }

        CampFireController fireController = fireObj.GetComponent<CampFireController>();
        if(fireController != null) {
            fireController.PlacedBy((Player)instigator);
        }

		instigator.EndMoveCrouch();

		base.Activate();
	}
}
