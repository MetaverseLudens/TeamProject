using UnityEngine;
using UnityEngine.AI;

public class RandomPosGenerator : MonoBehaviour
{
    public float radius = 20f; // 랜덤 위치를 찾을 반지름
    public Transform centerPoint; // 중심점 지정 (예: 플레이어, 월드 중앙 등)

    public Vector3 GetPointFromNavMesh()
    {
        NavMeshTriangulation navMeshData = NavMesh.CalculateTriangulation();

        int triangleIndex = Random.Range(0, navMeshData.indices.Length / 3) * 3;

        Vector3 vert1 = navMeshData.vertices[navMeshData.indices[triangleIndex]];
        Vector3 vert2 = navMeshData.vertices[navMeshData.indices[triangleIndex + 1]];
        Vector3 vert3 = navMeshData.vertices[navMeshData.indices[triangleIndex + 2]];

        Vector3 randomPoint = RandomPointInTriangle(vert1, vert2, vert3);

        return randomPoint;
    }

    private Vector3 RandomPointInTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        float r1 = Random.value;
        float r2 = Random.value;

        if (r1 + r2 >= 1f)
        {
            r1 = 1f - r1;
            r2 = 1f - r2;
        }

        return a + r1 * (b - a) + r2 * (c - a);
    }

}
