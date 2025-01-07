using UnityEngine;

public class StrengthItem : CellObject
{
    public int AmountGranted = 1;
    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeStrength(AmountGranted);
        GameManager.Instance.playerController.IncreaseStrength(AmountGranted);
    }
}
