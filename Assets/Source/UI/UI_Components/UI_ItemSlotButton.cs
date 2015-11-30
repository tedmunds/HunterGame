using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Button))]
public class UI_ItemSlotButton : MonoBehaviour {

    [SerializeField]
    public Image itemImage;

    public int id;


    public void SetEmpty() {
        itemImage.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);
    }
}