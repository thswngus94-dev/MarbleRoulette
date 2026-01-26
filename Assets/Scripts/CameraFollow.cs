using UnityEngine;
using System.Collections; // 코루틴을 위해 추가

public class CameraFollow : MonoBehaviour
{
    public static CameraFollow instance;

    public float smoothSpeed = 0.125f; // 카메라 이동 부드러움
    public Vector3 offset = new Vector3(0, 0, -10); // 카메라와 플레이어 사이 거리
    public float overviewSize = 10f; // 전체 화면일 때 카메라 크기 (Orthographic Size)
    public float followSize = 5f;    // 줌인했을 때 카메라 크기

    private Transform target; // 추적 대상 (1등 플레이어)
    private bool isFollowing = false;
    private Camera cam;

    private Vector3 startPosition; // 리셋을 위해 시작 위치를 저장할 변수

    void Awake() { instance = this; }

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographicSize = overviewSize; // 시작은 전체 화면 크기
        startPosition = transform.position;  // 처음 카메라 위치 저장
    }

    void LateUpdate()
    {
        if (!isFollowing || target == null) return;

        // 1등 플레이어의 위치로 부드럽게 이동
        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        // 카메라 줌인 효과
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, followSize, Time.deltaTime);
    }

    public void StartFollowing(Transform playerTransform)
    {
        
        target = playerTransform;
        isFollowing = true;
    }

    public void StopFollowing()
    {
        isFollowing = false;
        target = null;

        StopAllCoroutines();
        StartCoroutine(ReturnToOrigin());
    }

    IEnumerator ReturnToOrigin()
    {
        float elapsed = 0f;
        float duration = 1.0f; // 1초 동안 복귀 (원하는 대로 조절 가능)

        Vector3 currentPos = transform.position;
        float currentSize = cam.orthographicSize;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 부드러운 가속/감속 효과를 위해 가중치 조절 (선택 사항)
            t = t * t * (3f - 2f * t);

            // 위치 복구
            transform.position = Vector3.Lerp(currentPos, startPosition, t);
            // 줌(Size) 복구
            cam.orthographicSize = Mathf.Lerp(currentSize, overviewSize, t);

            yield return null;
        }

        transform.position = startPosition;
        cam.orthographicSize = overviewSize;
    }
}