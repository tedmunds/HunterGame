using UnityEngine;
using System.Collections;

[System.Serializable]
public class UI_SacrificeMenu : UI_Menu {

    private string selectedItem;

    //[SerializeField]
    //private UI_DietyMenu dietySelectionMenu;

    public virtual void Init(UI_PlayerBase creator) {
        base.Init(creator, "Make a Sacrifice:");

        float w = 500.0f;
        float h = 400.0f;

        panel = new Rect(Screen.width / 2.0f - w / 2.0f, Screen.height / 2.0f - h / 2.0f, w, h);

        childMenu = new UI_DietyMenu();
        childMenu.panelBackground = panelBackground;
        childMenu.Init(creator, "Select which Diety to sacrifice to", this);
    }




    public override void CloseMenu() {
        base.CloseMenu();

        selectedItem = "";
    }

    public override void OpenMenu() {
        base.OpenMenu();

        // Build the options based on what the player currently has in their inventory
        options.Clear();

        Player player = UI_base.GetOwner();
        Inventory inv = player.GetInventory();
        if(inv == null) {
            Debug.Log("ERROR! UI_SacrificeMenu::OpenMenu() Failed to find player inventory!");
            return;
        }

        // Create an option button for each craftin item
        foreach(string item in inv.CraftingItems()) {
            int numOfItem = inv.GetNumOfCraftingItem(item);
            MenuButton btn = new MenuButton(item, SacrificeSelected);

            string itemEffect = WorldManager.instance.GetCraftingItemByName(item).effectText;

            btn.info = "x " + numOfItem + " : " + itemEffect;
            options.Add(btn);
        }
    }


    public void SacrificeSelected(MenuButton pressed) {
        selectedItem = pressed.text;

        UI_DietyMenu dietySubMenu = (UI_DietyMenu)childMenu;

        // Sacrifice has been selected, not the player choosees which god to sacrfice to
        if(dietySubMenu != null) {
            dietySubMenu.SetSacrificeItem(selectedItem);
            dietySubMenu.OpenMenu();
        }

        CloseMenu();
    }

}