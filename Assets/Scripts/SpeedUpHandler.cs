using UnityEngine;
using UnityEngine.EventSystems; // 터치 감지를 위해 필요

public class SpeedUpHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
   
    public float fastSpeed = 3.0f;
    private float normalSpeed = 1.0f;

    // 화면을 누르는 순간 호출
    public void OnPointerDown(PointerEventData eventData)
    {
        Time.timeScale = fastSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    // 화면에서 손을 떼는 순간 호출
    public void OnPointerUp(PointerEventData eventData)
    {
        ResetSpeed();
    }

    // 속도를 정상으로 돌리는 함수 (골인 시에도 호출하기 위함)
    public void ResetSpeed()
    {
        Time.timeScale = normalSpeed;
        Time.fixedDeltaTime = 0.02f;
    }

    // 패널이 비활성화될 때(골인 등) 자동으로 속도 복구
    private void OnDisable()
    {
        ResetSpeed();
    }
}