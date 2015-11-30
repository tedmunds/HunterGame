using UnityEngine;
using System.Collections.Generic;

/** Abilites are used by pawns to affect other pawns and the world: They can range from spawning objects like fire pits to dealing damage to 
	some combination of those things. 

	Every ability goes through a lifecycle: When the pawn wants to use it it goes into the pending state, which is basically a warmup / intialization 
	state. This can last any amount of time, but if the pending time is 0, it will immediatly go to the next state (on the same frame)
	The next state is Activated. This is the place that any objects would be spawned (through the instigator pawn) or damage dealt. Finally
	there is the End state which is called as soon as the activated state is done (again it can last any amount of time). End is just for 
	whatever cleanup needs to be done, as well as notifying the instigator pawns its done.
  */
public abstract class Ability {

    // Distance range constants: what is considered 'adjacent' or 'nearby'
    const float ADJACENT_DISTANCE = 1.0f;
    const float NEARBY_DISTANCE = 5.0f;

	protected enum State {
		Inactive,
		Pending,
		Activated
	}

	/** Tag is how the ability is identified */
	public string tag;
	/** Numan friendly screen name */
	public string name;

	public Pawn instigator;
	public Pawn target;

	// time limits for each state, as well as the cumulative time 
	public float pendingTime;
	public float activeTime;
	public float totalDuration;

	// can it be used regardless of other abilities being active
	public bool bCanAlwaysUse;

    // Does this ability being active block other abilites being used
    public bool bBlocksUseWhenActive = true;

    /** How much to shake the camera on USE, not receivinbg if its an attack */
    public float cameraShakeFactor = 0.0f;

	/** These properties are general ability things that are checked by the pawn to govern its behaviour */
	public float coolDownTime;
	public bool bCanMoveWhilePending;
    public float recommendedRange;

	// timers
	public float pendingTimer;
	public float activeTimer;

	// Time the abilitiy was last used at
	public float useTime;

	public bool bIsInUse;

	public int activations;

    // What icon to display on HUD ability bar
    public Texture abilityIcon;

	/** Current state the ability is in: used only internally */
	protected State state;

	public Ability(float pendingTime, float activeTime) {
		bIsInUse = false;
		state = State.Inactive;

		this.pendingTime = pendingTime;
		this.activeTime = activeTime;
		totalDuration = pendingTime + activeTime;

        recommendedRange = 1.0f;

		tag = "Ability";

		pendingTimer = 0.0f;
		activeTimer = 0.0f;
	}

	public bool Use(Pawn instigatorPawn, Pawn targetPawn = null) {
		bIsInUse = true;
		instigator = instigatorPawn;
		target = targetPawn;

		if(!CanBeUsed()) {
			return false;
		}

		useTime = Time.time;
		activations += 1;

		Pending();

		return true;
	}

	public virtual bool CanBeUsed() {
		float timeSinceUse = Time.time - useTime;
		if(timeSinceUse >= coolDownTime || activations == 0) {
			return true;
		}

		return false;
	}

	public bool AllowMove() {
		return (state == State.Pending && bCanMoveWhilePending);
	}


	/** Called as soon as the ability has been used, sends it into the pending state */
	public virtual void Pending() {
		state = State.Pending;

		// If there is no pending phase, go straight to activate
		if(pendingTime == 0.0f) {
			Activate();
		}
	}

	/** Called as soon as the pending phase ends, sends it into the activated state */
	public virtual void Activate() {
		state = State.Activated;

		// If there is no pending phase, go straight to activate
		if(activeTime == 0.0f) {
			End();
		}
	}

	/** Called as soon as the activated phase ends, notifies pawn the ability is done and reset the ability */
	public virtual void End() {
		instigator.AbilityFinished(this);

		state = State.Inactive;
		bIsInUse = false;
		instigator = null;
		target = null;
		pendingTimer = 0.0f;
		activeTimer = 0.0f;
	}

	/** Tick function for the pending phase */
	protected virtual void UpdatePending() {
		pendingTimer += Time.deltaTime;
		if(pendingTimer >= pendingTime) {
			Activate();
		}
	}

	/** Tick function for the activated phase */
	protected virtual void UpdateActivated() {
		activeTimer += Time.deltaTime;
		if(activeTimer >= activeTime) {
			End();
		}
	}

	/** External tick function, the correct update is called depending on the state */
	public void Tick() {
		switch(state) {
		case State.Pending:
			UpdatePending();
			return;
		case State.Activated:
			UpdateActivated();
			return;
		}
	}

    // What percentage of time towards cooldown ending: 0 to 1
    public float GetCooldownPct() {
        float totalTime = Time.time - useTime;
        if(totalTime <= 0.0f || activations == 0) {
            return 1.0f;
        }
        return Mathf.Min(1.0f, totalTime / coolDownTime);
    }

    /*
     * Some overrideable mutators for various pawn properties that will be asked of this ability while it is active
     */

    public virtual float MutatedVisibility() {
        return 100.0f;
    }


    /*
     * Here on down is all Utility methods that a lot of different abilities will use. ex) collecting hits in different ranges etc. 
     */ 

    // Find all the pawns within distance of the input origin
    public List<Pawn> GetTargetsInRangeOf(Vector3 origin, float dist) {
        List<Pawn> adjacent = new List<Pawn>();

        foreach(Pawn p in instigator.GetWorld().VisiblePawns()) {
            if (p != instigator) {
                Vector3 toP = (origin - p.transform.position);
                if(toP.magnitude < dist) {
                    adjacent.Add(p);
                }
            }
        }

        return adjacent;
    }


    // Find all pawns in front of the instigator, within max distance
    public List<Pawn> GetTargetsInFront(float maxDist) {
        List<Pawn> inFrontOf = new List<Pawn>();

        Vector3 facing = instigator.GetFacingDirection();

        foreach(Pawn p in instigator.GetWorld().VisiblePawns()) {
            if(p != instigator) {
                Vector3 toP = (p.transform.position - instigator.transform.position);
                float dotToTarget = Vector3.Dot(facing, toP.normalized);

                // Must be in front and within range
                if(dotToTarget > 0.0f && toP.magnitude <= maxDist) {
                    inFrontOf.Add(p);
                }
            }
        }

        return inFrontOf;
    }


    // Returns the pawn attached to the collider if there is one, also checks the parent of the object
    public static Pawn IsColliderPawn(Collider2D c) {
        if(c == null) {
            return null;
        }

        Pawn p = c.GetComponent<Pawn>();

        if(p != null) {
            return p;
        }

        if(c.transform.parent != null) {
            p = c.transform.parent.GetComponent<Pawn>();
        }

        return p;
    }
}
