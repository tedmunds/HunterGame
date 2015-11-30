using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_ValueBar : MonoBehaviour {

    [SerializeField]
    public Image foreground;

    [SerializeField]
    public string targetVariable;

    public float width = 1.0f;
    
    public void UpdateBar() {
        foreground.fillAmount = width;
    }

    protected virtual void Start() {
        WorldManager.instance.GetUIManager().BindVariable("width", this, targetVariable, UpdateBar);
    }
}