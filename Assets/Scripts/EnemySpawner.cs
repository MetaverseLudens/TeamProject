using Photon.Pun;
using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    private float _spawnTick;

    public void StartSpawn()
    {
        StartCoroutine(nameof(CoUpdate));
    }

    private IEnumerator CoUpdate()
    {
        yield return new WaitUntil(() => PhotonNetwork.LocalPlayer.IsMasterClient);

        while (true)
        {
            var prefabName = "Enemy";
            var pos = new Vector3(Random.Range(-5, 5), 0, Random.Range(-5, 5));
            PhotonNetwork.InstantiateRoomObject(prefabName, pos, Quaternion.identity);
            yield return new WaitForSeconds(_spawnTick);
        }
    }
}
