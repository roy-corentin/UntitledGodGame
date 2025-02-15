using UnityEditor;
using UnityEngine;
using Unity.AI.Navigation;

public class NavMeshHandler : MonoBehaviour
{
    public NavMeshSurface navmeshSurface;
    public static NavMeshHandler Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void GenerateNavmesh()
    {
        if (navmeshSurface == null)
        {
            Debug.LogError("NavmeshSurface is not assigned!");
            return;
        }

        navmeshSurface.BuildNavMesh();
    }

    public void ClearNavmesh()
    {
        if (navmeshSurface == null)
        {
            Debug.LogError("NavmeshSurface is not assigned!");
            return;
        }

        navmeshSurface.RemoveData();
    }

    public void Rebake()
    {
        ClearNavmesh();
        GenerateNavmesh();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(NavMeshHandler))]
public class NavMeshHandlerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        NavMeshHandler navMeshHandler = (NavMeshHandler)target;

        if (GUILayout.Button("Rebake Navmesh"))
            navMeshHandler.Rebake();
    }
}
#endif