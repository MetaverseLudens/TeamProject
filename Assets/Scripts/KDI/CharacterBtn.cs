using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public int characterId;             // 캐릭터 고유 ID
    //public Image characterImage;        // UI 버튼에 보여줄 이미지

    public void OnClick()
    {
        // 이미 누군가 선택했는지 체크는 RoomManager가 할 거야
        RoomManager.Instance.RequestCharacterSelection(characterId);
    }
}
