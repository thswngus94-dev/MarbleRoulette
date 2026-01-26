using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    // 인스펙터에서 아래의 Text(TMP) 오브젝트를 이 칸에 드래그해서 넣어주세요!
    public TextMeshProUGUI nameText;
    public SpriteRenderer ballSprite;
    public SpriteRenderer minimapIcon;

    // 부모(구슬)가 회전해도 글자는 똑바로 보이게 유지
    void Update()
    {
        if (nameText != null)
        {
            // 구슬이 굴러가도 이름표는 똑바로 서있게 함
            nameText.transform.rotation = Quaternion.identity;
        }
    }

    public void SetInfo(string newName, Color randomColor)
    {
        if (nameText != null)
            nameText.text = newName;
            // 이름표 색상은 흰색으로 고정하거나, 구슬색과 맞출 수 있습니다.
            nameText.color = randomColor;

        if (ballSprite != null)
        {
            // 구슬의 색상을 랜덤 색상으로 변경
            ballSprite.color = randomColor;
        }

        
        if (minimapIcon != null)
        {
            minimapIcon.color = randomColor;
        }

    }

   

}