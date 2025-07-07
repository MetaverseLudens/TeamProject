using Photon.Pun;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConsumeItemSpawner : MonoBehaviourPunCallbacks
{
    public static ConsumeItemSpawner Instance;
    [System.Serializable]
    public class ItemSpawnInfo
    {
        public string prefabName; // ex. "SpeedItem"
        public Transform[] spawnPoints;
        public int maxCount = 3;
    }

    public List<ItemSpawnInfo> itemTypes;
    public float spawnInterval = 5f;
    public int consumeItemCounts = 0;
    public int maxConsumeCount = 3;
    //private Dictionary<string, int> currentItemCounts = new Dictionary<string, int>();

    void Awake()
    {
        Instance = this;
        /*
        foreach (var item in itemTypes)
        {
            currentItemCounts[item.prefabName] = 0;
        }
        */
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

            // 랜덤 소비 아이템 하나 선택
            int index = 0;
            //int index = Random.Range(0, itemTypes.Count);
            var info = itemTypes[index];
            if (consumeItemCounts < maxConsumeCount)
            {
                Transform spawnPoint = info.spawnPoints[Random.Range(0, info.spawnPoints.Length)];
                PhotonNetwork.Instantiate(info.prefabName, spawnPoint.position, Quaternion.identity);
                consumeItemCounts++;
                Debug.Log("생성");
            }

            /*
            if (currentItemCounts[info.prefabName] < info.maxCount)
            {
                // 스폰 위치 랜덤 선택
                currentItemCounts[info.prefabName]++;
            }
            */
        }
    }

    public void DecreaseItemCount() //아이템 먹으면 호출
    {
        if (consumeItemCounts > 0) consumeItemCounts--;
    }
}
