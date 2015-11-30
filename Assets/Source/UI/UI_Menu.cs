using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public abstract class UI_Menu {

    // Delegate for button handlers: called when the button is pressed, and passees in the index of the button int eh menu list
    public delegate void ButtonHandler(MenuButton pressed);

    // Callback when a childmenu is closed
    public delegate void ChildClosedHandler();

    // Data for a single Button
    public struct MenuButton {
        public string text;
        public ButtonHandler handler;

        // Optional info about what the botton does
        public string info;

        public MenuButton(string t, ButtonHandler h) {
            text = t;
            handler = h;
            info = "";
        }
    }

    // Main background texture of the menu panel
    public Texture panelBackground;

    // Panel location on screen
    protected Rect panel;

    protected float buttonWidth = 100.0f;

    protected string panelName;

    // Is the menu currently open
    protected bool bIsOpen;

    // All button options that will be rendered and pressable
    protected List<MenuButton> options;

    // UI that spawned the menu
    protected UI_PlayerBase UI_base;

    // Menu that caused this menu to be opened: will be null if it was opened through other means
    protected UI_Menu instigator;
    protected UI_Menu childMenu;
    protected ChildClosedHandler childClosedHandler;

    public bool IsOpen() { return bIsOpen; }

    public virtual void Init(UI_PlayerBase creator, string title, UI_Menu instigator = null) {
        options = new List<MenuButton>();
        UI_base = creator;
        panelName = title;
        this.instigator = instigator;

        childClosedHandler = ChildMenuClosed;
    }

    public virtual void OnUpdate() {
        if(childMenu != null) {
            childMenu.OnUpdate();
        }

        if(!bIsOpen) {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape)) {
            CloseMenu();
        }
    }


    public virtual void DrawGUI() {
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


    public virtual void OpenMenu() {
        bIsOpen = true;
    }


    public virtual void CloseMenu() {
        bIsOpen = false;

        if(instigator != null) {
            instigator.childClosedHandler();
        }
    }


    public virtual void ChildMenuClosed() {
        // re-open this menu
        OpenMenu();
    }
}