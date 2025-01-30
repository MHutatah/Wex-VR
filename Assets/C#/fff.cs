using UnityEngine;
using UnityEngine.SceneManagement; // مكتبة إدارة المشاهد

public class SceneSwitcher : MonoBehaviour
{
    // دالة لتغيير المشهد عند الضغط على الزر
    public void SwitchToStartScene()
    {
        SceneManager.LoadScene("Start"); // تحميل مشهد "Start"
    }
}
