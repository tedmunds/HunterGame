using UnityEngine;
using System.Collections;

[System.Serializable]
public class UI_TradeMenu : UI_Menu {

    // Which npc is being traded with
    private NPC trader;

    public void SetTrader(NPC trader) { this.trader = trader; }


    public override void Init(UI_PlayerBase creator, string title, UI_Menu instigator = null) {
        base.Init(creator, title, instigator);

        float w = 700.0f;
        float h = 400.0f;

        buttonWidth = 150.0f;

        panel = new Rect(Screen.width / 2.0f - w / 2.0f, Screen.height / 2.0f - h / 2.0f, w, h);
    }




    public override void OpenMenu() {
        if(trader == null) {
            Debug.Log("WARNING: UI_TradeMenu was opened without a trader being set!");
            return;
        }
        
        base.OpenMenu();

        // Build the options based on what the trader is willing to accept
        options.Clear();

        NPC.TradeOption[] tradeOptions = trader.GetTradeOptions();
        for(int i = 0; i < tradeOptions.Length; i++) {
            MenuButton btn = new MenuButton(tradeOptions[i].itemName, TradeSelected);

            btn.info = ": trade " + tradeOptions[i].numDesired + " for 1 " + tradeOptions[i].returnItem;
            options.Add(btn);
        }
    }



    public override void CloseMenu() {
        base.CloseMenu();
        trader = null;
    }


    // Totally overried draing of window so the buttons can have pictures
    public override void DrawGUI() {
        if(childMenu != null) {
            childMenu.DrawGUI();
        }

        if(!bIsOpen) {
            return;
        }

        GUI.BeginGroup(panel);

        GUI.DrawTexture(new Rect(0.0f, 0.0f, panel.width, panel.height), panelBackground);

        GUI.Label(new Rect(10.0f, 15.0f, panel.width, panel.height), panelName);

        Rect buttonRect = new Rect(10.0f, 35.0f, buttonWidth, 25.0f);
        const float buttonOffset = 5.0f;

        MenuButton selectedButton = new MenuButton("", null);
        bool bMadeSelection = false;

        foreach(MenuButton buttonOption in options) {
            // Cache which button was pressed, because the handler may modiy the options list
            if(GUI.Button(buttonRect, buttonOption.text)) {
                selectedButton = buttonOption;
                bMadeSelection = true;
                break;
            }

            // Draw the description of the button next to it
            GUI.Label(new Rect(buttonRect.x + buttonRect.width + 25.0f, buttonRect.y, panel.width, panel.height), buttonOption.info);

            buttonRect.y += buttonRect.height + buttonOffset;
        }

        // If a selection is made, call the handler
        if(bMadeSelection) {
            selectedButton.handler(selectedButton);
        }

        GUI.EndGroup();
    }



    public void TradeSelected(MenuButton pressed) {
        Inventory inventory = UI_base.GetOwner().GetInventory();
        if(inventory == null) {
            Debug.Log("ERROR! UI_TradeMenu::TradeSelected() Failed to find player inventory!");
            return;
        }
        string itemName = pressed.text;

        // get the trade option that was selected
        NPC.TradeOption tradeOption;
        if(trader.GetTradeOptionForItem(itemName, out tradeOption)) {
            // check that the inventory actually contains enough of the crafting item before removing them
            if(inventory.GetNumOfCraftingItem(itemName) >= tradeOption.numDesired) {
                inventory.RemoveCraftingItem(itemName, tradeOption.numDesired);

                // Give player the item
                UI_base.GetOwner().WearableItemAquired(tradeOption.returnItem);

                // only in this case, of a successful trade, does the menu close
                CloseMenu();
            }
        }
    }


}