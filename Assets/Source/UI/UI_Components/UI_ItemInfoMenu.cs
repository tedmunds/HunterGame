using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_ItemInfoMenu : MonoBehaviour {

    [SerializeField]
    public Image itemImage;

    [SerializeField]
    public Text itemNameField;

    [SerializeField]
    public Text itemStrengthField;

    [SerializeField]
    public Text equippedStrengthField;

    [SerializeField]
    private Color bestItemColor;

    [SerializeField]
    private Color worseItemColor;

    [SerializeField]
    private Color equalItemColor;

	void Start() {
	    
	}

}