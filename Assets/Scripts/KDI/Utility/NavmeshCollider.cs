using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshCollider : MonoBehaviour
{
    void Start()
    {
        // NavMesh 삼각형 데이터 가져오기
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        // 새 Mesh 생성
        Mesh mesh = new Mesh();
        mesh.vertices = triangulation.vertices;
        mesh.triangles = triangulation.indices;
        mesh.RecalculateNormals();

        // GameObject 생성하여 MeshCollider 부착
        GameObject navMeshColliderObject = new GameObject("NavMeshCollider");
        navMeshColliderObject.transform.position = Vector3.zero;

        MeshCollider meshCollider = navMeshColliderObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false; // 일반 바닥일 경우 convex는 false

        // 시각적으로 확인하고 싶다면 MeshRenderer도 추가
        MeshFilter mf = navMeshColliderObject.AddComponent<MeshFilter>();
        MeshRenderer mr = navMeshColliderObject.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.material = new Material(Shader.Find("Standard"));
    }
}
