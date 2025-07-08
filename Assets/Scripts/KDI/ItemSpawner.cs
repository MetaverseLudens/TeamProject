using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class ItemSpawner : MonoBehaviour, IPunPrefabPool
{
    public static ItemSpawner Instance;

    [System.Serializable]
    public class PoolInfo
    {
        public string prefabName;
        public GameObject prefab;
        public int prewarmCount = 5;
    }

    public List<PoolInfo> pools;

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this;
        PhotonNetwork.PrefabPool = this; // Photon에 등록
        InitializePools();
    }

    void InitializePools()
    {
        foreach (var pool in pools)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.prewarmCount; i++)
            {
                var obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDict[pool.prefabName] = queue;
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 pos, Quaternion rot)
    {
        if (poolDict.ContainsKey(prefabId))
        {
            if (poolDict[prefabId].Count > 0)
            {
                var obj = poolDict[prefabId].Dequeue();
                obj.transform.SetPositionAndRotation(pos, rot);
                obj.SetActive(true);
                return obj;
            }
        }

        // 풀에 없으면 새로 인스턴스 (권장 안함)
        return Instantiate(Resources.Load<GameObject>(prefabId), pos, rot);
    }

    public void Destroy(GameObject go)
    {
        go.SetActive(false);
        poolDict[go.name].Enqueue(go); // name이 prefab name과 같다고 가정
    }
}
