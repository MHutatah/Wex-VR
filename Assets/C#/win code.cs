using UnityEngine;
using UnityEngine.SceneManagement; // مكتبة إدارة المشاهد

public class WinOnTouch : MonoBehaviour
{
    public string winSceneName = "win"; // اسم مشهد الفوز

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // التحقق إذا كان الكائن الذي لمسه هو اللاعب
        {
            SceneManager.LoadScene(winSceneName); // تحميل مشهد الفوز
        }
    }
}
