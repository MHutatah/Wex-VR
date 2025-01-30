using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneButton : MonoBehaviour
{
    // This method can be called by a VR button press (for example, using XR Toolkit or Unity's Input System).
    public void GoToRoom1()
    {
        // Load Scene "Room1" (Make sure the scene is added to the build settings)
        SceneManager.LoadScene("Room2");
    }
}
