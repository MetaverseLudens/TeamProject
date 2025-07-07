using Photon.Pun;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RoomExitUI: MonoBehaviourPunCallbacks
{
    public Button leaveButton;

    private void Start()
    {
        leaveButton.onClick.AddListener(() =>
        {
            if (PhotonNetwork.InRoom)
            {
                PhotonNetwork.LeaveRoom();
            }
        });
    }
    public override void OnLeftRoom()
    {
        // �� ��ȯ�� ���⼭ ó��
        SceneFlowTracker.ReturnedFromRoom = true;
        SceneManager.LoadScene(MyString.SCENE_LOBBY);
    }
}
