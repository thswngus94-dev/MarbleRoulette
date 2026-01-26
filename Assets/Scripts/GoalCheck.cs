using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    public string prizeName = "WINNER";
    public GameManager gameManager;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                // 이름 문자열 대신, 부딪힌 '게임 오브젝트' 자체를 보냅니다.
                GameManager.instance.ShowResult(prizeName, other.gameObject);
                GameManager.instance.OnGoalReached();
            }
        }
    }


}