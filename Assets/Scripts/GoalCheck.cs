using UnityEngine;

public class GoalCheck : MonoBehaviour
{
    public string winner = "WINNER";
    private bool isGameFinished = false;


    private void OnTriggerEnter2D(Collider2D other)
    {
        // ���� ������Ʈ�� �±װ� "Marble"���� Ȯ��
        if (other.CompareTag("Player"))
        {
            // ������ ���� ������ ���� (�ӵ��� 0���� ����� ������ų ���� ����)
            Rigidbody2D marbleRb = other.GetComponent<Rigidbody2D>();
            if (marbleRb != null)
            {
                marbleRb.linearVelocity = Vector2.zero; // ���� ����
                marbleRb.isKinematic = true;      // ���� ���� ���� (����)
            }

            // ��� ���
            Debug.Log("WINNER" + "xxx");

            // ���⿡ ���߿� UI ���â�� ���� �ڵ带 �߰��� �����Դϴ�.
        }
    }
}