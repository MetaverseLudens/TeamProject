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
        public int preloadCount = 5; //�̸� Ǯ ������Ʈ�� ��������
    }

    public List<PoolInfo> poolList;

    private Dictionary<string, Queue<GameObject>> poolDict = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        PhotonNetwork.PrefabPool = this; // Photon�� �� Ǯ�� ���� ����

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
        if (poolDict.TryGetValue(prefabId, out var queue) && queue.Count > 0)
        {
            GameObject go = queue.Dequeue();
            go.transform.SetPositionAndRotation(position, rotation);
            go.SetActive(true);
            return go;
        }

        // ���� ������ ���� ����
        GameObject newObj = GameObject.Instantiate(Resources.Load<GameObject>(prefabId), position, rotation);
        newObj.name = prefabId;
        return newObj;
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
            GameObject.Destroy(go); // Ȥ�� Ǯ�� ��� �ȵ� ������Ʈ�� ���
        }
    }
}
