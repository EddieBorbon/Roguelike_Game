using UnityEngine;

public class SpeedItem : CellObject
{
    public int AmountGranted = 1;
    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.ChangeSpeed(AmountGranted);
        GameManager.Instance.playerController.IncreaseSpeed(AmountGranted);
    }
}
