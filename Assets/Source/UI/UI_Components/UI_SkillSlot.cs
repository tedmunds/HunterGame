using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UI_SkillSlot : MonoBehaviour {

    [SerializeField]
    private Image coolDownOverlay;

    [SerializeField]
    private Image lockedOverlay;

    [SerializeField]
    public int skillIndex = 0;

    public float coolDownLevel;
    public bool bIsLocked;

    public void UpdateCoolDownLevel() {
        coolDownOverlay.fillAmount = bIsLocked? 1.0f : coolDownLevel;
    }

    public void UpdateLockedOverlay() {
        lockedOverlay.enabled = bIsLocked;
    }


    protected virtual void Start() {
        WorldManager.instance.GetUIManager().BindVariable("coolDownLevel", this, "skill" + skillIndex, UpdateCoolDownLevel);
        WorldManager.instance.GetUIManager().BindVariable("bIsLocked", this, "LootBagHeld", UpdateLockedOverlay);
    }


}