using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;

public class XRInputfieldManager : MonoBehaviour
{
    public GameObject keyboardObject;         // Ű���� ������Ʈ ��ü
    public HangulKeyborad hangulKeyboard;     // Ű���� ��ũ��Ʈ

    void Update()
    {
        var selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null)
        {
            // InputField �����ٸ� Ű���� ����
            if (selected.GetComponent<InputField>() != null)
                return;

            // ���� ���õ� ������Ʈ�� Ű���� ���� ��ư�̸� ����
            if (selected.transform.IsChildOf(keyboardObject.transform))
                return;
        }

        // �� ���� ��� Ű���� �ݱ�
        if (keyboardObject.activeSelf)
            keyboardObject.SetActive(false);
    }
}
