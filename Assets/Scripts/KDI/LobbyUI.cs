using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] GameObject _introUI;
    [SerializeField] GameObject _photonUI;
    [SerializeField] GameObject _roomListUI;
    [SerializeField] GameObject _roomNameUI;
    [SerializeField] GameObject _optionUI;

    [SerializeField] TMP_InputField _nicknameInput;
    [SerializeField] Button _confirmBtn;
    private void Start()
    {
        if (SceneFlowTracker.ReturnedFromRoom)
        {
            _introUI.SetActive(false);
            _roomListUI.SetActive(true);
        }
    }
    public void BtnGameStart()
    {
        if (_photonUI != null)
        {
            _photonUI.SetActive(true);
            _introUI.SetActive(false);
        }
        else Debug.Log("_photonUI is null");
    }
    public void BtnOption()
    {
        if (_optionUI != null) _optionUI.SetActive(true);
        else Debug.Log("_optionUI is null");
    }
    public void BtnExit()
    {
        Application.Quit();
    }

    public void BtnConfirm()
    {
        _confirmBtn.interactable = false;
        string nickname = _nicknameInput.text;
        if (string.IsNullOrEmpty(_nicknameInput.text))
        {
            nickname = $"USER_{Random.Range(0, 100):00}";
            _nicknameInput.text = nickname;
        }
        PhotonNetwork.NickName = nickname;
        Invoke("ShowRoom", 1f);
    }
    void ShowRoom()
    {
        _confirmBtn.interactable = true;
        _photonUI.SetActive(false);
        _roomListUI.SetActive(true);
    }
}
