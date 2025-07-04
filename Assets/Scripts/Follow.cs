using Photon.Pun;
using UnityEngine;

public class Follow : MonoBehaviourPun
{
    [SerializeField]
    private Transform _targetTrs;

    private void Update()
    {
        if (photonView.IsMine == false)
            return;

        transform.position = _targetTrs.position;
    }
}
