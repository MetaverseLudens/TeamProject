using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshSurface))]
public class NavmeshCollider : MonoBehaviour
{
    void Start()
    {
        // NavMesh �ﰢ�� ������ ��������
        NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();

        // �� Mesh ����
        Mesh mesh = new Mesh();
        mesh.vertices = triangulation.vertices;
        mesh.triangles = triangulation.indices;
        mesh.RecalculateNormals();

        // GameObject �����Ͽ� MeshCollider ����
        GameObject navMeshColliderObject = new GameObject("NavMeshCollider");
        navMeshColliderObject.transform.position = Vector3.zero;

        MeshCollider meshCollider = navMeshColliderObject.AddComponent<MeshCollider>();
        meshCollider.sharedMesh = mesh;
        meshCollider.convex = false; // �Ϲ� �ٴ��� ��� convex�� false

        // �ð������� Ȯ���ϰ� �ʹٸ� MeshRenderer�� �߰�
        MeshFilter mf = navMeshColliderObject.AddComponent<MeshFilter>();
        MeshRenderer mr = navMeshColliderObject.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.material = new Material(Shader.Find("Standard"));
    }
}
