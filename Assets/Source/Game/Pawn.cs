using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pawn : Actor {

	public enum Facing {
		left, right
	}

	protected struct CachedAnimData {
		public Facing mirroring; 
		public float speed;
		public bool crouch;
		public bool throwing;
        public float transperency;
        public bool bFlagAttack;
	}

	/** Effect types */
	public static int NUM_EFFECTS = 7;
	public enum EffectType {
		Burning,
		Bleeding,
		Broken,
		Knockdown,
		Dazed,
		Poison,
		Blind
	}

	/** Pawn hit targets */
	public static int NUM_BODYPARTS = 11;
	public enum BodyPart {
		Head, 
		Torso,
		LeftArm,
		RightArm,
		LeftHand,
		RightHand,
		Groin,
		LeftLeg,
		RightLeg,
		LeftFoot, 
		RightFoot
	}

    /** Special moves are a state that the pawn can go into, usually set from outside the pawn. ie through abilities */
    public enum SpecialMove {
        SM_None,
        SM_Crouch,
        SM_Throw,
        SM_MeleeAttack,
        SM_Hidden,
        SM_Attack,
    }


	protected static Vector3 UN_MIRRORED_SCALE = new Vector3(1.0f, 1.0f, 1.0f);
	protected static Vector3 MIRRORED_SCALE = new Vector3(-1.0f, 1.0f, 1.0f);
    protected static float MAX_VISIBILITY = 100.0f;

	/** Highest value for stats and resistances */
	protected static int MAX_STAT_VALUE = 20;

    protected const int INVENTORY_SIZE = 20;

	[SerializeField]
	protected float baseHealth;

    [SerializeField] // this is on recieving damage effects
    protected ParticleSystem bloodEffectsPrototype;

    [SerializeField]
    protected ParticleSystem fireEffectsPrototype;

    [SerializeField]
    protected ParticleSystem bleedEffectsPrototype;

    [SerializeField]
    protected ParticleSystem poisonEffectsPrototype;

    /** Effect particle system instances. Only created as they are neaded by the pawn */
    protected bool bDoesConditionEffects = true;
    protected ParticleSystem bloodEffectSystem;
    protected ParticleSystem fireEffectSystem;
    protected ParticleSystem bleedEffectSystem;
    protected ParticleSystem poisonEffectSystem;

    /** Bits of pawns gameplay state */
	protected float health;
    protected bool bIsDead;
    protected float visibility;

	protected bool bCanMove = true;
    protected bool bCanModifyVisibilty = false;
    protected bool bTakesDamage = true;
	
	protected CharacterMover movementComp;
	protected Animator animator;
	protected CachedAnimData cachedAnimData;

    protected SpecialMove specialMove;

	private float lastDamageTime;
	private float damageColorFlashLength = 0.2f;

    protected bool bIsOnGround = true;
    /** The y pos when the pawn initiated the jump */
    protected float groundHeightForJump;
    protected float jumpVelocity;

    /** Every pawn stores a reference to the world manager */
	protected WorldManager world;

	/** Main list of abilitier available to this pawn */
	protected List<Ability> abilities;
	/** List of abilities that are currently bing updated / used */
	protected List<Ability> abilitiesInUse;
	/** Queue for abilities that were used but have to wait until the current one is finished */
	protected Queue<Ability> pendingAbilities;

	protected int maxQueuedAbilities = 1;
	protected float maxQueuedWait = 1.0f;

	/** A sort of heurisitic of the effects on this actor and their durations etc */
	protected Effect[] activeEffects;
	/** List of stat levels, indecies defined by the stats enum: value b/w 0 and 20 */
	protected int[] stats;

	/** 
     * resistances to the active effects: indexes defined by the effecttypes enum: Each resistance is used in different weays by each effect
     * Range = 0.0f to 1.0f
     */
	protected float[] resistances;

    protected List<DroppedItem> nearbyDrops;
    protected bool bCanPickupItems = false;

    protected Inventory inventory;
    /** Should items in inventory cause effects on the pawn */
    protected bool bItemsCauseEffects = false;

    /** Other pawn that this pawn is targeting for attack */
    protected BodyPart attackTargetLocation;

    /** Transform for the pawns shadow object, can be null if it's not found on intiliazation */
    protected Transform shadowTransform;

	public void SetCanMove(bool b) { bCanMove = b; }
    public List<DroppedItem> GetNearbyDrops() { return nearbyDrops; }
	public int[] GetStats() { return stats; }
	public float[] GetResistances() { return resistances; }
    public bool IsDead() { return bIsDead; }
    public WorldManager GetWorld() { return world; }
    public float GetVisibility() { return visibility; }
    public Inventory GetInventory() { return inventory; }

    public float GetResistanceFor(EffectType effect) {
        return resistances[(int)effect];
    }

    public void AddResistanceTo(EffectType effect, float amount) {
        // clamp the resistance value b/w 1 and 0
        resistances[(int)effect] += Mathf.Clamp(resistances[(int)effect] + amount, 0.0f, 1.0f);
    }

	void Start() {
		InitializeActor();
	}

	void Update() {
		UpdateActor();
	}

    // Update world manager on which pawns are currently visible
    void OnBecameVisible() {
        world.PawnBecameVisible(this);
    }

    void OnBecameInvisible() {
        world.PawnBecameInvisible(this);
    }

    // current health over Base health
    public float GetHealthPercent() {
        return ((float)health / (float)baseHealth);
    }

    // Vector pointing int he direction they are facing
    public Vector3 GetFacingDirection() {
        return (cachedAnimData.mirroring == Facing.right)? Vector3.right : -Vector3.right;
    }

	protected override void  InitializeActor() {
		base.InitializeActor();

		world = FindObjectOfType<WorldManager>();
		if(world ==  null) {
			Debug.Log("ERROR!     "+name+" couldn't find the WorldManager on initialization!");
		}

		// Store all anim info to be set in the animator at the end of the frame
		cachedAnimData = new CachedAnimData();
        cachedAnimData.transperency = 1.0f;

		movementComp = GetComponent<CharacterMover>();
		if(movementComp == null) {
			Debug.Log("WARNING!      "+name+" :: Doesn't have a movement component!");
		}

		animator = GetComponent<Animator>();
		if(animator == null) {
			Debug.Log("WARNING!      "+name+" :: Doesn't have an animator!");
		}
		animator.logWarnings = false;

        // Add the blood particle effect at init b/c its so frequently used. The others are added as needed
        if(bDoesConditionEffects && bloodEffectsPrototype != null) {
            bloodEffectSystem = AddEffectAttached(bloodEffectsPrototype, Vector3.zero);
        }

		health = baseHealth;
		cachedAnimData.mirroring = Facing.right;
        bIsDead = false;
        specialMove = SpecialMove.SM_None;

        if(!bCanModifyVisibilty) {
            visibility = MAX_VISIBILITY;
        }

		// Ability sets
		abilities = new List<Ability>();
		abilitiesInUse = new List<Ability>();
		pendingAbilities = new Queue<Ability>();

		// Create the effect heuristic: The effect classes apear in the same order as the effects enum
		activeEffects = new Effect[NUM_EFFECTS];
        activeEffects[(int)EffectType.Burning] = new Effect_HealthDrain(this, EffectType.Burning, 50, 0.3f);
        activeEffects[(int)EffectType.Bleeding] = new Effect_HealthDrain(this, EffectType.Bleeding, 10, 0.5f);
        activeEffects[(int)EffectType.Broken] = new Effect_Broken(this, EffectType.Broken);
        activeEffects[(int)EffectType.Knockdown] = new Effect_Knockdown(this, EffectType.Knockdown);
        activeEffects[(int)EffectType.Dazed] = new Effect_Daze(this, EffectType.Dazed);
        activeEffects[(int)EffectType.Poison] = new Effect_HealthDrain(this, EffectType.Poison, 25, 0.5f);
        activeEffects[(int)EffectType.Blind] = new Effect_Blind(this, EffectType.Blind);

		// One resistance stat for each effect type
		resistances = new float[NUM_EFFECTS];
		for(int i = 0; i < resistances.Length; i++) { 
			resistances[i] = 0.0f; 
		}

		// Stat Table: starts out 0'd
        //stats = new int[NUM_STATS];
        //for(int i = 0; i < stats.Length; i++) { 
        //    stats[i] = 0; 
        //}

        inventory = new Inventory(INVENTORY_SIZE);

        // Just set default attack location to be the torso
        attackTargetLocation = BodyPart.Torso;

        nearbyDrops = new List<DroppedItem>();

        // Search all children transforms for the shadow one
        if(transform.childCount > 0) {
            for(int i = 0; i < transform.childCount; i++) {
                if(transform.GetChild(i).GetComponent<SpriteRenderer>()) {
                    shadowTransform = transform.GetChild(i);
                    break;
                }
            }
        }
	}
	
	protected override void UpdateActor() {
		base.UpdateActor();

        if(movementComp == null) {
            return;
        }

		// Determine facing based on the movement speed
		float xVelocity = movementComp.xVel();
		if(xVelocity < -0.02f) {
			cachedAnimData.mirroring = Facing.left;
		}
		else if(xVelocity > 0.02f) {
			cachedAnimData.mirroring = Facing.right;
		}

		cachedAnimData.speed = movementComp.NormalizedVel();

		if(Time.time - lastDamageTime > damageColorFlashLength && spriteRenderer.color != Color.white) {
			spriteRenderer.color = Color.white;
		}

        // Update jump offset
        if(!bIsOnGround) {
            transform.position += new Vector3(0.0f, jumpVelocity * Time.deltaTime, 0.0f);
            jumpVelocity -= 9.8f * Time.deltaTime;

            if(shadowTransform != null) {
                shadowTransform.position = new Vector3(transform.position.x, groundHeightForJump, 0.0f);
            }

            if(transform.position.y - GetBaseOffset().y < groundHeightForJump) {
                transform.position = new Vector3(transform.position.x, groundHeightForJump + GetBaseOffset().y, 0.0f);
                bIsOnGround = true;
                jumpVelocity = 0.0f;
                movementComp.SetMovemetCollision(true);
            }
        }

		// The following things should only be updated when the game is not paused for whatever reason
		bool bIsPaused = world.GetIsCombatPaused();
		if(!bIsPaused) {
            // Allows abilities to modify current visibility ratings
            if(bCanModifyVisibilty) {
                visibility = GetVisiblityRating();
            }

			// Update all active abilities and check any restrictions the place on the pawn
			for(int i = abilitiesInUse.Count - 1; i >= 0; i--) {
                abilitiesInUse[i].Tick();
			}

			// Update the active effects. All effects are considered active, and the class itself is the one handles timing functionality
			for(int i = 0; i < activeEffects.Length; i++) {
				if(activeEffects[i].duration > 0.0f) {
					activeEffects[i].Update();
				}
                else {
                    // Stop any paticle systems for an effect that has ended
                    if(i == (int)EffectType.Poison && poisonEffectSystem != null && poisonEffectSystem.isPlaying) {
                        poisonEffectSystem.Stop();
                        poisonEffectSystem.Clear();
                    }
                    if(i == (int)EffectType.Bleeding && bleedEffectSystem != null && bleedEffectSystem.isPlaying) {
                        bleedEffectSystem.Stop();
                        poisonEffectSystem.Clear();
                    }
                    if(i == (int)EffectType.Burning && fireEffectSystem != null && fireEffectSystem.isPlaying) {
                        fireEffectSystem.Stop();
                        fireEffectSystem.Clear();
                    }
                }
			}

            // Check all items in inventory for effects
            if(bItemsCauseEffects) {
                foreach(Item itemInInventory in inventory.Items()) {
                    if(itemInInventory.bIsActive) {
                        itemInInventory.UpdateEffect(this);
                    }
                }
            }
		}

	}


    /** Gets the position at the bottom of the sprite bounds */
    public Vector3 GetFootPosition() {
        return transform.position - GetBaseOffset();
    }



	protected virtual void ApplyCachedAnimData() {
		// always set mirroring
		if(cachedAnimData.mirroring == Facing.right) {
			transform.localScale  = UN_MIRRORED_SCALE;
		}
		else if(cachedAnimData.mirroring == Facing.left) {
			transform.localScale  = MIRRORED_SCALE;
		}

        // Animator settings
		if(animator == null) {
			return;
		}
        
		animator.SetFloat("Speed", cachedAnimData.speed);
		animator.SetBool("Crouch", cachedAnimData.crouch);
		animator.SetBool("Throwing", cachedAnimData.throwing);

        // Set triggers from cached flags
        if(cachedAnimData.bFlagAttack) {
            animator.SetTrigger("Attack");
            cachedAnimData.bFlagAttack = false;
        }

        // Renderer settings
        if(GetComponent<Renderer>() == null) {
            return;
        }

        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 
                                         cachedAnimData.transperency);
	}


    // Checks if the current weapon should make an attack: is it in range and has enough time passed since the last attack
    //public virtual bool CheckForWeaponAttack(Weapon currentWeapon, Pawn target) {
    //    if (target == null) {
    //        return false ;
    //    }


    //    float distToTarget = (target.GetFootPosition() - GetFootPosition()).magnitude;
    //    AttackType attack = currentWeapon.attackTypes[currentWeapon.selectedAttackType];

    //    if (distToTarget > attack.maxRange) {
    //        return false;
    //    }

    //    float timeSinceLastAttack = Time.time - currentWeapon.timeOfLastAttack;
    //    float weaponAttackRate = currentWeapon.baseAttackRate * attack.attackSpeedModifier;
    //    if (timeSinceLastAttack <= weaponAttackRate) {
    //        return false;
    //    }

    //    // In range and enough time has passed to do attack
    //    return true;
    //}


    public virtual float GetDamageBonus() {
        return 0.0f;
    }

    public virtual float GetDamageReduction() {
        return 0.0f;
    }


	/** Main damage interface */
	public virtual void RecieveAttack(float damageAmount, Pawn instigator, BodyPart targetLocation) {
		// First determine if the attack hits
        bool bAttackHit = true;

        if(!bTakesDamage) {
            return;
        }

        // If ther was no attack type, it means to not check to hit
        if(bAttackHit) {
            // Save teh damage before it gets reduced by toughness and armor, so that it can be restored in some cases
            float damageBeforeReduction = damageAmount;

            damageAmount -= GetDamageReduction();

            if (damageAmount < 0) {
                damageAmount = 0;
            }

            // Finally, apply the damage
            TakeDamage(damageAmount, instigator);

            // Do blood effects (if there is a system)
            if(bloodEffectSystem != null) {
                bloodEffectSystem.Play();
            }

            lastDamageTime = Time.time;
            spriteRenderer.color = Color.red;
        }
	}


    // Directly applies damage. It will not be modified at all: the normal damage pipline is through RecieveAttack
	public virtual void TakeDamage(float damageAmount, Pawn instigator, bool bBroadcastEvent = true) {
        if(!bTakesDamage || bIsDead) {
            return;
        }
        
        health = Mathf.Max(health - damageAmount, 0);
		if(health <= 0) {
			Died();
		}

        if(bBroadcastEvent) {
            world.DamageEvent(damageAmount, this, instigator);
        }
	}

    // Heal some amount of current health without going over max
    public virtual void HealAmount(float healAmount) {
        if(bIsDead) {
            return;
        }

        health = Mathf.Min(health + healAmount, baseHealth);
    }


	/** Check if the attack lands based on a dice roll, and the attackers / defenders stats. There is a chance the hit location will change */
    //public bool ResolveAttackHit(Pawn attacker, AttackType attackType, ref BodyPart hitLocation) {
    //    if(attackType == null) {
    //        return false;
    //    }
        
    //    const int toHitWeight = 5; // Weight that favors hits b/c missing kinda sucks
    //    bool bHit = false;

    //    int attackerStat = attacker.GetStats()[(int)attackType.offensiveStat];
    //    int defenderStat = stats[(int)attackType.defensiveStat];

    //    // TODO: distribute random values on curve
    //    int attackVal = attackerStat + Random.Range(0, 20) + toHitWeight;
    //    int defendVal = defenderStat + Random.Range(0, 20) + BodyPartHitModifiers[(int)hitLocation];
		
    //    if(attackVal > defendVal) {
    //        bHit = true;
    //    }

    //    // Almost missed or if the attack canot be targeted anyways, so readjust the hit location to new random one
    //    if(attackVal == defendVal || !attackType.bCanBeTargeted) {
    //        bHit = true;
    //        hitLocation = (BodyPart)Random.Range(0, NUM_BODYPARTS);
    //    }

    //    return bHit;
    //}


	protected virtual void Died() {
        world.DeathEvent(this);
        bIsDead = true;

        // TODO: Play death animation
        gameObject.SetActive(false);
	}

    public virtual void Revive() {
        bIsDead = false;
        health = baseHealth;
        bCanMove = true;
        visibility = 100.0f;

        abilitiesInUse.Clear();
        
        for(int i = 0; i < activeEffects.Length; i++) {
            activeEffects[i].duration = 0.0f;
        }

        gameObject.SetActive(true);
    }


	/** abilityIdx is which ability rto use, forceUse will prevent it from failing if there is an ability in use already */
	public bool UseAbility(int abilityIdx, bool forceUse = false, bool queueIfFailed = true) {
		if(abilityIdx < 0 || abilityIdx >= abilities.Count) {
			Debug.Log("WARNING!  "+name+" tried to use ability at index: "+abilityIdx+", which is not assigned!");
			return false;
		}

		Ability ability = abilities[abilityIdx];
		bool wasUsed = false;

        bool canUse = true;

        // First check if there are any abilities blocking this use
        if(abilitiesInUse.Count > 0 && !ability.bCanAlwaysUse) {
            for(int i = 0; i < abilitiesInUse.Count; i++) {
                if(abilitiesInUse[i].bBlocksUseWhenActive) {
                    canUse = false;
                }
            }
        }

        if(!ability.CanBeUsed()) {
            canUse = false;
        }
        
		if(canUse || forceUse) {
			UseAbility(ability);
			wasUsed = true;
		}
		else if(queueIfFailed && (pendingAbilities.Count < maxQueuedAbilities) && (ability.coolDownTime < maxQueuedWait)){
			pendingAbilities.Enqueue(ability);
		}

		return wasUsed;
	}

	/** Called by the ability when it's End method is called */ 
	public void AbilityFinished(Ability ended) {
		abilitiesInUse.Remove(ended);

		// Activate the next queued ability (if any of course)
		if(pendingAbilities.Count > 0) {
			Ability next = pendingAbilities.Dequeue();
			if(next != null) {
				UseAbility(next);
			}
		}
	}

	// Little utility for using abilities that ensures they are added to teh correct lists
	// TODO: some way of using targets?
	private void UseAbility(Ability a) {
		// Dont bother adding to the list if it is instant use
		bool used = a.Use(this);

		if(a.totalDuration > 0.0f && used) {
			abilitiesInUse.Add(a);
		}
	}


	public GameObject AbilityWantsObject(string objName, Vector3 objLoc) {
		return world.SpawnObject(objName, objLoc);
	}


    public void DropItem(Item itemToDrop) {
        GameObject dropObj = world.SpawnObject("P_DroppedItem", transform.position);
        if(dropObj == null || itemToDrop == null) {
            return;
        }

        DroppedItem itemContainer = dropObj.GetComponent<DroppedItem>();
        if(itemContainer == null) {
            return;
        }

        if(itemToDrop.GetType() == typeof(CraftingItem)) {
            itemContainer.DropCraftingItem(this, (CraftingItem)itemToDrop);
        }
        else {
            itemContainer.Drop(this, itemToDrop.GetType());
        }
    }



    public void ApplyInstantForce(Vector3 direction, float force) {
        float vertComponent = direction.normalized.y * force;
        float horzComponent = direction.normalized.x * force;

        movementComp.ApplyHorizontalForce(horzComponent);
        movementComp.ApplyVerticalForce(vertComponent);
    }


    // Attempts to add the item to the pawns inventory. If it fails it returns false
    public bool AddItemToInventory(Item newItem) {
        if(newItem == null) {
            return false;
        }
        
        // There is two cases; if the item is a crafing item, them it goes intot he crafting section of the inventory
        bool bAdded = false;
        if(newItem.GetType() == typeof(CraftingItem)) {
            bAdded = inventory.AddCraftingItem((CraftingItem)newItem);
        }
        else {
            bAdded = inventory.AddItem(newItem);
        }

        // Begin the effect of the item
        if(bAdded && bItemsCauseEffects) {
            newItem.BeginEffect(this);
        }

        return bAdded;
    }


    // Attempts to remove the item from the inventory
    public void DropItemFromInventory(Item item) {
        if(item == null) {
            return;
        }

        if(!inventory.RemoveItem(item)) {
            return;
        }

        // Potentially end the items effect. Only necessary if the effect was on
        if(bItemsCauseEffects && item.bIsActive) {
            item.EndEffect(this);
        }
       
        // Spawn the world object for the item
        DropItem(item);
    }

    // Called by DroppedItem when this pawn is in range of it: Pawn maintains a list of all items in range
    public void DropIsNearby(DroppedItem drop) {
        if(!bCanPickupItems) {
            return;
        }

        if(!nearbyDrops.Contains(drop)) {
            nearbyDrops.Add(drop);
        }
    }

    // Called by DroppedItem when this pawn leaves its range
    public void DopIsOutOfRange(DroppedItem drop) {
        if(!bCanPickupItems) {
            return;
        }

        if(nearbyDrops.Contains(drop)) {
            nearbyDrops.Remove(drop);
        }
    }


    // Initiates a little visual jump
    public void DoJump(float jumpVel) {
        if(jumpVel < 0.0f) {
            return;
        }

        bIsOnGround = false;
        groundHeightForJump = (transform.position - GetBaseOffset()).y;
        jumpVelocity = jumpVel;
        movementComp.SetMovemetCollision(true);
    }



    // Is this pawns health info considered relavant: assumes they are already on screen
    public virtual bool HealthIsRelevant() {
        return true;
    }

    /** Returns current visibility rating from 0 (invisible) to 100 (normal visibility) */
    private float GetVisiblityRating() {
        if(abilitiesInUse.Count > 0) {
            float visibity = 100.0f;
            foreach(Ability a in abilitiesInUse) {
                float modifiedVis = a.MutatedVisibility();
                if(modifiedVis < visibity) {
                    visibity = modifiedVis;
                }
            }

            return visibity;
        }

        return 100.0f;
    }


	// Effect mutators -------------------------------------------------------------------------------------------------

    // Clears any duration of the input effect type
    public void RemoveEffect(EffectType effect) {
        activeEffects[(int)effect].duration = 0.0f;
    }

	public void AddBurning(float duration) {
		activeEffects[(int)EffectType.Burning].duration += duration;

        // Should add effect and it hasnt been added yet
        if(bDoesConditionEffects && fireEffectsPrototype != null && fireEffectSystem == null) {
            fireEffectSystem = AddEffectAttached(fireEffectsPrototype, Vector3.zero);
        }

        if(bDoesConditionEffects && fireEffectSystem != null) {
            fireEffectSystem.Play();
        }
	}

	public void AddBleeding(float duration) {
		activeEffects[(int)EffectType.Bleeding].duration += duration;

        // Should add effect and it hasnt been added yet
        if(bDoesConditionEffects && bleedEffectsPrototype != null && bleedEffectSystem == null) {
            bleedEffectSystem = AddEffectAttached(bleedEffectsPrototype, Vector3.zero);
        }

        if(bDoesConditionEffects && bleedEffectSystem != null) {
            bleedEffectSystem.Play();
        }
	}

    public void AddPoison(float duration) {
        activeEffects[(int)EffectType.Poison].duration += duration;

        // Should add effect and it hasnt been added yet
        if(bDoesConditionEffects && poisonEffectsPrototype != null && poisonEffectSystem == null) {
            poisonEffectSystem = AddEffectAttached(poisonEffectsPrototype, Vector3.zero);
        }

        if(bDoesConditionEffects && poisonEffectSystem != null) {
            poisonEffectSystem.Play();
        }
    }

	public void AddBroken(BodyPart brokenTarget) {
		Effect_Broken b = (Effect_Broken)(activeEffects[(int)EffectType.Broken]);
		b.brokenLimbs[(int)brokenTarget] = true;
	}

	public void AddKnockdown(float duration) {
		activeEffects[(int)EffectType.Knockdown].duration += duration;
	}

	public void AddDazed(float duration) {
		activeEffects[(int)EffectType.Dazed].duration += duration;
	}

    // returns the most potent effect semantically: primarily for UI coloring and stuff
    public EffectType GetMostSevereEffect() {
        if(activeEffects[(int)EffectType.Burning].duration > 0.0f) {
            return EffectType.Burning;
        }
        if(activeEffects[(int)EffectType.Poison].duration > 0.0f) {
            return EffectType.Poison;
        }
        if(activeEffects[(int)EffectType.Bleeding].duration > 0.0f) {
            return EffectType.Bleeding;
        }

        // Dont care about the others right now
        return (EffectType)(-1);
    }

	// Special moves ------------------------------------------------------------------------------------------------

    public bool bCanEnterSpecialMove() {
        return specialMove == SpecialMove.SM_None;
    }

	public void DoMoveCrouch() {
        specialMove = SpecialMove.SM_Crouch;
		bCanMove = false;
		cachedAnimData.crouch = true;
	}
	public void EndMoveCrouch() {
        specialMove = SpecialMove.SM_None;
		bCanMove = true;
		cachedAnimData.crouch = false;
	}

	public void DoMoveThrow() {
        specialMove = SpecialMove.SM_Throw;
		bCanMove = false;
		cachedAnimData.throwing = true;
	}
	public void EndMoveThrow() {
        specialMove = SpecialMove.SM_None;
		bCanMove = true;
		cachedAnimData.throwing = false;
	}

    public void DoMoveMeleeAttack() {
        specialMove = SpecialMove.SM_MeleeAttack;
        bCanMove = false;
        // TODO: animation
    }
    public void EndMoveMeleeAttack() {
        specialMove = SpecialMove.SM_None;
        bCanMove = true;
        // TODO: animation
    }

    public void DoMoveHidden(float transparencyPct) {
        specialMove = SpecialMove.SM_Hidden;
        cachedAnimData.transperency = transparencyPct;
    }
    public void EndMoveHidden() {
        specialMove = SpecialMove.SM_None;
        cachedAnimData.transperency = 1.0f;
    }

    public void DoMoveAttack() {
        specialMove = SpecialMove.SM_Attack;
        bCanMove = false;
        cachedAnimData.bFlagAttack = true;
    }
    public void EndMoveAttack() {
        specialMove = SpecialMove.SM_None;
        bCanMove = true;
    }
}







