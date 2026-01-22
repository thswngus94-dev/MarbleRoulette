using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 설정
    public static GameManager instance;

    // 이름과 숫자를 입력창 버튼
    public TMP_InputField playerCountInput;
    public Button spawnButton;

    // 프리펩 설정
    public GameObject playerPrefab;
    public Transform spawnPoint;

    // 가로/세로 간격 변수 분리
    public float spacingX = 0.5f;
    public float spacingY = 0.6f;
    public int maxPerRow = 10; // 한 줄당 최대 개수

    // 실시간으로 생성된 구슬들을 관리할 리스트
    private List<GameObject> activePlayers = new List<GameObject>();

    // 골 체크 관련
    public TextMeshProUGUI resultText;
    private bool isGameOver = false;

    void Awake()
    { 
        // 싱글톤 패턴: 다른 스크립트에서 GameManager.instance로 접근 가능
        instance = this;
    }

    void Start()
    {
        resultText.gameObject.SetActive(false);

        // 입력창의 숫자가 바뀔 때마다 실행되도록 연결
        playerCountInput.onValueChanged.AddListener(delegate { OnInputChanged(); });
    }

    // 숫자가 바뀔 때마다 호출되는 함수
    public void OnInputChanged()
    {
        string input = playerCountInput.text;
        if (string.IsNullOrEmpty(input))
        {
            UpdatePlayerList(0, new List<string>());
            return;
        }

        List<string> nameList = new List<string>();

        // 1. 우선 콤마(,)를 기준으로 모든 항목을 나눕니다.
        // (콤마가 없어도 이 함수는 전체 문장을 하나로 가져옵니다)
        string[] entries = input.Split(',');

        foreach (string entry in entries)
        {
            string trimmedEntry = entry.Trim();
            if (string.IsNullOrEmpty(trimmedEntry)) continue;

            // 2. 각 항목 안에 별표(*)가 있는지 개별적으로 확인합니다.
            if (trimmedEntry.Contains("*"))
            {
                string[] parts = trimmedEntry.Split('*');
                string baseName = parts[0].Trim();

                if (parts.Length > 1 && int.TryParse(parts[1].Trim(), out int count))
                {
                    for (int i = 0; i < count; i++)
                    {
                        nameList.Add(count > 1 ? $"{baseName}{i + 1}" : baseName);
                    }
                }
                else
                {
                    // 숫자가 잘못 입력된 경우 그냥 글자 그대로 추가
                    nameList.Add(trimmedEntry);
                }
            }
            else
            {
                // 별표가 없는 일반 이름인 경우
                nameList.Add(trimmedEntry);
            }
        }

        // 최종적으로 합쳐진 nameList의 개수대로 구슬 생성
        UpdatePlayerList(nameList.Count, nameList);

    }

    // 리스트의 개수를 입력값에 맞게 조절하는 핵심 로직
    void UpdatePlayerList(int count, List<string> names)
    {
        // 부족하면 더 생성
        while (activePlayers.Count < count)
        {
            CreatePlayer(activePlayers.Count); // baseName 인자 삭제
        }

        // 많으면 마지막부터 삭제
        while (activePlayers.Count > count)
        {
            RemoveLastPlayer();
        }

        // 이름 실시간 업데이트 (리스트에 담긴 각각의 이름을 부여)
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (i < names.Count) // 바구니에 이름이 있는 만큼만
            {
                activePlayers[i].name = names[i]; // 구슬 오브젝트의 이름을 리스트에 있는 이름으로 교체!
            }
        }
    }

    // 구슬 하나 생성 및 위치 계산 (줄바꿈 포함)
    void CreatePlayer(int index)
    {
        int row = index / maxPerRow;    // 줄 번호
        int col = index % maxPerRow;    // 칸 번호

        // 줄바꿈 위치 계산
        Vector3 pos = spawnPoint.position + new Vector3(col * spacingX, -row * spacingY, 0);

        GameObject newPlayer = Instantiate(playerPrefab, pos, Quaternion.identity);
        

        Rigidbody2D rb = newPlayer.GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic; // 고정 상태

        activePlayers.Add(newPlayer);
    }

    // 구슬 하나 삭제
    void RemoveLastPlayer()
    {
        if (activePlayers.Count > 0)
        {
            GameObject last = activePlayers[activePlayers.Count - 1];
            activePlayers.RemoveAt(activePlayers.Count - 1);
            Destroy(last);
        }
    }

    // 버튼을 눌렀을 때 실행될 함수 / 게임 시작 버튼
    public void OnSpawnButtonClick()
    {

        StartCoroutine(StartGameCoroutine());
        spawnButton.interactable = false;
    }

    // 대기/발사 로직
    IEnumerator StartGameCoroutine()
    {
        // 3초 대기
        yield return new WaitForSeconds(2.0f);
    
        // 리스트에 있는 모든 구슬 물리 가동
        foreach (GameObject player in activePlayers)
        {
            if (player != null)
            {
                Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.bodyType = RigidbodyType2D.Dynamic;
                    rb.AddForce(new Vector2(Random.Range(-1f, 1f), 0), ForceMode2D.Impulse);
                }
            }
        }
    }
    
    
    // 골인 지점에서 호출할 함수 (당첨된 오브젝트를 매개변수로 받음)
    public void ShowResult(string winner, string winnerName)
        {
            if (!isGameOver)
            {
                isGameOver = true;
                resultText.gameObject.SetActive(true);
                // 누가 당첨되었는지 이름 표시
    
                resultText.text = winner + "\n" + winnerName + "!";
    
            }
        }
   
}