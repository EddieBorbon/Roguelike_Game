using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public void LoadSandTheme()
    {
        SceneManager.LoadScene("SandTheme");
    }

    public void LoadSnowTheme()
    {
        SceneManager.LoadScene("SnowTheme");
    }

    public void LoadUrbanTheme()
    {
        SceneManager.LoadScene("UrbanTheme");
    }
}