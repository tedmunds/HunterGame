using UnityEngine;
using System.Collections.Generic;

public class DroppedItem : Actor {

	public Item item;
    public Pawn droppedBy;

    protected Vector3 velocity;
    protected float startDropSpeed;
    protected float groundHeight;

    protected const float maxVelocity = 5.0f;
    protected const float gravAccel = 10.0f;

    protected bool bHasLanded;
    protected bool bDoPhysics = true;

    [SerializeField]
	public List<Sprite> itemIcons;

	// Use this for initialization
	void Start () {
		InitializeActor();

		// TODO: set the image based on the item dropped
		//spriteRenderer.sprite = itemIcons[0];
	}

    void OnEnable() {
        
    }
	
	// Update is called once per frame
	void Update () {
		UpdateActor();

        if(bDoPhysics && !bHasLanded) {
            transform.position += velocity * Time.deltaTime;
            velocity.y -= gravAccel * Time.deltaTime;

            if(transform.position.y <= groundHeight) {
                bHasLanded = true;
            }
        }
	}

	// called after the object has been created, and it is being dropped
	public void Drop(Pawn dropper, System.Type itemType) {
        if(!itemType.IsSubclassOf(typeof(Item))) {
            Debug.Log("WARNING! "+dropper.name+" tried to drop a "+itemType.ToString()+". Must extend Item!");
            return;
        }

        bHasLanded = false;
        startDropSpeed = Random.Range(0.0f, maxVelocity);
        velocity = Random.insideUnitCircle * startDropSpeed;
        velocity.y = Mathf.Abs(velocity.y);
        groundHeight = (dropper.transform.position - dropper.GetBaseOffset()).y;

        item = (Item)System.Activator.CreateInstance(itemType);
        droppedBy = dropper;
	}


    // Crafting items are different b/c there is only one class, but any number of values
    public void DropCraftingItem(Pawn dropper, CraftingItem itemToDrop) {
        bHasLanded = false;
        startDropSpeed = Random.Range(0.0f, maxVelocity);
        velocity = Random.insideUnitCircle * startDropSpeed;
        velocity.y = Mathf.Abs(velocity.y);
        groundHeight = (dropper.transform.position - dropper.GetBaseOffset()).y;

        // Craftingitems can be a direct reference b/c they have no functionality, only data that doesn't change
        item = itemToDrop;
        droppedBy = dropper;

        if(item != null) {
            if(spriteRenderer == null) {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            SetSpriteImage();
        }
    }


	/** Attempt to add the itemtype to the item to the users inventory */
	public virtual void PickedUpBy(Pawn user) {
        if(user == null) {
            return;
        }

        // Add the item this drop is storing to the pawns inventory. If its added, then this drop can be removed
        if(user.AddItemToInventory(item)) {
            gameObject.SetActive(false);
        }
	}


	void OnTriggerEnter2D(Collider2D other) {
		Pawn p = other.GetComponent<Pawn>();
		if(p == null) {
			return;
		}

		// let the pawn know that they can pick this up
        p.DropIsNearby(this);
	}

    void OnTriggerExit2D(Collider2D other) {
        Pawn p = other.GetComponent<Pawn>();
        if(p == null) {
            return;
        }

        // Tell the pawn they cannot pickup the item anymore
        p.DopIsOutOfRange(this);
    }


    // Sets up the sprite to use based on item name
    public void SetSpriteImage() {
        string itemName = item.name;
        foreach(Sprite icon in itemIcons) {
            if(icon.name == itemName) {
                spriteRenderer.sprite = icon;
                break;
            }
        }
    }

    public Texture GetItemTexture(string itemName, out Rect textureRect) {
        foreach(Sprite icon in itemIcons) {
            if(icon.name == itemName) {
                textureRect = icon.textureRect;
                return icon.texture;
            }
        }
        textureRect = new Rect(0.0f, 0.0f, 0.0f, 0.0f);
        return null;
    }
}
