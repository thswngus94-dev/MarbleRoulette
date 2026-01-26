using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 어디서든 접근 가능하게 설정
    public static GameManager instance;

    public GameObject uiInput;

    // 이름과 숫자를 입력창 버튼
    public TMP_InputField playerCountInput;
    public Button spawnButton;
    public Button shuffleButton;
    public Button QuitButton;

    // 프리펩 설정
    public GameObject playerPrefab;
    public Transform spawnPoint;

    // 가로/세로 간격 변수 분리
    public float spacingX = 0.1f;
    public float spacingY = 0.15f;
    public int maxPerRow = 10; // 한 줄당 최대 개수

    // 실시간으로 생성된 구슬들을 관리할 리스트
    private List<GameObject> activePlayers = new List<GameObject>();

    // 골 체크 관련
    public GameObject resultPanel; // Result_Text 부모 오브젝트
    public TextMeshProUGUI winnerLabel; // "WINNER" 표시용
    public TextMeshProUGUI playerNameText; // "플레이어 이름" 표시용
    private bool isGameOver = false;

    // 랭킹 관련
    public TextMeshProUGUI rankingText; // Ranking_Text 드래그 연결
    public int displayCount = 10;       // 상위 몇 명까지 보여줄지
    private int finishedCount = 0;

    // 슬로우 모션 설정
    public float slowMotionThreshold = -72f; // 슬로우 모션 시작 Y 위치
    public float slowMotionScale = 0.5f;    // 얼마나 느려질지 (0.2는 5배 느려짐)
    private bool isSlowMotionActive = false;

    // 컨페티 파티클 프리팹
    public GameObject confettiPrefab;

    public GameObject speedUpPanel;

    void Awake()
    { 
        // 싱글톤 패턴: 다른 스크립트에서 GameManager.instance로 접근 가능
        instance = this;
    }

    void Start()
    {
        resultPanel.gameObject.SetActive(false);

        // 입력창의 숫자가 바뀔 때마다 실행되도록 연결
        playerCountInput.onValueChanged.AddListener(delegate { OnInputChanged(); });
    }


    //-------------------------------------------------------
    // 구슬 소환 및 이름 관리 (OnInputChanged, UpdatePlayerList)
    // 입력 : 이름 입력 >> 구슬 자동생성

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

        // 1우선 콤마(,)를 기준으로 모든 항목을 나눕니다.
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

        // 3. 이름 실시간 업데이트 (오브젝트 이름 + 머리 위 이름표)
        for (int i = 0; i < activePlayers.Count; i++)
        {
            if (i < names.Count) // 전달받은 이름 리스트 범위 내에서만 작동
            {
                // [오브젝트 이름 변경] 하이어라키 창에서 보일 이름
                activePlayers[i].name = names[i];

                // [머리 위 이름표 변경] 프리팹 내부의 Text(TMP) 변경
                // 프리팹에 붙여둔 PlayerInfo 스크립트를 찾아옵니다.
                Player info = activePlayers[i].GetComponent<Player>();

                if (info != null)
                {
                    Color randomColor = Color.HSVToRGB(Random.value, 0.7f, 0.9f);

                    info.SetInfo(names[i], randomColor); // PlayerInfo의 SetInfo 함수 실행
                }
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

    

    // 기존 구슬들을 완전히 삭제하는 안전한 함수 추가
    void ClearAllPlayers()
    {
        foreach (GameObject player in activePlayers)
        {
            if (player != null)
            {
                Destroy(player);
            }
        }
        activePlayers.Clear(); // 리스트 비우기

    }

    //-------------------------------------------------------
    // 게임 실행 및 물리
    // 버튼을 눌렀을 때 실행될 함수 / 게임 시작 버튼
    public void OnSpawnButtonClick()
    {
        finishedCount = 0; // 새로 시작할 때 완주자 수를 0으로 리셋
        isGameOver = false; // 게임 시작 상태로 설정

        // 버튼 클릭 시 UI 그룹을 비활성화
        if (speedUpPanel != null) speedUpPanel.SetActive(true);
        if (uiInput != null) uiInput.SetActive(false);
        if (shuffleButton != null) shuffleButton.interactable = false;

        StartCoroutine(StartGameCoroutine());
        spawnButton.interactable = false;
    }
    // 구슬 위치 섞기 함수
    public void OnShuffleButtonClick()
    {

        // 만약 게임이 끝난 상태라면? -> 게임 리셋(처음으로)
        if (isGameOver)
        {
            ResetGame();
            return;
        }

        // 게임 중이라면? -> 기존 섞기 로직 실행
        if (activePlayers.Count <= 1) return; // 구슬이 0~1개면 섞을 필요 없음

        // 현재 모든 구슬이 서 있는 '좌표'들만 따로 복사해서 리스트를 만듭니다.
        List<Vector3> gridPositions = new List<Vector3>();
        foreach (GameObject player in activePlayers)
        {
            gridPositions.Add(player.transform.position);
        }

        //  좌표 리스트의 순서를 무작위로 섞습니다.
        for (int i = 0; i < gridPositions.Count; i++)
        {
            Vector3 temp = gridPositions[i];
            int randomIndex = Random.Range(i, gridPositions.Count);
            gridPositions[i] = gridPositions[randomIndex];
            gridPositions[randomIndex] = temp;
        }

        // 섞인 좌표들을 구슬들에게 하나씩 다시 배정합니다.
        for (int i = 0; i < activePlayers.Count; i++)
        {
            activePlayers[i].transform.position = gridPositions[i];
        }

    }
    // 대기/발사
    IEnumerator StartGameCoroutine()
    {
        // 구슬 발사 1.5초 대기
        yield return new WaitForSeconds(1.5f);
    
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

        // 2초 동안 대기 (전체 화면 유지)
        yield return new WaitForSeconds(2.2f);

        // 실시간으로 1등을 추적하는 루틴 시작
        StartCoroutine(TrackLeadPlayer());
    }

    //-------------------------------------------------------
    // 실시간 랭킹 갱신 : 1등 추적 카메라 + 실시간 랭킹 업데이트.
    void Update()
    {
        // 게임 중일 때만 실시간 랭킹 업데이트
        if (!isGameOver && activePlayers.Count > 0)
        {
            UpdateRanking();
        }
    }
    void UpdateRanking()
    {
        // 살아있는 플레이어 정렬
        var sortedPlayers = activePlayers
            .Where(p => p != null)
            .OrderBy(p => p.transform.position.y)
            .ToList();


        // sortedPlayers.Count = 현재 맵에 존재하는 구슬의 총 개수  // (완주자수 / 전체참가자수)
        string rankingString = $" ({finishedCount} / {sortedPlayers.Count})\n";

        // 리스트 부분: 이름 #순위
        int showLimit = Mathf.Min(displayCount, sortedPlayers.Count);

        for (int i = 0; i < showLimit; i++)
        {
            string pName = sortedPlayers[i].name;
            Player info = sortedPlayers[i].GetComponent<Player>();

            if (info != null && info.ballSprite != null)
            {
                string colorHex = ColorUtility.ToHtmlStringRGB(info.ballSprite.color);

                //  "누구누구 #순위"
                rankingString += $"<color=#{colorHex}>{pName} #{i + 1}</color>\n";
            }
        }

        // UI 적용
        rankingText.text = rankingString;
    }

    IEnumerator TrackLeadPlayer()
    {
        while (!isGameOver)
        {
            GameObject leadPlayer = null;
            float lowestY = float.MaxValue; // Y값이 낮을수록(아래로 갈수록) 1등인 경우

            foreach (GameObject player in activePlayers)
            {
                if (player != null)
                {
                    // 여기서는 Y값이 가장 낮은(가장 많이 내려간) 플레이어를 1등으로 판정
                    if (player.transform.position.y < lowestY)
                    {
                        lowestY = player.transform.position.y;
                        leadPlayer = player;
                    }
                }
            }

            if (leadPlayer != null)
            {
                CameraFollow.instance.StartFollowing(leadPlayer.transform);

                // 1등이 -60보다 더 멀리 내려갔을 때
                if (!isSlowMotionActive && leadPlayer.transform.position.y <= slowMotionThreshold)
                {
                    StartSlowMotion();
                }

            }

            yield return new WaitForSeconds(0.5f); // 0.5초마다 1등을 갱신해서 카메라 타겟 변경
        }
    }
    void StartSlowMotion()
    {
        isSlowMotionActive = true;
        Time.timeScale = slowMotionScale;
        // 슬로우 모션 시 물리 연산 시간 간격도 함께 조절해야 부드럽습니다.
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    void StopSlowMotion()
    {
        isSlowMotionActive = false;
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }
    //-------------------------------------------------------
    //결과 및 종료 함수
    public void ShowResult(string winner, GameObject winnerObj)
    {
        if (!isGameOver)
        {
            // 골인 지점에 들어올 때마다 숫자 증가
            finishedCount++;

            StopSlowMotion();
            isGameOver = true;
            resultPanel.SetActive(true);

            // 결과창이 뜰 때 UI 입력창도 같이 다시 켭니다.
            if (uiInput != null)
            {
                uiInput.SetActive(true);
            }

            //결과 제목 설정
            winnerLabel.text = winner;
            // 당첨자 이름 설정
            playerNameText.text = winnerObj.name;

            // 당첨자 색상 입히기 (PlayerInfo에서 색상을 가져옴)
            Player info = winnerObj.GetComponent<Player>();
            if (info != null && info.ballSprite != null)
            {
                playerNameText.color = info.ballSprite.color;
            }

            // 1등이 결정되면 꺼져있던 섞기(리셋) 버튼을 다시 활성화
            if (shuffleButton != null)
            {
                shuffleButton.interactable = true;
               
            }

            PlayConfetti(winnerObj.transform.position);

            if (shuffleButton != null) shuffleButton.interactable = true;
        }
    }

    public void OnGoalReached()
    {
        if (speedUpPanel != null)
        {
            // 패널을 끄면 SpeedUpHandler의 OnDisable이 실행되어 자동으로 1배속이 됩니다.
            speedUpPanel.SetActive(false);
        }
    }

    void PlayConfetti(Vector3 ignorePos)
    {
        if (confettiPrefab == null) return;

        // 1. 카메라 좌표 계산 (Z축을 카메라-월드 사이 거리로 유연하게 설정)
        float distanceToCamera = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 screenPos = new Vector3(Screen.width * 0.9f, Screen.height * 0.1f, distanceToCamera);
        Vector3 spawnPos = Camera.main.ScreenToWorldPoint(screenPos);

        // 2D 게임이라면 Z축을 0으로 고정해주는 것이 안전합니다.
        spawnPos.z = 0;

        // 2. 생성 및 회전
        Quaternion rotation = Quaternion.Euler(0, 0, 45f);
        GameObject confetti = Instantiate(confettiPrefab, spawnPos, rotation);

        // 3. 색상 랜덤 및 레이어 강제 설정
        ParticleSystem ps = confetti.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            // 런타임에 레이어를 강제로 높여 UI 위로 올리기 (코드 방식)
            ps.GetComponent<Renderer>().sortingOrder = 100;

            var main = ps.main;
            main.startColor = new ParticleSystem.MinMaxGradient(Random.ColorHSV(0f, 1f, 1f, 1f, 0.7f, 1f));
            ps.Play();
        }

        Destroy(confetti, 5f);
    }

    // 게임을 처음 상태로 되돌리는 함수
    void ResetGame()
    {
        // 상태 즉시 변경 (중복 실행 방지)
        StopSlowMotion();
        isGameOver = false;
        finishedCount = 0;

        // 모든 코루틴 중단 (추적 루틴 등 방해 요소 제거)
        StopAllCoroutines();

        // UI 정리
        resultPanel.SetActive(false);
        rankingText.text = ""; // 랭킹창 초기화
        if (uiInput != null) uiInput.SetActive(true);

        // 버튼 상태 복구
        spawnButton.interactable = true;
        shuffleButton.interactable = true;

        // 카메라를 처음 지점으로 이동 (코루틴 실행)
        if (CameraFollow.instance != null)
        {
            CameraFollow.instance.StopFollowing();
        }

        // 구슬 완전히 삭제 및 리스트 클리어
        ClearAllPlayers();

        // 입력된 정보에 따라 구슬들 초기 위치에 다시 생성
        OnInputChanged();
    }

    public void QuitGame()
    {
        // 1. 유니티 에디터에서 실행 중일 때 종료 (테스트용)
        
        Application.Quit();
        Debug.Log("게임 종료 버튼이 눌렸습니다.");
    }


}