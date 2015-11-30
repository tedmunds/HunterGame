using UnityEngine;
using System.Collections;

public class Item_MonsterBait : Item {


    public Item_MonsterBait() {
        name = "Monster Bait";
        recipe = new RecipeRequirement[1];
        recipe[0] = new RecipeRequirement("RottingFlesh", 3);
    }
}
