using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : MonoBehaviour
{
    public static CharacterSelectPanel Instance;
    public CharacterButton[] buttons;
    private int previousCharId = -1;

    private void Awake()
    {
        Instance = this;
    }
    public void EnableCharacterButton(int charId)
    {
        foreach (var btn in buttons)
        {
            if (btn.characterId == charId)
            {
                btn.GetComponent<Button>().interactable = true;
                btn.transform.GetChild(1).gameObject.SetActive(false);
                break;
            }
        }
    }
    public void DisableCharacterButton(int charId)
    {
        foreach (var btn in buttons)
        {
            if (btn.characterId == charId)
            {
                btn.GetComponent<Button>().interactable = false;
                btn.transform.GetChild(1).gameObject.SetActive(true);
                break;
            }
        }
    }
    public void UpdateCharacterButtons(int newCharId)
    {
        // ���� ĳ���� ��ư ����
        if (previousCharId != -1 && previousCharId != newCharId)
        {
            foreach (var btn in buttons)
            {
                if (btn.characterId == previousCharId)
                {
                    btn.GetComponent<Button>().interactable = true;
                    break;
                }
            }
        }

        // �� ĳ���� ��ư ��Ȱ��ȭ
        foreach (var btn in buttons)
        {
            if (btn.characterId == newCharId)
            {
                btn.GetComponent<Button>().interactable = false;
                break;
            }
        }

        previousCharId = newCharId;
    }
}