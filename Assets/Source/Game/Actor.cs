using UnityEngine;
using System.Collections;


[RequireComponent(typeof(SpriteRenderer))]
public class Actor : MonoBehaviour {

	[SerializeField] // does this actor do anything or is it just sort rendererd?
	private bool bIsStatic;

    [SerializeField] // human readeable name that is game applicable
    public string gameName;

	protected SpriteRenderer spriteRenderer;

	/** Position of the base of this actor */
	protected float baseHeight;

	/** Sprite sorting layer of this actor */
	private int sortLayer;

	void Start() {
		InitializeActor();
	}

	void Update() {
		UpdateActor();
	}

	protected virtual void  InitializeActor() {
		spriteRenderer = GetComponent<SpriteRenderer>();
		if(bIsStatic) {
			SetRenderOrderFromY();
		}
	}
	
	protected virtual void UpdateActor() {
		if(!bIsStatic) {
			SetRenderOrderFromY();
		}
	}


	private void SetRenderOrderFromY()  {
		baseHeight = transform.position.y - GetComponent<Renderer>().bounds.size.y/2.0f;
		
		if(baseHeight < 0.0f) {
			sortLayer = (int)Mathf.Abs(baseHeight*100);
		}
		else {
			sortLayer = 32767 - (int)(baseHeight*100);
		}
		
		spriteRenderer.sortingOrder = sortLayer;
	}

    // Gets the offset vector from actor position to the apparent bottom of the sprite
    public Vector3 GetBaseOffset() {
        return new Vector3(0.0f, GetComponent<Renderer>().bounds.size.y/2.0f, 0.0f);
    }


    /** Creates an instance of the input particle system object, and attaches it to this actors transform as a child */
    public ParticleSystem AddEffectAttached(ParticleSystem prototype, Vector3 localOffset, bool activateOnAdd = false) {
        if(prototype == null) {
            Debug.Log("WARNING! Tried to attach a null system: " + prototype.name);
            return null;
        }

        ParticleSystem addedSystem = (ParticleSystem)Instantiate(prototype);
        addedSystem.transform.parent = transform;
        addedSystem.transform.localPosition = localOffset;
        if(!activateOnAdd) 
        {
            addedSystem.Stop();
        }

        return addedSystem;
    }

    /**
     * Deactivates the actor gameobject after a certain amount of time
     */
    public void DeactivateDelayed(float delay) {
        StartCoroutine(Co_DeactivateDelayed(delay));
    }

    private IEnumerator Co_DeactivateDelayed(float delay) {
        for(float initiated = Time.time; Time.time - initiated < delay; ) {
            yield return null;
        }

        gameObject.SetActive(false);
    }


    protected void UPDATE_BINDING(string varName, object newValue) {
        if(WorldManager.instance != null) {
            WorldManager.instance.GetUIManager().ValueUpdated(varName, newValue);
        }
    }
}
