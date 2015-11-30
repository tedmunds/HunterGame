using UnityEngine;
using System.Collections.Generic;

public class Inventory {

    public int maxCapacity;

    public const int MAX_ITEMS = 99;

    private List<Item> itemList;

    /** Map simply tracks how many of each type of item owner has */
    private Dictionary<string, int> craftingItems;


    public Inventory(int size) {
        maxCapacity = size;
        itemList = new List<Item>(maxCapacity);
        craftingItems = new Dictionary<string, int>();
    }


    // List of the crafting items in the inventory
    public System.Collections.IEnumerable CraftingItems() {
        foreach(string item in craftingItems.Keys) {
            yield return item;
        }
    }


    public bool AddCraftingItem(CraftingItem craftingItem) {
        // If the map doesn't contain a key for this item yet, a new entry will have to be created
        int currentAmout = 0;
        
        if(craftingItems.TryGetValue(craftingItem.craftingItemName, out currentAmout)) {
            int newAmout = Mathf.Min(currentAmout + 1, MAX_ITEMS);

            craftingItems[craftingItem.craftingItemName] = newAmout;
        }
        else {
            craftingItems.Add(craftingItem.craftingItemName, 1);
        }
        
        return true;
    }


    public bool AddItem(Item item) {
        if(itemList.Count >= maxCapacity) {
            return false;
        }

        itemList.Add(item);
        return true;
    }


    public IEnumerable<Item> Items() {
        for(int i = 0; i < itemList.Count; i++ ) {
            yield return itemList[i];
        }
    }

    public bool RemoveItem(Item item) {
        if(itemList.Contains(item)) {
            itemList.Remove(item);
            return true;
        }

        return false;
    }

    public bool RemoveCraftingItem(string craftingItem, int numToRemove = 1) {
        int amount = -1;

        // check taht this type of item is in the inventory
        if(craftingItems.TryGetValue(craftingItem, out amount)) {
            int newAmount = Mathf.Max(0, amount - numToRemove);
            craftingItems[craftingItem] = newAmount;

            // there is none left, remove it from the crafting items list as well
            if(newAmount == 0) {
                craftingItems.Remove(craftingItem);
            }

            return true;
        }

        return false;
    }

    // How many of the specific type of item does this inventory contain
    public int GetNumOfCraftingItem(string itemName) {
        int num;
        if(craftingItems.TryGetValue(itemName, out num)) {
            return num;
        }

        return 0;
    }

}
