using UnityEngine;
using System.Collections;

public class SpearController : Throwable {

    [SerializeField]
    private int baseDamage = 100;

    void Start() {
        InitializeActor();
    }

    void Update() {
        UpdateActor();
    }


    protected override void UpdateActor() {
        base.UpdateActor();

    }


    protected override void InitThrowable() {
        base.InitThrowable();

        gravityAcc = 3.0f;
    }


    protected override void EndPhysics() {
        base.EndPhysics();

        gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other) {

        Pawn victim = Ability.IsColliderPawn(other);
        if(victim != null && victim != instigator 
            && (victim.GetType() != instigator.GetType())) { // Don't let spears collide with the same type of thing that threw them
            victim.RecieveAttack(baseDamage, instigator, Pawn.BodyPart.Torso);
            victim.ApplyInstantForce(velocity, 10.0f);
            EndPhysics();
        }
    }
}
