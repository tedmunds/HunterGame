using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Player))]
public class UI_PlayerBase : MonoBehaviour {

    // Kicker category determines the color of the kicker text
    public enum KickerType {
        Friendly, Neutral, Aggresive
    }

    private static Color friendlyColor = new Color(50, 50, 255);
    private static Color neutralColor = new Color(255, 255, 0);
    private static Color aggresiveColor = new Color(255, 0, 0);

    // Data for a single kicker event
    protected class KickerEvent {
        public Color col;       // what color is it
        public string text;     // what text is it
        public float lifeTime;  // how long until it is removed
        public Vector3 loc;     // world location
        public Vector2 screenLoc; // Screen location
    }

    protected struct ItemAquiredPopupInfo {
        public Texture image;
        public string name;
        public float activatedTime;
    }

    // constant section
    private const float KICKER_LIFESPAN = 1.0f;
    private const float KICKER_SPEED = 3.0f;
    private const float KICKER_VERT_OFFSET = 0.5f;

    private const float skillBarWidth = 200.0f;
    private const float skillBarHeight = 50.0f;

    private static Vector3 floating_HealthBar_Offset = new Vector3(0.0f, 0.2f, 0.0f);
    private const float floating_HealthBar_Width = 40.0f;

    [SerializeField]
    private GameObject selectRingPrototype;


    [SerializeField]
    private Texture healthBarTexture;

    [SerializeField]
    private Texture healthBarTexture_bleed;

    [SerializeField]
    private Texture healthBarTexture_poison;

    [SerializeField]
    private Texture healthBarTexture_burn;


    [SerializeField]
    private Texture healthBarBackingTexture;

    [SerializeField]
    private Texture abilityCooldownTexture;

    [SerializeField]
    private Texture abilityLockedTexture;

    [SerializeField]
    private Font baseFont;

    // Menu for making sacrifices
    [SerializeField]
    public UI_SacrificeMenu sacrificeMenu;

    // Menu for trading with NPC
    [SerializeField]
    public UI_TradeMenu tradingMenu;

    [SerializeField] // this is just so the ui can know about the textures
    public DroppedItem droppedItemPrototype;

    private GUIStyle mainStyle;

    private Player playerOwner;
    private Camera playerCamera;
    private Ability[] playerAbilities;
    private Inventory playerInv;

    /** The skill bar is where the player health and abilities are drawn */
    private Rect skillBarRect;
    private Rect healthBarRect;

    /** Popup for dropped item pickup */
    private bool bDrawPickupPrompt;
    private const string pickupPromtText = "[E] Pickup ";

    /** Popup promt for interacting with npc */
    private bool bDrawNPCInteractPrompt;
    private const string npcInteractBaseText = "[E] Talk to ";
    private const string camprFireInteractionBaseText = "[E] Use ";
    private Rect interactionPromtRect;
    private bool bInteractionWindowOpen;

    /** Title text */
    private bool bDrawTitleText;
    private string titleText;
    private float titleTextInitiatedTime;
    private float titleTextLingerTime;
    private float titleTextFadeTime;

    // Were to draw on screen promts
    private Rect promtRect;

    // Data about the popup when an item is aquired
    ItemAquiredPopupInfo itemAquiredPopupInfo;
    bool bDrawItemPopup;

    // Kicker system vars
    private List<KickerEvent> kickers;

    public Player GetOwner() { return playerOwner; }

	void Start () {
        mainStyle = new GUIStyle();
        if(baseFont != null) {
            mainStyle.font = baseFont;
        }
        
        kickers = new List<KickerEvent>();

        skillBarRect = new Rect(Screen.width / 2.0f - skillBarWidth / 2.0f, 
                                Screen.height - skillBarHeight - 15.0f, 
                                skillBarWidth, skillBarHeight);

        healthBarRect = new Rect(skillBarRect.x, skillBarRect.y - 15.0f, skillBarRect.width, 7.0f);

        promtRect = new Rect(Screen.width / 2.0f, Screen.height / 2.0f - 50.0f, 250.0f, 25.0f);

        // This rect is updated to hover above the npc in question
        interactionPromtRect = new Rect(0.0f, 0.0f, 250.0f, 25.0f);
	}

    public void InitializeUI() {
        playerOwner = GetComponent<Player>();
        playerCamera = playerOwner.GetCamera();

        playerAbilities = playerOwner.GetAbilitySet();

        playerInv = playerOwner.GetInventory();

        DoFullScreenTitleText("BEGIN THE PILGRIMAGE", 1.0f, 5.0f);

        sacrificeMenu.Init(this);
        tradingMenu.Init(this, "Trade ");
    }
	
	// Update should update the data model
	void Update () {
        // Update all kickers, reverse iteration so that they can be removed
        for(int i = kickers.Count - 1; i >= 0; i-- ) {
            // Velocity magnitude gets lower as the kicker approaches end of its life time
            float velMag = kickers[i].lifeTime / KICKER_LIFESPAN;

            kickers[i].loc.y += velMag * KICKER_SPEED * Time.deltaTime;

            // We keep the kicker moving in world space so that it doesnt float wierdly with the screen
            kickers[i].screenLoc = playerCamera.WorldToScreenPoint(kickers[i].loc);
            kickers[i].screenLoc.y = Screen.height - kickers[i].screenLoc.y;

            kickers[i].lifeTime -= Time.deltaTime;
            if(kickers[i].lifeTime <= 0.0f) {
                kickers.RemoveAt(i);
            }
        }

        // Check if the player has any pickups in range
        if(playerOwner.GetNearbyDrops().Count > 0 && !Player.bAutoPickup) {
            bDrawPickupPrompt = true;
        }
        else {
            bDrawPickupPrompt = false;
        }

        // Check if player has any npc to talk to: pickups take precedence over talking, b/c you can get them out of the way easily
        if(playerOwner.GetInteractableNPC() != null && !bDrawPickupPrompt) {
            bDrawNPCInteractPrompt = true;

            // Figure out where to draw promt
            NPC playerTarget = playerOwner.GetInteractableNPC();
            Vector3 worldPos = playerTarget.transform.position + playerTarget.GetBaseOffset() + Vector3.up * 0.5f - Vector3.right * 0.3f;

            Vector3 npcScreenLoc = playerCamera.WorldToScreenPoint(worldPos);
            npcScreenLoc.y = Screen.height - npcScreenLoc.y;

            interactionPromtRect.x = npcScreenLoc.x;
            interactionPromtRect.y = npcScreenLoc.y;
        }
        else {
            bDrawNPCInteractPrompt = false;
        }

        if(playerOwner.IsTalkingToNPC()) {
            bInteractionWindowOpen = true;
        }
        else {
            bInteractionWindowOpen = false;
        }

        // Check if title text should be drawn
        if(bDrawTitleText && Time.time - titleTextInitiatedTime > titleTextLingerTime + titleTextFadeTime) {
            bDrawTitleText = false;
        }

        // Update the menu so that it can consume inputs etc.
        sacrificeMenu.OnUpdate();
        tradingMenu.OnUpdate();
	}


    // GUI should never update model, just reference data 
    void OnGUI() {
        //DrawAbilityBar();

        DrawKickers();

        // Draw enemy health bars
        foreach(Pawn p in playerOwner.GetWorld().VisiblePawns()) {
            if(p == playerOwner || !p.HealthIsRelevant()) {
                continue;
            }

            float barWidth = floating_HealthBar_Width * p.GetHealthPercent(); 

            Vector3 worldPos = p.transform.position + p.GetBaseOffset() + floating_HealthBar_Offset;
            Vector3 screenPos = playerCamera.WorldToScreenPoint(worldPos);
            Rect hpRect = new Rect(screenPos.x - floating_HealthBar_Width/2.0f, Screen.height - screenPos.y, barWidth, 5.0f);

            GUI.DrawTexture(hpRect, healthBarTexture);
        }

        // Pickup promt: always goes in the middle of the screen and slightly up and right 
        if(bDrawPickupPrompt && playerOwner.GetNearbyDrops().Count > 0) {
            GUI.Label(promtRect, pickupPromtText + playerOwner.GetNearbyDrops()[0].item.name, mainStyle);
        }

        // Draw the player to npc interaction system
        if(bInteractionWindowOpen) {
            DrawInteractionWindow();
        }
        else if(bDrawNPCInteractPrompt && playerOwner.GetInteractableNPC() != null) {
            string talkText = "";

            // Special case for the camp fire
            if(playerOwner.GetInteractableNPC().GetType() == typeof(CampFireController)) {
                talkText = camprFireInteractionBaseText + playerOwner.GetInteractableNPC().gameName;
            }
            else {
                talkText = npcInteractBaseText + playerOwner.GetInteractableNPC().gameName;
            }

            mainStyle.fontSize = 12;
            GUI.Label(interactionPromtRect, talkText, mainStyle);
        }

        // Always very last thing: Big dramtic title text
        if(bDrawTitleText) {
            DrawTitleText();
        }

        if(bDrawItemPopup) {
            DrawItemAquired();
        }

        // render the other main windows: they handle when to actual draw themselves (ie. if they are open)
        sacrificeMenu.DrawGUI();
        tradingMenu.DrawGUI();
    }


    //private void DrawAbilityBar() {
    //    // Ability bar placement: TEMPORARY, how should this be done?
    //    const float abilityIconSpacing = 7.0f;
    //    const float abilityIconWidth = 50.0f;

    //    // there are 2 extra slots for the camp fire ability (common to all) and the main attack skill
    //    float abilityBarWidth = (abilityIconWidth + abilityIconSpacing) * (Player.NUM_ABILTIES + 2) - abilityIconSpacing;
    //    float abilityXOrigin = Screen.width / 2.0f - abilityBarWidth / 2.0f;

    //    // Draw the attack ability (on the A key by default)
    //    Rect baseAbilityRect = new Rect(abilityXOrigin,
    //                                    skillBarRect.y - abilityIconSpacing * 2.0f, // moved up a bit to distringuish it
    //                                    abilityIconWidth, abilityIconWidth);

    //    GUI.DrawTexture(baseAbilityRect, healthBarBackingTexture);
    //    GUI.Label(baseAbilityRect, playerOwner.GetAbiltyControlKeys()[0]);

    //    if(playerAbilities[0] != null) {
    //        float fillPct = playerAbilities[0].GetCooldownPct();
    //        if(playerOwner.IsCarryingLootBag()) {
    //            fillPct = 0.0f;
    //        }

    //        float yOffset = baseAbilityRect.height * fillPct;
    //        Rect overlayRect = new Rect(baseAbilityRect.x, baseAbilityRect.y + yOffset, baseAbilityRect.width, baseAbilityRect.height - yOffset);
    //        GUI.DrawTexture(overlayRect, abilityCooldownTexture);

    //        if(playerOwner.IsCarryingLootBag()) {
    //            GUI.DrawTexture(baseAbilityRect, abilityLockedTexture);
    //        }

    //        //GUI.Label(abilityRect, playerAbilities[i].name);
    //    }

    //    // Draw the ability bar
    //    for(int i = 0; i < Player.NUM_ABILTIES; i++) {
    //        Rect abilityRect = new Rect(abilityXOrigin + (abilityIconWidth + abilityIconSpacing) * (i + 1),
    //                                    skillBarRect.y,
    //                                    abilityIconWidth, abilityIconWidth);

    //        GUI.DrawTexture(abilityRect, healthBarBackingTexture);

    //        // +1 b/c we are doing the main abilities, and idx 0 is the players auto ability
    //        GUI.Label(abilityRect, playerOwner.GetAbiltyControlKeys()[i + 1]);

    //        if(playerAbilities[i + 1] != null) {
    //            float fillPct = playerAbilities[i + 1].GetCooldownPct();

    //            if(playerOwner.IsCarryingLootBag()) {
    //                fillPct = 0.0f;
    //            }

    //            float yOffset = abilityRect.height * fillPct;
    //            Rect overlayRect = new Rect(abilityRect.x, abilityRect.y + yOffset, abilityRect.width, abilityRect.height - yOffset);
    //            GUI.DrawTexture(overlayRect, abilityCooldownTexture);

    //            if(playerOwner.IsCarryingLootBag()) {
    //                GUI.DrawTexture(abilityRect, abilityLockedTexture);
    //            }

    //            //GUI.Label(abilityRect, playerAbilities[i].name);
    //        }
    //    }

    //    // Draw the camp fire ability (on the R key by default)
    //    int lastAbilityIdx = playerOwner.GetAbiltyControlKeys().Length - 1;

    //    Rect fireAbilityRect = new Rect(abilityXOrigin + (abilityIconWidth + abilityIconSpacing) * (lastAbilityIdx),
    //                                    skillBarRect.y,
    //                                    abilityIconWidth, abilityIconWidth);

    //    GUI.DrawTexture(fireAbilityRect, healthBarBackingTexture);

    //    GUI.Label(fireAbilityRect, playerOwner.GetAbiltyControlKeys()[lastAbilityIdx]);

    //    if(playerAbilities[lastAbilityIdx] != null) {
    //        float fillPct = playerAbilities[lastAbilityIdx].GetCooldownPct();

    //        float yOffset = fireAbilityRect.height * fillPct;
    //        Rect overlayRect = new Rect(fireAbilityRect.x, fireAbilityRect.y + yOffset, fireAbilityRect.width, fireAbilityRect.height - yOffset);
    //        GUI.DrawTexture(overlayRect, abilityCooldownTexture);

    //        //GUI.Label(abilityRect, playerAbilities[i].name);
    //    }

    //    // To the right of the ability bar, is the inventory breifing: just a list of the items in the players inventory along with how many of each there are
    //    const float itemIconWidth = 30.0f;
    //    const float itemIconSpacing = 30.0f;
    //    const float itemNumberOffset = itemIconWidth + 5.0f;

    //    int index = 0;
    //    foreach(string itemName in playerInv.CraftingItems()) {
    //        int numInInv = playerInv.GetNumOfCraftingItem(itemName);

    //        Rect itemRect = new Rect(abilityXOrigin + abilityBarWidth + 25.0f + index * (itemIconWidth + itemIconSpacing),
    //                                 skillBarRect.y, itemIconWidth, itemIconWidth);

    //        Rect textureRect;
    //        Texture itemTex = droppedItemPrototype.GetItemTexture(itemName, out textureRect);

    //        Rect subTextureRect = new Rect(textureRect.x / itemTex.width, textureRect.y / itemTex.height,
    //                                       textureRect.width / itemTex.width, textureRect.height / itemTex.height);

    //        //GUI.DrawTexture(itemRect, abilityCooldownTexture);
    //        GUI.DrawTextureWithTexCoords(itemRect, itemTex, subTextureRect);

    //        itemRect.x += itemNumberOffset;
    //        string itemNum = "x" + numInInv;
    //        GUI.Label(itemRect, itemNum, mainStyle);

    //        index++;
    //    }

    //    // Draw health bar above abilities
    //    //healthBarRect.width = (abilityIconWidth + abilityIconSpacing) * (Player.NUM_ABILTIES) - abilityIconSpacing;
    //    //healthBarRect.x = abilityXOrigin + abilityIconWidth + abilityIconSpacing;
    //    //GUI.DrawTexture(healthBarRect, healthBarBackingTexture);

    //    //healthBarRect.width *= playerOwner.GetHealthPercent();

    //    // Set the color of the health bar based on the current player effects
    //    Pawn.EffectType currentEffect = playerOwner.GetMostSevereEffect();

    //    //switch(currentEffect) {
    //    //    case Pawn.EffectType.Bleeding:
    //    //        GUI.DrawTexture(healthBarRect, healthBarTexture_bleed);
    //    //        break;
    //    //    case Pawn.EffectType.Poison:
    //    //        GUI.DrawTexture(healthBarRect, healthBarTexture_poison);
    //    //        break;
    //    //    case Pawn.EffectType.Burning:
    //    //        GUI.DrawTexture(healthBarRect, healthBarTexture_burn);
    //    //        break;
    //    //    default:
    //    //        GUI.DrawTexture(healthBarRect, healthBarTexture);
    //    //        break;
    //    //}

    //}


    private void DrawKickers() {
        // Draw all active kickers
        foreach(KickerEvent kevent in kickers) {
            mainStyle.fontSize = 12;
            mainStyle.normal.textColor = kevent.col;

            Rect kickerRect = new Rect(kevent.screenLoc.x, kevent.screenLoc.y, 50.0f, 25.0f);
            GUI.Label(kickerRect, kevent.text, mainStyle);
        }

        mainStyle.normal.textColor = Color.black;
    }


    // Add the player - NPC interaction window
    private void DrawInteractionWindow() {
        NPC interactingNPC = playerOwner.GetInteractableNPC();
        if(interactingNPC == null) {
            return;
        }

        List<NPC.Interaction> npcInteractions = interactingNPC.GetInteractionList(playerOwner);
        const float w = 200.0f;
        const float btn_height = 25.0f;
        const float btn_spacing = 5.0f;
        float x = Screen.width / 2.0f - w / 2.0f;
        float startY = Screen.height / 2.0f + 25.0f;

        // First draw the npc's current text
        mainStyle.fontSize = 12;
        string npcStatement = interactingNPC.GetCurrentStatement();
        Rect statementRect = new Rect(x, Screen.height / 2.0f - 75.0f, w, 50.0f);
        GUI.Label(statementRect, npcStatement, mainStyle);

        // Finally draw all the possible interactions
        for(int i = 0; i < npcInteractions.Count; i++) {
            float y = startY + (btn_height + btn_spacing) * i;
            Rect btn_Rect = new Rect(x, y, w, btn_height);

            string key_prefix = "[" + (i + 1) + "] ";

            // The button populated with interaction content and handler
            if(GUI.Button(btn_Rect, key_prefix + npcInteractions[i].text, mainStyle) || Input.GetKeyDown(GetNumberKey(i))) {
                npcInteractions[i].handler(this);
            }
        }

        // And always allow abort by pressing escape
        if(Input.GetKeyDown(KeyCode.Escape)) {
            npcInteractions[0].handler(this);
        }
    }


    private void DrawTitleText() {
        float activeTime = Time.time - titleTextInitiatedTime;
        float alphaRatio = 1.0f;
        if(activeTime > titleTextLingerTime) {
            alphaRatio = 1.0f - (activeTime - titleTextLingerTime) / titleTextFadeTime;
        }

        mainStyle.fontSize = 64;
        mainStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f * alphaRatio);
        Vector2 textSize = mainStyle.CalcSize(new GUIContent(titleText));

        Rect titleRect = new Rect(Screen.width / 2.0f - textSize.x / 2.0f,
                                    Screen.height / 2.0f - textSize.y - 25.0f,
                                    textSize.x + 100.0f, textSize.y + 100.0f);

        GUI.Label(titleRect, titleText, mainStyle);
    }


    public void StartItemAquiredSequence(string itemName, Texture itemImage) {
        itemAquiredPopupInfo = new ItemAquiredPopupInfo();
        itemAquiredPopupInfo.image = itemImage;
        itemAquiredPopupInfo.name = "Aquired " + itemName + "!";
        itemAquiredPopupInfo.activatedTime = Time.time;

        bDrawItemPopup = true;
    }


    private void DrawItemAquired() {
        const float decayTime = 2.0f;
        const float imgSize = 128.0f;

        if(itemAquiredPopupInfo.activatedTime <= 0.0f) {
            return;
        }

        float sinceActivation = Time.time - itemAquiredPopupInfo.activatedTime;

        // check if it should be on screen
        if(sinceActivation < decayTime) {
            Rect popupRect = new Rect(Screen.width / 2.0f - imgSize / 2.0f, Screen.height / 2.0f - imgSize / 2.0f, imgSize, imgSize);

            float alphaRatio = 1.0f - sinceActivation / decayTime;

            mainStyle.fontSize = 32;
            mainStyle.normal.textColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);

            GUI.DrawTexture(popupRect, itemAquiredPopupInfo.image);

            Vector2 textSize = mainStyle.CalcSize(new GUIContent(itemAquiredPopupInfo.name));

            popupRect.y += imgSize + 50.0f;
            popupRect.x = Screen.width / 2.0f - textSize.x / 2.0f;

            GUI.Label(popupRect, itemAquiredPopupInfo.name, mainStyle);
        }
        else {
            bDrawItemPopup = false;
        }
    }


    // Interface to the kicker number system: Must originate off of an actor of any sort
    public void AddKickerNumber(Actor source, int displayVal, KickerType type) {
        KickerEvent newKicker = new KickerEvent();
        newKicker.col = GetKickerColor(type);
        newKicker.text = ""+displayVal;
        newKicker.lifeTime = KICKER_LIFESPAN;
        newKicker.loc = source.transform.position + source.GetBaseOffset();
        newKicker.loc.y += KICKER_VERT_OFFSET;

        kickers.Add(newKicker);
    }



    // Opens the trading interface with the given npc as the trader
    public void OpenTradeMenuFor(NPC trader) {
        // ensure they arnt already trading with someone
        if(trader != null && !tradingMenu.IsOpen()) {
            tradingMenu.SetTrader(trader);
            tradingMenu.OpenMenu();
        }
    }


    // Gives the color for the given kicker type
    private Color GetKickerColor(KickerType type) {
        switch(type) {
            case KickerType.Friendly:
                return friendlyColor;
            case KickerType.Neutral:
                return neutralColor;
            case KickerType.Aggresive:
                return aggresiveColor;
        }
        return neutralColor;
    }


    /**
     * Initiates a big full screen title text that will stay around for screenTime, then fade out over fadeOutTime
     */ 
    public void DoFullScreenTitleText(string text, float screenTime, float fadeOutTime) {
        bDrawTitleText = true;

        titleText = text;
        titleTextInitiatedTime = Time.time;
        titleTextLingerTime = screenTime;
        titleTextFadeTime = fadeOutTime;
    }



    // Returns the keycode for the integer, where 0 = Alpha1 etc, for number keys
    private KeyCode GetNumberKey(int num) {
        return (KeyCode)(49 + num);
    }
}
