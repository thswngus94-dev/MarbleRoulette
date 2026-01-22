using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 설정
    public static GameManager instance;

    // 골 체크 관련
    public TextMeshProUGUI resultText;
    private bool isGameOver = false;

    void Awake()
    { 
        // 싱글톤 패턴: 다른 스크립트에서 GameManager.instance로 접근 가능
        instance = this;
    }

    public void ShowResult(string prize)
    {
        if (!isGameOver)
        {
            isGameOver = true;

            // UI 텍스트 오브젝트를 활성화하고 문구 삽입
            resultText.gameObject.SetActive(true);
            resultText.text = prize + "\n 플레이어 누구" + "!";

        }
    }
}