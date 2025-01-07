using UnityEngine;

public class DefenseItem : CellObject
{
    public int AmountGranted = 1;

    public override void PlayerEntered()
    {
        Destroy(gameObject);
        GameManager.Instance.playerController.ActivateTemporaryDefense(AmountGranted);
        GameManager.Instance.m_HasTemporaryDefense = true;
        GameManager.Instance.ChangeDefense(true);
    }
}