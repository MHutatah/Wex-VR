
using UnityEngine;

public class mov : MonoBehaviour
{
    public float moveSpeed = 5f; // سرعة الحركة

    void Update()
    {
        // الحصول على المدخلات من لوحة المفاتيح
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        // إنشاء متجه للحركة
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // تحريك الكائن
        transform.Translate(movement * moveSpeed * Time.deltaTime);
    }
}