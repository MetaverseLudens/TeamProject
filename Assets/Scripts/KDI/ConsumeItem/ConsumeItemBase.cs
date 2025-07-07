using UnityEngine;
using Photon.Pun;

public abstract class ConsumeItemBase : MonoBehaviourPun
{
    public abstract void OnConsumed(GameObject player);
}
