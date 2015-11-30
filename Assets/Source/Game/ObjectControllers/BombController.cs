using UnityEngine;
using System.Collections;

public class BombController : Throwable {

    [SerializeField]
    private float explodeDelay;

    [SerializeField]
    private float explosionRadius;

    [SerializeField]
    private int baseDamage;

    [SerializeField]
    private float explosiveForce;

    /** What time did the countdown start at */
    private float startedCountdownTime;

    /** When did the comb explode */
    private float explodedTime;

    private bool bHasBegunCoutdown;
    private bool bHasExploded;

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }


    protected override void UpdateActor() {
        base.UpdateActor();

        if(bHasBegunCoutdown && !bHasExploded) {
            float timeSinceCDStarted = Time.time - startedCountdownTime;
            if(timeSinceCDStarted >= explodeDelay) {
                SpawnExplosion();
            }
        }
    }


    protected override void InitThrowable() {
        base.InitThrowable();

    }


    protected override void EndPhysics() {
        base.EndPhysics();

        // Start bomb timer when it comes to a rest
        bHasBegunCoutdown = true;
        bHasExploded = false;

        startedCountdownTime = Time.time;
    }


    public void SpawnExplosion() {
        GameObject exploObj = instigator.GetWorld().SpawnObject("P_ExplosionMedium", transform.position);
        if(exploObj != null) {
            Explosion explosion = exploObj.GetComponent<Explosion>();
            if(explosion != null) {
                explosion.Explode(instigator);
            }
        }

        bHasExploded = true;
        gameObject.SetActive(false);
    }
}
