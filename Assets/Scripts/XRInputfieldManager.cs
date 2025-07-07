using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit.UI;
using UnityEngine.UI;

public class XRInputfieldManager : MonoBehaviour
{
    public GameObject keyboardObject;         // 키보드 오브젝트 전체
    public HangulKeyborad hangulKeyboard;     // 키보드 스크립트

    void Update()
    {
        var selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null)
        {
            // InputField 눌렀다면 키보드 유지
            if (selected.GetComponent<InputField>() != null)
                return;

            // 현재 선택된 오브젝트가 키보드 내부 버튼이면 유지
            if (selected.transform.IsChildOf(keyboardObject.transform))
                return;
        }

        // 그 외의 경우 키보드 닫기
        if (keyboardObject.activeSelf)
            keyboardObject.SetActive(false);
    }
}
