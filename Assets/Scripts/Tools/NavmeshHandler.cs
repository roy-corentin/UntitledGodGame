using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Unity.AI;
using Unity.AI.Navigation;
using MapGenerator;
using UnityEngine.AI;

public static class NavmeshHandler
{
    //public static void GenerateNavmesh()
    //{
    //    if(Map.Instance.navmeshSurface != null){
    //        Debug.LogWarning("Already a navmeshSurface on this map! Overriding it in order to generate navmesh");
    //        ClearNavmesh();
    //    }

    //    Map.Instance.navmeshSurface = Map.Instance.meshMap.gameObject.AddComponent<NavMeshSurface>();

    //    // si besoin de modifier la generation, notamment pour une surface type 1x1m -> scene AR:
    //    // -- attention: toutes les valeurs ont des minimums qui ne sont pas si petits que ce l'on peut esperer, si on indique une valeur inferieure au min, Unity ignore
    //    // -- voir https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/NavMeshSurface.html#parameters section "Advanced Settings"
    //    // 
    //    //navmeshSurface.overrideVoxelSize = true;
    //    //navmeshSurface.voxelSize = ...
    //    //navmeshSurface.overrideTileSize = true;
    //    //navmeshSurface.tileSize = ... ;
    //    //navmeshSurface.minRegionArea = ...;0


    //    // on veut pas inclure le GameObject correspondant a l'eau (Map>Water) dans le bake sinon on pourrait marcher sur l'eau
    //    // donc on ne prend que meshMap pour construire correctement la navmesh
    //    Map.Instance.navmeshSurface.collectObjects = CollectObjects.Children;

    //    Map.Instance.navmeshUnderseaObstacleGO = new GameObject("Navmesh Undersea Obstacle");

    //    Map.Instance.navmeshUnderseaObstacleGO.transform.SetParent(Map.Instance.gameObject.transform);

    //    Map.Instance.navmeshUnderseaObstacle = Map.Instance.navmeshUnderseaObstacleGO.AddComponent<NavMeshObstacle>();
    //    Map.Instance.navmeshUnderseaObstacle.shape = NavMeshObstacleShape.Box;

    //    Map.Instance.navmeshUnderseaObstacle.size = ARtoVR.Instance.VRMapSize;

    //    // Debug.Log("BiomeManager.Instance.waterHeight: " + BiomeManager.Instance.waterHeight);
    //    // semble etre de 0.28 donc un offset de 0.05 par rapport à la hauteur relle de l'eau dans le jeu qui semble etre a 0.23 ? (en negatif)
    //    // todo: remplacer magic value de -0.23 par un truc dynamique partant de BiomeManager.Instance.waterHeight
    //    // Map.Instance.navmeshUnderseaObstacle.center = new Vector3(0, -0.23f, 0);
    //    // on doit probablement ajuster par rapport au scale VR
    //    Map.Instance.navmeshUnderseaObstacle.center = new Vector3(0, (-0.23f*ARtoVR.Instance.VRMapSize.x), 0);

    //    Map.Instance.navmeshUnderseaObstacle.carving = true;

    //    // deuxieme parametre permet de garder la position world (=/= local pos) (?)
    //    Map.Instance.navmeshUnderseaObstacleGO.transform.SetParent(Map.Instance.meshMap.gameObject.transform, true);
    //    Vector3 obsPos = Map.Instance.navmeshUnderseaObstacleGO.transform.localPosition;
    //    Map.Instance.navmeshUnderseaObstacleGO.transform.localPosition = new Vector3(obsPos.x, 0, obsPos.z);

    //    Map.Instance.navmeshSurface.BuildNavMesh();

    //    }

    public static void GenerateNavmesh()
    {
        if (Map.Instance.navmeshSurface != null)
        {
            Debug.LogWarning("Already a navmeshSurface on this map! Overriding it in order to generate navmesh");
            ClearNavmesh();
        }

        Map.Instance.navmeshSurface = Map.Instance.meshMap.gameObject.AddComponent<NavMeshSurface>();

        // si besoin de modifier la generation, notamment pour une surface type 1x1m -> scene AR:
        // -- attention: toutes les valeurs ont des minimums qui ne sont pas si petits que ce l'on peut esperer, si on indique une valeur inferieure au min, Unity ignore
        // -- voir https://docs.unity3d.com/Packages/com.unity.ai.navigation@1.1/manual/NavMeshSurface.html#parameters section "Advanced Settings"
        // 
        //navmeshSurface.overrideVoxelSize = true;
        //navmeshSurface.voxelSize = ...
        //navmeshSurface.overrideTileSize = true;
        //navmeshSurface.tileSize = ... ;
        //navmeshSurface.minRegionArea = ...;

        Map.Instance.navmeshSurface.BuildNavMesh();
    }

    public static void ClearNavmesh()
    {
        if (Map.Instance == null)
        {
            Debug.LogWarning("Couldn't clear navmesh: parentMap is null");
            return;
        }

        if (Map.Instance.navmeshSurface == null)
        {
            Debug.LogWarning("Couldn't clear navmesh: no navmeshSurface assigned to Map object");
            return;
        }

        #if UNITY_EDITOR
            Map.DestroyImmediate(Map.Instance.navmeshSurface);
        #else
            Destroy(Map.Instance.navmeshSurface);
        #endif
    }

    public static void Rebake()
        {
            if(Map.Instance.navmeshSurface != null)
                ClearNavmesh();

            Map.Instance.navmeshSurface.BuildNavMesh();
        }
}