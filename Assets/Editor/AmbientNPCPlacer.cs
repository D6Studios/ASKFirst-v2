using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

/// Places a few silent background customers (AmbientNPC) on free spots of each level's NavMesh.
/// Clones the level's existing NPCs so they look like the rest of the cast. Safe to re-run: old ones are replaced.
public static class AmbientNPCPlacer
{
    const int PerLevel = 3;
    const int StopsPerRoute = 3; // spawn spot + 2 more nearby
    const string WaypointRoot = "Background Customer Waypoints";
    static readonly string[] Scenes = { "Assets/Scenes/Level2.unity", "Assets/Scenes/Level3.unity" };

    [MenuItem("Tools/ASK First/Place Ambient NPCs (Level 2 + 3)")]
    public static void Run()
    {
        // Opening the level scenes replaces what's open, so let the user save first, then reopen it after.
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        string previous = SceneManager.GetActiveScene().path;
        foreach (string path in Scenes) PlaceIn(path);
        if (!string.IsNullOrEmpty(previous)) EditorSceneManager.OpenScene(previous);
    }

    static void PlaceIn(string scenePath)
    {
        var scene = EditorSceneManager.OpenScene(scenePath);
        foreach (var old in Object.FindObjectsByType<AmbientNPC>(FindObjectsInactive.Include))
            Object.DestroyImmediate(old.gameObject);
        var oldWaypoints = GameObject.Find(WaypointRoot);
        if (oldWaypoints != null) Object.DestroyImmediate(oldWaypoints);
        var waypointRoot = new GameObject(WaypointRoot);
        SceneManager.MoveGameObjectToScene(waypointRoot, scene);

        var npcs = Object.FindObjectsByType<NPCBehavior>().OrderBy(n => n.name).ToList();
        var player = GameObject.FindGameObjectWithTag("Player").transform;
        var exit = GameObject.FindGameObjectWithTag("Exit").transform;

        // Spots to stay away from: player start, exit, existing NPCs and their patrol routes.
        var avoid = new List<Vector3> { player.position, exit.position };
        foreach (var n in npcs)
        {
            avoid.Add(n.transform.position);
            var pts = new SerializedObject(n).FindProperty("patrolPoints");
            for (int i = 0; i < pts.arraySize; i++)
                if (pts.GetArrayElementAtIndex(i).objectReferenceValue is Transform t) avoid.Add(t.position);
        }

        var tri = NavMesh.CalculateTriangulation();
        if (tri.indices.Length == 0) { Debug.LogError("[AmbientNPCPlacer] No NavMesh in " + scenePath); return; }

        var rng = new System.Random(scenePath.GetHashCode());
        var chosen = new List<Vector3>();
        for (int attempt = 0; attempt < 5000 && chosen.Count < PerLevel; attempt++)
        {
            int t = rng.Next(tri.indices.Length / 3) * 3;
            Vector3 p = (tri.vertices[tri.indices[t]] + tri.vertices[tri.indices[t + 1]] + tri.vertices[tri.indices[t + 2]]) / 3f;
            if (Mathf.Abs(p.y - player.position.y) > 0.5f) continue; // floor level only, not shelf tops
            if (!NavMesh.SamplePosition(p, out var hit, 0.3f, NavMesh.AllAreas)) continue;
            // Need open floor around it so it doesn't block an aisle.
            if (NavMesh.FindClosestEdge(hit.position, out var edge, NavMesh.AllAreas) && edge.distance < 0.6f) continue;
            if (avoid.Any(a => Vector3.Distance(a, hit.position) < 2.5f)) continue;
            if (chosen.Any(c => Vector3.Distance(c, hit.position) < 4f)) continue;
            chosen.Add(hit.position);
        }

        for (int i = 0; i < chosen.Count; i++)
        {
            var src = npcs[i % npcs.Count].gameObject;
            var go = (GameObject)PrefabUtility.InstantiatePrefab(PrefabUtility.GetCorrespondingObjectFromSource(src), scene);
            PrefabUtility.SetPropertyModifications(go, PrefabUtility.GetPropertyModifications(src)); // keep the source NPC's look
            // Some shoplifters (Level 3) swap in a scene-added model and switch the default body off; added
            // objects aren't copied, so fall back to the prefab's normal body (also avoids the shoplifter look).
            if (go.GetComponentInChildren<SkinnedMeshRenderer>() == null)
                foreach (Transform child in go.transform)
                    if (child.GetComponentInChildren<SkinnedMeshRenderer>(true) != null) { child.gameObject.SetActive(true); break; }
            if (go.GetComponentInChildren<SkinnedMeshRenderer>() == null) Debug.LogError("[AmbientNPCPlacer] " + go.name + " has no visible model");
            go.name = "Customer (Background) " + (i + 1);
            go.transform.SetPositionAndRotation(chosen[i], Quaternion.Euler(0, rng.Next(360), 0));

            var b = new SerializedObject(go.GetComponent<NPCBehavior>());
            b.FindProperty("isShoplifter").boolValue = false;
            b.FindProperty("walkOnceThenStay").boolValue = false;
            // Short browsing loop: wander between nearby reachable spots, pausing a few seconds at each.
            var route = PickRoute(chosen[i], avoid, rng);
            var pts = b.FindProperty("patrolPoints");
            pts.arraySize = route.Count;
            for (int k = 0; k < route.Count; k++)
            {
                var wp = new GameObject(go.name + " Stop " + (k + 1)).transform;
                wp.SetParent(waypointRoot.transform);
                wp.position = route[k];
                pts.GetArrayElementAtIndex(k).objectReferenceValue = wp;
            }
            b.FindProperty("idleWaitTime").vector2Value = new Vector2(3f, 8f);
            b.ApplyModifiedPropertiesWithoutUndo();
            var dialogue = go.GetComponent<NPCDialogue>();
            if (dialogue != null) Object.DestroyImmediate(dialogue);
            go.AddComponent<AmbientNPC>();
            Debug.Log($"[AmbientNPCPlacer] {scenePath}: {go.name} at {chosen[i]} (look from {src.name})");
        }
        if (chosen.Count < PerLevel) Debug.LogWarning($"[AmbientNPCPlacer] Only found {chosen.Count} spots in {scenePath}");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // Spawn spot plus nearby open-floor spots 3-7 m away that the NavMesh can actually walk to.
    static List<Vector3> PickRoute(Vector3 start, List<Vector3> avoid, System.Random rng)
    {
        var route = new List<Vector3> { start };
        var path = new NavMeshPath();
        for (int attempt = 0; attempt < 300 && route.Count < StopsPerRoute; attempt++)
        {
            float angle = (float)(rng.NextDouble() * Mathf.PI * 2), dist = 3f + (float)rng.NextDouble() * 4f;
            var p = start + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * dist;
            if (!NavMesh.SamplePosition(p, out var hit, 0.5f, NavMesh.AllAreas)) continue;
            if (Mathf.Abs(hit.position.y - start.y) > 0.3f) continue;
            if (NavMesh.FindClosestEdge(hit.position, out var edge, NavMesh.AllAreas) && edge.distance < 0.5f) continue;
            if (avoid.Any(a => Vector3.Distance(a, hit.position) < 1.5f)) continue;
            if (route.Any(r => Vector3.Distance(r, hit.position) < 2.5f)) continue;
            if (!NavMesh.CalculatePath(start, hit.position, NavMesh.AllAreas, path) || path.status != NavMeshPathStatus.PathComplete) continue;
            route.Add(hit.position);
        }
        return route;
    }
}
