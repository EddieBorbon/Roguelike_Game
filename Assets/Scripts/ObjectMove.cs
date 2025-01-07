using System.Collections;
using UnityEngine;

public class ObjectMove : MonoBehaviour
{
    // Singleton instance
    public static ObjectMove Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this; // Assign this instance as the singleton
            DontDestroyOnLoad(gameObject); // Optional: persist across scenes
        }
        else
        {
            Destroy(gameObject); // Destroy any duplicates
        }
    }

    // Method to move objects smoothly
    public IEnumerator SmoothMove(Transform objectTransform, Vector3 targetPosition, float moveSpeed)
    {
        Vector3 startPosition = objectTransform.position;
        float elapsedTime = 0;

        while (elapsedTime < 1f / moveSpeed)
        {
            objectTransform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime * moveSpeed);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectTransform.position = targetPosition;
    }
}