using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UI_InventoryMenu : MonoBehaviour {

    [SerializeField]
    public UI_ItemInfoMenu infoMenu;

    [SerializeField]
    public UI_ItemSlotButton itemSlotPrototype;

    [SerializeField]
    int gridWidth;

    [SerializeField]
    int gridHeight;

    public List<UI_ItemSlotButton> itemSlots;

    void Start() {
        infoMenu.gameObject.SetActive(false);

        // place all item tiles
        for(int y = 0; y < gridHeight; y++) {
            for(int x = 0; x < gridWidth; x++) {
                UI_ItemSlotButton slot = Instantiate<UI_ItemSlotButton>(itemSlotPrototype);
                RectTransform t = (RectTransform)slot.transform;
                t.SetParent((RectTransform)gameObject.transform, false);

                t.localPosition = new Vector2(-100.0f + x * 30.0f + x, 50.0f - y * 30.0f - y * 2.0f);

                // Each slot has a unique id taht corresponds to its location in the players inventory
                slot.id = x + y * gridWidth;

                slot.SetEmpty();
                itemSlots.Add(slot);

                // Assigns the item selected method as the onclick listener, and uses the buttons id as its parrameter
                Button btn = slot.GetComponent<Button>();
                btn.onClick.AddListener(() => ItemSelected(slot.id));
            }
        }
    }



    public void ItemSelected(int id) {
        Debug.Log("Clicked: " + id);
    }

}