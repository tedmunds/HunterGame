using UnityEngine;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

/**
 * This utility is the basic tamplate for any class that will load a list of variations on some content. 
 * In this case, it is loading the set of crafting items, whose differences are only different values 
 * for the general set of crafting item perameters. 
 */ 
[XmlRoot("ItemUtility")]
public class ItemUtility {

    [XmlArray("CraftingItems")]
    [XmlArrayItem("CraftingItem")]
    public CraftingItem[] craftingItems;

    [XmlArray("WearableItems")]
    [XmlArrayItem("WearableItem")]
    public WearableItem[] wearableItems;


    public static ItemUtility LoadCraftingItems(string sourceFile) {
        XmlSerializer serializer = new XmlSerializer(typeof(ItemUtility));

        string filePath = Path.Combine(Application.dataPath, "Data/" + sourceFile + ".xml");

        try {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            ItemUtility itemUtility = serializer.Deserialize(stream) as ItemUtility;
            for(int i = 0; i < itemUtility.craftingItems.Length; i++ ) {
                itemUtility.craftingItems[i].name = itemUtility.craftingItems[i].craftingItemName;
            }

            return itemUtility;
        }
        catch(IOException e) {
            Debug.Log("ERROR! could find crafting item source file " + filePath + " :: " + e.Message);
            return null;
        }
    }



    public static ItemUtility LoadWearableItems(string sourceFile) {
        XmlSerializer serializer = new XmlSerializer(typeof(ItemUtility));

        string filePath = Path.Combine(Application.dataPath, "Data/" + sourceFile + ".xml");

        try {
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            ItemUtility itemUtility = serializer.Deserialize(stream) as ItemUtility;
            for(int i = 0; i < itemUtility.wearableItems.Length; i++) {
                itemUtility.wearableItems[i].name = itemUtility.wearableItems[i].wearableItemName;
            }

            return itemUtility;
        }
        catch(IOException e) {
            Debug.Log("ERROR! could find wearable item source file " + filePath + " :: " + e.Message);
            return null;
        }
    }

}
