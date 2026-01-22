using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // 값이 클수록 빠르게 회전
    private float rotationSpeed = 200f;
    // true면 시계방향, false면 반시계방향
    public bool isClockwise = true;

    void Update()
    {
        // 방향 결정 (시계방향은 -1, 반시계방향은 1)
        float direction = isClockwise ? -1f : 1f;
        // 회전 적용
        transform.Rotate(0, 0, rotationSpeed * Time.deltaTime);
    }
}
