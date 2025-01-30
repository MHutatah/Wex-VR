using UnityEngine;

public class ExitGameButton : MonoBehaviour
{
    // يمكن استدعاء هذه الطريقة عند الضغط على زر VR.
    public void ExitGame()
    {
        // إغلاق اللعبة
        Debug.Log("إغلاق اللعبة...");
        Application.Quit();

        // إذا كنت تشغل اللعبة في المحرر (لتجربة اللعبة داخل Unity Editor)، توقف عن وضع التشغيل
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}
