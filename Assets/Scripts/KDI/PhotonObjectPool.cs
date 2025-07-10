using Photon.Pun;
using UnityEngine;
using System.Collections.Generic;

public class PhotonObjectPool : MonoBehaviour, IPunPrefabPool
{
    [System.Serializable]
    public class PoolInfo
    {
        public string prefabName;
        public GameObject prefab;
        public int preloadCount = 5; //미리 풀 오브젝트에 꺼내놓을 개수
    }

    public List<PoolInfo> poolList;

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        PhotonNetwork.PrefabPool = this; // Photon이 이 풀을 쓰게 만듦

        foreach (var pool in poolList)
        {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < pool.preloadCount; i++)
            {
                GameObject go = Instantiate(pool.prefab);
                go.name = pool.prefabName;
                go.SetActive(false);
                queue.Enqueue(go);
            }
            poolDict[pool.prefabName] = queue;
        }
    }

    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        GameObject go;

        if (poolDict.TryGetValue(prefabId, out var queue) && queue.Count > 0)
        {
            go = queue.Dequeue();
        }
        else
        {
            go = Instantiate(Resources.Load<GameObject>(prefabId));
            go.name = prefabId;
        }

        go.transform.SetPositionAndRotation(position, rotation);

        // Photon이 나중에 SetActive(true) 호출하므로 여기선 비활성 상태로 반환해야 함
        return go;
    }


    public void Destroy(GameObject go)
    {
        go.SetActive(false);
        if (poolDict.TryGetValue(go.name, out var queue))
        {
            queue.Enqueue(go);
        }
        else
        {
            UnityEngine.Object.Destroy(go);
        }
    }
}
