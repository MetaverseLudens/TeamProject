using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSlotUI : MonoBehaviour
{
    public TMP_Text nicknameText;
    public GameObject readyStatusText;
    public Image characterImage;

    public void SetNickname(string nickname)
    {
        nicknameText.text = nickname;
    }

    public void SetReady(bool ready, bool isMaster= false)
    {
        if (isMaster)
        {
            readyStatusText.SetActive(true);
            readyStatusText.GetComponent<TMP_Text>().text = "πÊ¿Â";
        }
        else
        {
            readyStatusText.SetActive(ready);
            readyStatusText.GetComponent<TMP_Text>().text = "Ready";
        }
    }
    public void SetCharacterSprite(Sprite sprite)
    {
        characterImage.sprite = sprite;
        characterImage.enabled = true;
    }
}
