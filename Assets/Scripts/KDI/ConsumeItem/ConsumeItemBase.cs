using UnityEngine;
using Photon.Pun;

public abstract class ConsumeItemBase : MonoBehaviourPun
{
    [SerializeField]
    protected AudioSource _audioSrc;

    public abstract void OnConsumed(GameObject player);
}
