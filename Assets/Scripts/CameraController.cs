using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public CinemachineCamera virtualCamera; // Referencia a la cámara virtual
    public BoardManager boardManager; // Referencia al BoardManager
    public Transform player; // Referencia al jugador
    public CinemachinePositionComposer positionCamera;

    private float baseOrthographicSize = 3f; // Tamaño base de la cámara para un tablero 5x5
    private Vector3 baseTargetOffset = new Vector3(3f, 3f, 0f); // Offset base para un tablero 5x5

    private void Start()
    {
        if (virtualCamera == null || boardManager == null || player == null)
        {
            Debug.LogError("Virtual Camera, Board Manager o Jugador no asignados.");
            return;
        }
        positionCamera.TargetOffset = baseTargetOffset;
        // Suscribirse al evento de inicialización del tablero
        AdjustCamera(boardManager.BaseWidth, boardManager.BaseHeight);
    }

    public void AdjustCamera(float baseWidth, float baseHeight)
    {
        // Calcular el Orthographic Size dinámico
        float orthographicSize = Mathf.Max(baseWidth, baseHeight) / 2f;
        virtualCamera.Lens.OrthographicSize = orthographicSize;

        // Calcular el Target Offset dinámico
        Vector3 mapCenter = new Vector3(baseWidth / 2f, baseHeight / 2f, 0f);
        // Aplicar el Target Offset al Cinemachine Position Composer
        positionCamera.TargetOffset = mapCenter;
    }
}