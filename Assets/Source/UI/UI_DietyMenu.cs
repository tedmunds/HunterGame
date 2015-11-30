using UnityEngine;
using System.Collections;

public class UI_DietyMenu : UI_Menu {

    // What is the sacrificial item in question
    private string itemName;

    public void SetSacrificeItem(string item) { itemName = item; }

    public override void Init(UI_PlayerBase creator, string title, UI_Menu instigator = null) {
        base.Init(creator, title, instigator);

        float w = 700.0f;
        float h = 400.0f;

        buttonWidth = 350.0f;

        panel = new Rect(Screen.width / 2.0f - w / 2.0f, Screen.height / 2.0f - h / 2.0f, w, h);
    }


    public override void OpenMenu() {
        base.OpenMenu();

        // Build the options based on what the player currently has in their inventory
        options.Clear();

        Diety[] dieties = WorldManager.instance.GetDieties();
        for(int i = 0; i < dieties.Length; i++) {
            MenuButton btn = new MenuButton(dieties[i].dietyName, DietySelected);

            // Add some info about how it feels towards the item
            string reaction = dieties[i].GetReactionToItem(itemName);
            btn.info = reaction + itemName;

            options.Add(btn);
        }
    }



    public void DietySelected(MenuButton pressed) {
        // Find the diety with the name stored in button text, make the sacrifice to them
        Diety[] dieties = WorldManager.instance.GetDieties();
        int selectedIdx = -1;
        for(int i = 0; i < dieties.Length; i++) {
            if(dieties[i].dietyName == pressed.text) {
                selectedIdx = i;
                break;
            }
        }

        // Found the diety, can make a sacrifice to them
        if(selectedIdx >= 0) {
            Inventory inventory = UI_base.GetOwner().GetInventory();
            if(inventory == null) {
                Debug.Log("ERROR! UI_DietyMenu::DietySelected() Failed to find player inventory!");
                return;
            }

            // Only make the actuall sacrifice if the item was actually found and removed from the inventory
            if(inventory.RemoveCraftingItem(itemName)) {
                dieties[selectedIdx].SacrificeMade(itemName);
            }
        }

        CloseMenu();
    }

}