//#define final

using UnityEngine;
using System.Collections;

public class Player : Pawn {

    private const int MAIN_ATTACK_INDEX = 0;
    private const int CAMP_FIRE_INDEX = 5;

    /** How many abilities the play has in their list */
    public const int NUM_ABILTIES = 4;

	[SerializeField]
	private PlayerCamera cameraPrototype;

    [SerializeField]
    private LayerMask clickSelectLayer;

    [SerializeField]
    private Texture tempTex;

    [SerializeField]
    private SpriteRenderer lootBagSprite;

    private bool bTalkingToNPC;

    private string[] abilityControlKeys = { "A", "Z", "X", "C", "V" , "R"};

	private PlayerCamera myCamera;
	private PlayerState playerState;

    private UI_PlayerBase ui;

    private const bool bCanTalkAndWalk = false;
    public const bool bAutoPickup = true;
    private const float pickupPullSpeed = 3.0f;

    private bool bCarryingLootBag = true;
    private DroppedLootBag droppedLootBag;

    private UI_InventoryMenu inventoryMenu;

    /** If an npc is in range to interact with, they will be stored here */
    private NPC interactableNPC;

	public void SetState(PlayerState newState) {
		if(newState == null) { return; }
		playerState = newState;
	}

    public string[] GetAbiltyControlKeys() { return abilityControlKeys; }
    public Ability[] GetAbilitySet() { return abilities.ToArray(); }
    public NPC GetInteractableNPC() { return interactableNPC; }
    public bool IsTalkingToNPC() { return bTalkingToNPC; }
    public bool IsCarryingLootBag() { return bCarryingLootBag; }
    public DroppedLootBag GetDroppedLootBag() { return droppedLootBag; }

    public Camera GetCamera() {
        return myCamera.GetComponent<Camera>();
    }


	void Start() {
		InitializeActor();

        //Cursor.visible = false;
	}
	
	void Update() {
		UpdateActor();
	}
	
	protected override void  InitializeActor() {
		base.InitializeActor();

        bCanPickupItems = true;
        bItemsCauseEffects = true;
        bCanModifyVisibilty = true;

		if(cameraPrototype == null) {
			Debug.Log("ERROR!       Player actor "+name+" has no camera prototype!");
			return;
		}
		myCamera = (PlayerCamera)GameObject.Instantiate(cameraPrototype, transform.position + new Vector3(0.0f, 0.0f, -10.0f), transform.rotation);
		if(myCamera != null) {
			myCamera.InitailizeCamera(this);
		}
		else {
			Debug.Log("ERROR!       Player actor "+name+" failed to initialize its camera because it couldn'y instantiate it!");
		}

        ui = GetComponent<UI_PlayerBase>();
        if(ui == null) {
            Debug.Log("WARNING!       Player actor " + name + " doesn't have any UI component!");
        }

        // Tries to find a UI menu to use as the players inventory menu
        inventoryMenu = GameObject.FindObjectOfType<UI_InventoryMenu>();
        if(inventoryMenu == null) {
            Debug.Log("WARNING!       Player actor " + name + " could not find a UI_InventoryMenu in the scene!");
        }
        else {
            inventoryMenu.gameObject.SetActive(false);
        }

		// HACK: temporarily adding some abilities. haven't decided how this will actually work yet
        abilities.Add(new Ability_HunterAttack()); // Main attack ability in first place

		abilities.Add(new Ability_Invisibility());
		abilities.Add(new Ability_PlaceTrap());
        abilities.Add(new Ability_TossBomb());
        abilities.Add(new Ability_Slash());

        // Player always has the cap fire ability
        abilities.Add(new Ability_CreateFirePit());

        //// TEMP: give the player some stuff in their inventory and try equipping the sword
        //inventory.Add(new Sword());
        //inventory.Add(new Bow());
        //EquipWeapon((Weapon)inventory[0]);

        ui.InitializeUI();
	}
	
	protected override void UpdateActor() {
		base.UpdateActor();

		Vector2 movemntDir = Vector2.zero;

        float hMag = Input.GetAxis("Horizontal");
        float vMag = Input.GetAxis("Vertical");
        movemntDir = new Vector2(hMag, vMag).normalized;

        // Check that interactions and abilities can occur
        if(!bTalkingToNPC) {
            // Main attack key: TODO: Make all this stuff configurable
            if(Input.GetKeyDown(KeyCode.A)) {
                PlayerUseAbility(MAIN_ATTACK_INDEX);
            }

		    // HACK: how should abilities actually get used by player??
		    if(Input.GetKeyDown(KeyCode.Z)) {
                PlayerUseAbility(1);
		    }
		    if(Input.GetKeyDown(KeyCode.X)) {
                PlayerUseAbility(2);
		    }
            if(Input.GetKeyDown(KeyCode.C)) {
                PlayerUseAbility(3);
            }
            if(Input.GetKeyDown(KeyCode.V)) {
                PlayerUseAbility(4);
            }      
  
            if(Input.GetKeyDown(KeyCode.R)) {
                PlayerUseAbility(CAMP_FIRE_INDEX);
            }

            // Try to pick up an item
            if(Input.GetKeyDown(KeyCode.E)) {
                PerformInteraction();
            }

            // TEMP: decide what button this should be
            if(Input.GetKeyDown(KeyCode.Space)) {
                if(bCarryingLootBag) {
                    DropLootBag();
                }
                else {
                    PickUpLootBag();
                }
            }

            // Toggles the inventory menu
            if(Input.GetKeyDown(KeyCode.I)) {
                inventoryMenu.gameObject.SetActive(!inventoryMenu.gameObject.activeSelf);
            }
        }

        // Cancle any moves cases
        if(PreventMovement()) {
			movemntDir = Vector2.zero;
		}

		movementComp.Walk(movemntDir);

		ApplyCachedAnimData();

#if final
		// Escape key is always close
		if(Input.GetKeyUp(KeyCode.Escape)) {
			world.SaveGame();
			Application.Quit();
		}
#endif

        // Gravitate any nearby pickups towards us
        if(bAutoPickup) {
            for(int i = nearbyDrops.Count - 1; i >= 0; i--) {
                Vector3 toPlayer = (transform.position - nearbyDrops[i].transform.position).normalized;

                nearbyDrops[i].transform.position += toPlayer * pickupPullSpeed * Time.deltaTime;
                if((transform.position - nearbyDrops[i].transform.position).magnitude < 0.1f) {
                    nearbyDrops[i].PickedUpBy(this);
                    nearbyDrops.RemoveAt(i);
                }
            }
        }

        // Forces the loot bag behind the main sprite
        lootBagSprite.sortingOrder = spriteRenderer.sortingOrder - 1;

        UPDATE_BINDING("PlayerHealth", health / baseHealth);
        UPDATE_BINDING("PlayerEffect", GetMostSevereEffect());

        // update ablilities
        for(int i = 0; i < abilities.Count; i++ ) {
            UPDATE_BINDING("skill" + i, 1.0f - abilities[i].GetCooldownPct());
        }
	}


    private bool PreventMovement() {
        return !bCanMove || (!bCanTalkAndWalk && bTalkingToNPC);
    }


	// Gives a direction to move the camera to, usually in the direction its walking 
	public Vector2 GetLookOffset() {
		float offsetScaler = 3.5f;
		return new Vector3(movementComp.xVel() / offsetScaler, 
		                   movementComp.yVel() / offsetScaler);
	}

	// Updates the player state to the most recent values
	public void CachePlayerState() {
		playerState.health = health;
		playerState.maxHealth = baseHealth;
		playerState.name = "DefaultName";
		playerState.xPos = transform.position.x;
		playerState.yPos = transform.position.y;
	}


    public void WearableItemAquired(string itemName) {
        WearableItem itemType = WorldManager.instance.GetWearableItemByName(itemName);

        // TODO: get image texture from somewhere

        ui.StartItemAquiredSequence(itemType.verboseName, tempTex);
    }


    public void MouseButtonPressed() {
       
    }



    // Base handling for clicking an actor 
    private void SelectedActor(Actor clickedActor) {

    }


    public override float GetDamageBonus() {
        return base.GetDamageBonus();
    }



    public override float GetDamageReduction() {
        return base.GetDamageReduction();
    }

    // Called fromt he world when a damage event occurs, to notify the player
    public void NotifiedDamageEvent(float dmgAmount, Pawn victim, Pawn instigator) {
        if(victim == this) {
             ui.AddKickerNumber(victim, (int)dmgAmount, UI_PlayerBase.KickerType.Aggresive);
        }
        else {
             ui.AddKickerNumber(victim, (int)dmgAmount, UI_PlayerBase.KickerType.Neutral);
        }
    }


    // NOTE: attack type is basically deprecated
    public override void RecieveAttack(float damageAmount, Pawn instigator, BodyPart targetLocation) {
        base.RecieveAttack(damageAmount, instigator, targetLocation);

        // Do camera shake
        myCamera.DoCameraShake(Vector3.up, PlayerCamera.CAMERA_SHAKE_AMPLITUDE);
    }


    private void PlayerUseAbility(int abilityIdx) {
        if(bCarryingLootBag) {
            return;
        }

        if(UseAbility(abilityIdx)) {
            // Do the shake if any
            if(abilities[abilityIdx].cameraShakeFactor > 0.0f) {
                myCamera.DoCameraShake(Vector3.up, abilities[abilityIdx].cameraShakeFactor * PlayerCamera.CAMERA_SHAKE_AMPLITUDE);
            }
        }
    }

    // Try to pick up any item thet player is standing near
    private void PerformInteraction() {
        if(nearbyDrops.Count > 0) {
            if(nearbyDrops.Count > 1) {
                // Need to only pickup the closest one
                int closestIdx = 0;
                float bestDist = 999999.9f;

                for(int i = 0; i < nearbyDrops.Count; i++ ) {
                    float dist = (nearbyDrops[i].transform.position - transform.position).magnitude;
                    if(dist < bestDist) {
                        bestDist = dist;
                        closestIdx = i;
                    }
                }

                nearbyDrops[closestIdx].PickedUpBy(this);
                nearbyDrops.RemoveAt(closestIdx);
            }
            else {
                nearbyDrops[0].PickedUpBy(this);
                nearbyDrops.RemoveAt(0);
            }
        }
        else if(interactableNPC != null && !interactableNPC.IsDead()) {
            // Begin an exchange with this npc
            if(!bTalkingToNPC) {
                interactableNPC.BeginExchangeWithPlayer();
                bTalkingToNPC = true;
                Cursor.visible = true;
            }
        }
    }


    public void EndCurrentExchange() {
        bTalkingToNPC = false;

        Cursor.visible = false;

        if(interactableNPC != null) {
            interactableNPC.EndExchangeWithPlayer();
        }
    }


    public void NPCNearby(NPC nowInRange) {
        if(interactableNPC == null) {
            interactableNPC = nowInRange;
        }
        else if(nowInRange != interactableNPC) {
            // if its a different npc, decide which is closer
            float distCurrent = (interactableNPC.transform.position - transform.position).magnitude;
            float distNew = (nowInRange.transform.position - transform.position).magnitude;

            if(distNew < distCurrent) {
                //Make sure any conversation ends
                if(interactableNPC.IsInConversation() && bTalkingToNPC) {
                    EndCurrentExchange();
                }

                interactableNPC = nowInRange;
            }
        }
    }


    public void NPCOutOfRange(NPC nowOutOfRange) {
        if(interactableNPC == nowOutOfRange) {
            if(bTalkingToNPC && interactableNPC.IsInConversation()) {
                EndCurrentExchange();
            }
            interactableNPC = null;
        }
    }




    public void DropLootBag() {
        lootBagSprite.enabled = false;
        bCarryingLootBag = false;

        // create bag dropped pickup object
        GameObject spawned = WorldManager.instance.SpawnObject("P_LootBag", transform.position);
        if(spawned == null) {
            Debug.LogError("WARNING: Player::DropLootBag --- World manager does not have a dropped loot bag object");
            return;
        }

        DroppedLootBag droppedObject = spawned.GetComponent<DroppedLootBag>();
        if(droppedObject == null) {
            Debug.LogError("WARNING: Player::DropLootBag --- DroppedLootBag object does not have DroppedLootBag component");
            return;
        }

        droppedLootBag = droppedObject;
        droppedObject.InitBag(this, inventory);

        UPDATE_BINDING("LootBagHeld", false);
    }
    

    public void PickUpLootBag() {
        const float pickupRange = 1.0f;

        // Find the loot object
        if(droppedLootBag == null) {
            // The object hasnt been dropped at all
            return;
        }

        float distance = (droppedLootBag.transform.position - transform.position).magnitude;
        if(distance > pickupRange) {
            return;
        }

        // The bag is in range and available, so consume the object
        droppedLootBag.PickedUpBy(this);
        lootBagSprite.enabled = true;
        bCarryingLootBag = true;

        droppedLootBag = null;

        UPDATE_BINDING("LootBagHeld", true);
    }
}
