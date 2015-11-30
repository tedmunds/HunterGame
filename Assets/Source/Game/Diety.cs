using UnityEngine;
using System.Collections.Generic;

public class Diety {

    /** The absalute maximum change in player relation that can occur in a single sacrifice event */
    private const float GLOBAL_MAX_DELTA_REACTION = 0.3f;

    /** Difference levels of preference */
    public enum DietyPrefefernceLevels {
        hates, dislikes, ambivalent, likes, adores
    }

    /** States the dieties feelings towards a single sacrifice type: ie. CraftingItem */
    public struct DietyTrait {
        public string sacrificeType;
        public bool bAppreciates;
        public string qualifierText;
    }

    /** Procedurally generated name: composed of <prefix> <title> <suffix> : ex. Beautiful Hamerrax of the Undying */
    public string dietyName;

    public string namePrefix;
    public string nameTitle;
    public string nameSuffix;

    // A mapping of all crafting item names to a value corresponding to how much this diety 'like' or dislikes that item
    public Dictionary<string, float> itemPreferences; 

    // This dieties feelings towards the player: ranges from -1 to 1
    public float playerRelation;

    private static bool bDebugDietyStats = true;


    public Diety(string pref, string title, string suff) {
        namePrefix = pref;
        nameTitle = title;
        nameSuffix = suff;

        dietyName = namePrefix  + nameTitle + nameSuffix;
        
        // Always start at 0 relation
        playerRelation = 0.0f;

        string debugPrefs = "";

        // Add all of the items to the map with randomvalues TODO: decide a more algorithmic way of deciding these preferences
        itemPreferences = new Dictionary<string, float>();
        foreach(CraftingItem item in WorldManager.instance.GetAllCraftingItems()) {
            float itemPreference = Random.Range(-1.0f, 1.0f);
            itemPreferences.Add(item.craftingItemName, itemPreference);

            if(bDebugDietyStats) {
                debugPrefs += GetReactionToItem(item.craftingItemName) + item.craftingItemName + ", ";
            }
        }

        if(bDebugDietyStats) {
            Debug.Log(dietyName);
            Debug.Log(debugPrefs);
        }
    }


    /** 
     * Called whenever the player makes a sacrifice to the gods: this god will then decide how it wants to react
     * to this type of sacrifice, based on their preferences
     */ 
    public void SacrificeMade(string sacrificeItemName) {
        // Get the actual item being sacrificed, we need it's use function
        CraftingItem sacrificeItem = WorldManager.instance.GetCraftingItemByName(sacrificeItemName);
        if(sacrificeItem == null) {
            Debug.Log("WARNING! Diety::SacrificeMade() Could not find the CraftingItem with the name: " + sacrificeItemName);
            return;
        }

        // Perfrom the function of the crafting item
        sacrificeItem.ActivateUse(WorldManager.instance.GetActivePlayer());

        // Adjust the gods feelings towards the player based on the item sacrifised and its preferences
        float prefVal = GetValueForItem(sacrificeItemName);
        float prefMag = Mathf.Abs(prefVal);
        float prefSign = Mathf.Sign(prefVal);

        // The dieties relation to the player will react more at the smaller ends of the spectrum
        float deltaRelation = 0.0f;

        // the larger the magnitude of the relation, the smaller the change will be so that a larger relation is harder to change
        float reactionMag = 1.0f - Mathf.Abs(playerRelation);
        float maxDeltaReaction = prefMag * GLOBAL_MAX_DELTA_REACTION;

        deltaRelation = (maxDeltaReaction * reactionMag) * prefSign;

        playerRelation += deltaRelation;

        Debug.Log(sacrificeItemName + " for " + dietyName + " :: " + playerRelation + " --- Delta = " + deltaRelation);
    }



    public float GetValueForItem(CraftingItem item) {
        return GetValueForItem(item.craftingItemName);
    }

    public float GetValueForItem(string itemName) {
        float preferenceVal = 0.0f;
        itemPreferences.TryGetValue(itemName, out preferenceVal);
        
        return preferenceVal;
    }



    public string GetReactionToItem(CraftingItem item) {
        return GetReactionToItem(item.craftingItemName);
    }

    public string GetReactionToItem(string itemName) {
        float prefVal;

        if(itemPreferences.TryGetValue(itemName, out prefVal)) {
            DietyPrefefernceLevels reaction = PreferenceLevelToReaction(prefVal);

            string reactionString = reaction.ToString() + " ";

            // use slightly different text for ambivalent
            if(reaction == DietyPrefefernceLevels.ambivalent) {
                reactionString = "is " + reactionString + "towards "; // is ambivalent towards
            }

            return reactionString;
        }

        return "has no feelings for ";
    }


    // Translate the items preference value to a discrete value
    private DietyPrefefernceLevels PreferenceLevelToReaction(float prefValue) {
        float preferencePct = (prefValue + 1.0f) / 2.0f; // translate from (-1 to 1) to (0 to 1)
        int prefLevelVal = (int)(5.0f * preferencePct);

        return (DietyPrefefernceLevels)prefLevelVal;
    }
}
