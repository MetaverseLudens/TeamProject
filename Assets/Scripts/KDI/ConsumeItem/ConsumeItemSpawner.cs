using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsumeItemSpawner : MonoBehaviourPunCallbacks
{
    public static ConsumeItemSpawner Instance;
    public RandomPosGenerator generator;

    [System.Serializable]
    public class ItemSpawnInfo
    {
        public string prefabName;
        //public Transform[] spawnPoints;
    }
    [SerializeField] Vector3[] _itemSpawnPos; //이걸 위치를 정해줄지 아예 맵 전체에서 

    public List<ItemSpawnInfo> itemTypes;
    public float spawnInterval = 5f;
    //public int consumeItemCounts = 0;
    //public int maxConsumeCount = 3;

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(SpawnRoutine());
            Debug.Log("Master Client");
        }
        else
        {
            Debug.Log("방장 아님");
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            int index = Random.Range(0, itemTypes.Count);
            var info = itemTypes[index];
            //if (consumeItemCounts < maxConsumeCount) {
            Vector3 pos = generator.GetPointFromNavMesh();
            //Transform spawnPoint = info.spawnPoints[Random.Range(0, info.spawnPoints.Length)];
                PhotonNetwork.Instantiate(info.prefabName, pos, Quaternion.identity);
                //consumeItemCounts++;
                Debug.Log("생성");
            //}
        }
    }

    public void DecreaseItemCount() //아이템 먹으면 호출
    {
        //if (consumeItemCounts > 0) consumeItemCounts--;
    }
}
