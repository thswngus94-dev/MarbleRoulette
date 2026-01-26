using UnityEngine;

public class Obstacle : MonoBehaviour
{
    // 값이 클수록 빠르게 회전
    public float rotationSpeed = 200f;
    // true면 시계방향, false면 반시계방향
    public bool isClockwise = true;

    void Update()
    {
        // 방향 결정 (시계방향은 -1, 반시계방향은 1)
        float direction = isClockwise ? -1f : 1f;
        // 회전 적용
        transform.Rotate(0, 0, rotationSpeed * direction * Time.deltaTime);
    }

    /* z축에다 사용하는 이유?
     * 평면에 놓인 종이(오브젝트)를 평면 위에서 돌리려면, 종이 정중앙에 핀(Z축)을 꽂고 돌려야 하죠? 
     * 그래서 transform.Rotate(0, 0, angle) 처럼 X, Y값은 0으로 두고 Z값만 변화를 주는 것입니다.*/
}
