using UnityEngine;
using System.Collections.Generic;

// Traps are set up to support object pooling
[RequireComponent(typeof(Animator))]
public class TrapController : MonoBehaviour {

	[SerializeField]
	protected float triggerDamage;

	[SerializeField]
	protected float triggerRadius;

    /** Pre modified damage for trigger */
    protected int baseDamage = 500;

    /** Does the owner of this trap trigger it by walking on it */
    protected bool bCanOwnerTrigger = false;

	protected Animator animator;

	protected bool bHasBeenTriggered;

    // Who planted this trap
    protected Pawn owner;

	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void SetTrap(Pawn planter) {
        owner = planter;
    }


	void OnTriggerEnter2D(Collider2D other) {
		if(bHasBeenTriggered) {
			return;
		}

		Pawn victim = other.GetComponent<Pawn>();
        if(victim != null && (victim != owner || bCanOwnerTrigger)) {
			TriggerTrap(victim);
		}
	}

	public virtual void TriggerTrap(Pawn victim) {
		animator.SetTrigger("Deploy");
		bHasBeenTriggered = true;

        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, triggerRadius);

        for(int i = 0; i < overlaps.Length; i++) {
            Pawn hit = overlaps[i].GetComponent<Pawn>();
            if(hit != null && hit != owner) {
                hit.RecieveAttack(baseDamage, owner, Pawn.BodyPart.Torso);

            }
        }

	}

	public virtual void ResetTrap() {
		animator.SetTrigger("Reset");
		bHasBeenTriggered = false;
	}

}
