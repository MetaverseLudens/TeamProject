using Photon.Pun;
using UnityEngine;

public class Test : MonoBehaviourPun
{
    [SerializeField]
    private PhotonView _view;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _view.RequestOwnership();
        }
    }
}
