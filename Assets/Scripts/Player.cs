using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // 인스펙터에서 아래의 Text(TMP) 오브젝트를 이 칸에 드래그해서 넣어주세요!
    public TextMeshProUGUI nameText;

    // 부모(구슬)가 회전해도 글자는 똑바로 보이게 유지
    void Update()
    {
        if (nameText != null)
        {
            nameText.transform.rotation = Quaternion.identity;
        }
    }

    public void SetName(string newName)
    {
        if (nameText != null)
            nameText.text = newName;
    }
}