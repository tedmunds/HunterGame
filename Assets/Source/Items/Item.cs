using UnityEngine;
using System.Collections;

public class Item  {

    /** Is this item suppoed to be effecting the pawn that owns it */
    public bool bIsActive;

    public struct RecipeRequirement {
        public RecipeRequirement(string s, int n) {
            craftingItemName = s;
            numRequired = n;
        }
        public string craftingItemName;
        public int numRequired;
    }

    /** relevant game name */
	public string name;

    /** Item all can be crafted from recipes (if this array has anything in it) */
    public RecipeRequirement[] recipe;


    public Item() {
        bIsActive = false;
    }


    // Called from pawn when the item begins effecting the pawn that has it
    public virtual void BeginEffect(Pawn owner) {
        bIsActive = true;
    }


    // Called every frame by the pawn that owns this item: Only when it is active ie. effecting the pawn
    public virtual void UpdateEffect(Pawn owner) {

    }


    // Called when this item stops effecting the pawn that owns it
    public virtual void EndEffect(Pawn owner) {
        bIsActive = false;
    }
}
