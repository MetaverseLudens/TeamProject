using UnityEngine;
using UnityEngine.UI;

public class CharacterButton : MonoBehaviour
{
    public int characterId;             // ĳ���� ���� ID
    //public Image characterImage;        // UI ��ư�� ������ �̹���

    public void OnClick()
    {
        // �̹� ������ �����ߴ��� üũ�� RoomManager�� �� �ž�
        RoomManager.Instance.RequestCharacterSelection(characterId);
    }
}
