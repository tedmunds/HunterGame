using UnityEngine;
using System.Collections;


// Type of physicsy object that can be tossed a specified distance by a pawn
public class Throwable : Actor {

    protected float gravityAcc = 9.8f;

    [SerializeField]
    protected bool bDoBounce = false;

    [SerializeField]
    protected float bounceFactor = 1.0f;

    [SerializeField]
    protected float bounceFriction = 0.0f;

    /** how many bounces at most */
    [SerializeField]
    protected int maxBoucnes = 1;

    protected Pawn instigator;

    protected Vector3 velocity;

    /** Y height that will be considered the ground level */
    protected float groundY;

    /** Shadow transform should alsways be at the ground height */
    protected Transform shadowObj;
    protected float shadowheight;

    protected bool bIsPhysics = false;

    protected int numBounces;

    /** Time at which this was thrown */
    protected float throwTime;

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }

	protected override void UpdateActor () {
        base.UpdateActor();

        if(bIsPhysics) {
            transform.position += velocity * Time.deltaTime;
            velocity.y -= gravityAcc * Time.deltaTime;

            Vector3 bottomPoint = transform.position - GetBaseOffset();

            // If there is a shadow it should follow a steight ling along the ground
            if(shadowObj != null) {
                shadowObj.position = new Vector3(bottomPoint.x, groundY - shadowheight/2.0f, 0.0f);
            }

            if(bottomPoint.y <= groundY) {
                transform.position = new Vector3(transform.position.x, groundY + GetBaseOffset().y, 0.0f);

                if(bDoBounce) {
                    numBounces += 1;

                    velocity.y = -velocity.y * bounceFactor;
                    velocity.x = velocity.x * (1.0f - bounceFriction);

                    if(numBounces >= maxBoucnes) {
                        EndPhysics();
                    }
                }
                else {
                    EndPhysics();
                }
            }
        }
	}

    // Method to call to intiate the movement
    public void Throw(Pawn thrower, float horzSpeed, float vertSpeed, Vector3 direction) {
        InitThrowable();

        instigator = thrower;
        groundY = thrower.GetFootPosition().y;

        velocity = new Vector3(horzSpeed * Mathf.Sign(direction.x), vertSpeed, 0.0f);
        bIsPhysics = true;
        numBounces = 0;

        // Set mirroring: assumes base if facing right
        if(direction.x < 0.0f) {
            transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
        }
        else {
            transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
        }

        throwTime = Time.time;
    }


    protected virtual void InitThrowable() {
        if(transform.childCount > 0) {
            shadowObj = transform.GetChild(0);
            SpriteRenderer shadowRenderer = shadowObj.GetComponent<SpriteRenderer>();
            if(shadowRenderer != null) {
                shadowheight = shadowRenderer.bounds.size.y;
            }
        }
        else {
            Debug.Log("WARNING: Throwable:"+name+" has no shadow object child!");
        }
    }

    // Stop the movement
    protected virtual void EndPhysics() {
        velocity = Vector3.zero;
        bIsPhysics = false;
    }
}
