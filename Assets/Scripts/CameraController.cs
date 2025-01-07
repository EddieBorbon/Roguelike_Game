using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera virtualCamera; 
    public BoardManager boardManager; 
    public Transform player; 
    public CinemachinePositionComposer positionCamera;

    private float baseOrthographicSize = 5f; 
    private Vector3 baseTargetOffset = new Vector3(3f, 3f, 0f); 


    private void Start()
    {
        if (virtualCamera == null || boardManager == null || player == null)
        {
            return;
        }
        positionCamera.TargetOffset = baseTargetOffset;        
    }

    public void AdjustCamera(float baseWidth, float baseHeight)
    {
        float orthographicSize = Mathf.Max(baseWidth, baseHeight)/2f;
        virtualCamera.Lens.OrthographicSize = orthographicSize;

        Vector3 mapCenter = new Vector3(baseWidth / 2f, baseHeight / 2f, 0f);
        positionCamera.TargetOffset = mapCenter;
    }
}