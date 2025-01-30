using UnityEngine;
using UnityEngine.SceneManagement;

public class fff : MonoBehaviour
{
    public void SwitchToScene()
    {
        SceneManager.LoadScene("Game"); // استبدل "Scene2" باسم المشهد الذي تريده.
    }
}
