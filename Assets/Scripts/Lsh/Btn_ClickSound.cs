using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Btn_ClickSound : MonoBehaviour
{
    private Button _btn;

    [SerializeField]
    private AudioClip _clickClip;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(()=> SoundManager.Instance.PlaySfx(_clickClip));
    }
}
