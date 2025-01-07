using UnityEngine;

public class DefenseItem : CellObject
{
    public int AmountGranted = 1;
    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeDefense(AmountGranted);
        GameManager.Instance.playerController.IncreaseDefense(AmountGranted);
    }
}