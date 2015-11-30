using UnityEngine;
using System.Collections;

public class Explosion : MonoBehaviour {

    [SerializeField]
    private float explosionRadius;

    [SerializeField]
    private int baseDamage;

    [SerializeField]
    private float explosiveForce;


    // Explode and damage all nearby pawns
    public void Explode(Pawn instigator) {
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        for(int i = 0; i < overlaps.Length; i++) {
            Pawn victim = overlaps[i].GetComponent<Pawn>();
            if(victim != null) {
                victim.RecieveAttack(baseDamage, instigator, Pawn.BodyPart.Torso);

                victim.ApplyInstantForce(victim.transform.position - transform.position, explosiveForce);
            }
        }

        gameObject.SetActive(false);
    }
}
