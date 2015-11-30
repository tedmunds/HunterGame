using UnityEngine;
using System.Collections;

public class FireBallController : Throwable {

    [SerializeField]
    private int baseDamage = 0;

    [SerializeField]
    private bool bApplyFire = true;

    [SerializeField]
    private float baseBurningTime = 1.0f;

    [SerializeField]
    private float impactForce = 10.0f;

    [SerializeField]
    private float maxFlightTime = 3.0f;

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }


    protected override void UpdateActor() {
        base.UpdateActor();

        if(Time.time - throwTime >= maxFlightTime) {
            EndPhysics();
        }
    }


    protected override void InitThrowable() {
        base.InitThrowable();

        gravityAcc = 0.0f;
    }


    protected override void EndPhysics() {
        base.EndPhysics();

        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other) {

        Pawn victim = Ability.IsColliderPawn(other);
        if(victim != null && victim != instigator
            && (victim.GetType() != instigator.GetType())) { // Don't let fireballs collide with the same type of thing that threw them
            victim.RecieveAttack(baseDamage, instigator, Pawn.BodyPart.Torso);

            if(bApplyFire) {
                victim.AddBurning(baseBurningTime);
            }

            victim.ApplyInstantForce(velocity, impactForce);
            EndPhysics();
        }
    }
}
