
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public float moveSpeed = 2f; // سرعة الحركة
    public float moveRange = 3f; // مدى الحركة
    private Vector3 startingPosition; // موقع البداية
    private bool movingRight = true; // الاتجاه الحالي للحركة

    void Start()
    {
        startingPosition = transform.position; // حفظ موقع البداية
    }

    void Update()
    {
        // الحصول على موضع الشخصية
        float newPosition = transform.position.x;

        // تحريك الشخصية
        if (movingRight)
        {
            newPosition += moveSpeed * Time.deltaTime;
            if (newPosition >= startingPosition.x + moveRange)
            {
                movingRight = false; // تغيير الاتجاه إلى اليسار
            }
        }
        else
        {
            newPosition -= moveSpeed * Time.deltaTime;
            if (newPosition <= startingPosition.x - moveRange)
            {
                movingRight = true; // تغيير الاتجاه إلى اليمين
            }
        }

        // تحديث موقع الشخصية
        transform.position = new Vector3(newPosition, transform.position.y, transform.position.z);
    }
}