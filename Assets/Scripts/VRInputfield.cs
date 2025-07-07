using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRInputfield : MonoBehaviour, IPointerClickHandler
{
    public GameObject virtualKeyboard;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (virtualKeyboard != null)
        {
            virtualKeyboard.GetComponent<HangulKeyborad>().korField = this.GetComponent<TMP_InputField>();
        }
    }
}
