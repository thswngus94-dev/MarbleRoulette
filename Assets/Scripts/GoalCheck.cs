using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    public string prizeName = "WINNER";
    


    private void OnTriggerEnter2D(Collider2D other)
    {
        // 태그 확인
        if (other.CompareTag("Player"))
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.ShowResult(prizeName);
            }
        }
    }


}