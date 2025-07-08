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
            Debug.Log("���� �ƴ�");
        }
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            // ���� �Һ� ������ �ϳ� ����
            int index = 0;
            //int index = Random.Range(0, itemTypes.Count);
            var info = itemTypes[index];
            if (consumeItemCounts < maxConsumeCount)
            {
                Transform spawnPoint = info.spawnPoints[Random.Range(0, info.spawnPoints.Length)];
                PhotonNetwork.Instantiate(info.prefabName, spawnPoint.position, Quaternion.identity);
                consumeItemCounts++;
                Debug.Log("����");
            }

            /*
            if (currentItemCounts[info.prefabName] < info.maxCount)
            {
                // ���� ��ġ ���� ����
                currentItemCounts[info.prefabName]++;
            }
            */
        }
    }

    public void DecreaseItemCount() //������ ������ ȣ��
    {
        if (consumeItemCounts > 0) consumeItemCounts--;
    }
}
