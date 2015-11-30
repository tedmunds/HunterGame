using UnityEngine;
using System.Collections;

public class UI_HealthBar : UI_ValueBar {

    [SerializeField]
    public Color NormalColor;

    [SerializeField]
    public Color BleedingColor;

    [SerializeField]
    public Color BuringColor;

    [SerializeField]
    public Color PoisonColor;

    public Pawn.EffectType currentEffect;

    public void UpdateBarColor() {
        switch(currentEffect) {
            case Pawn.EffectType.Bleeding:
                foreground.color = BleedingColor;
                break;
            case Pawn.EffectType.Burning:
                foreground.color = BuringColor;
                break;
            case Pawn.EffectType.Poison:
                foreground.color = PoisonColor;
                break;
            default:
                foreground.color = NormalColor;
                break;
        }
    }


    protected override void Start() {
        base.Start();
        WorldManager.instance.GetUIManager().BindVariable("currentEffect", this, "PlayerEffect", UpdateBarColor);
    }
}