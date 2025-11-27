using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace EditorTools
{
    /// <summary>
    /// Editor tool to intelligently place GameObjects and their children on the ground using raycasts.
    /// </summary>
    public class PlaceOnGroundTool : EditorWindow
    {
        private GameObject targetObject;
        private LayerMask groundLayers = ~0; // All layers by default
        private float raycastDistance = 1000f;
        private bool placeChildren = true;
        private int hierarchyDepth = 1;
        private bool filterSiblings = true;
        private bool preserveRelativePositions = true;
        private bool useColliders = true;
        private float yOffset = 0f;
        private bool autoRecordUndo = true;

        [MenuItem("Tools/Place On Ground")]
        public static void ShowWindow()
        {
            PlaceOnGroundTool window = GetWindow<PlaceOnGroundTool>("Place On Ground");
            window.minSize = new Vector2(350, 300);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Place Objects On Ground", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox(
                "This tool places GameObjects on the ground using raycasts. " +
                "It intelligently handles hierarchies and preserves relative positions.",
                MessageType.Info);

            EditorGUILayout.Space();

            // Target object
            targetObject = (GameObject)EditorGUILayout.ObjectField(
                "Target Object",
                targetObject,
                typeof(GameObject),
                true);

            EditorGUILayout.Space();

            // Settings
            GUILayout.Label("Settings", EditorStyles.boldLabel);

            placeChildren = EditorGUILayout.Toggle(
                new GUIContent("Place Children", "Place all child objects on ground individually"),
                placeChildren);

            EditorGUI.BeginDisabledGroup(!placeChildren);
            hierarchyDepth = EditorGUILayout.IntSlider(
                new GUIContent("Hierarchy Depth", "How many levels of children to process (1 = direct children only, -1 = all levels)"),
                hierarchyDepth,
                -1,
                10);
            
            filterSiblings = EditorGUILayout.Toggle(
                new GUIContent("Filter Siblings", "Ignore sibling objects when raycasting to prevent stacking"),
                filterSiblings);
            EditorGUI.EndDisabledGroup();

            preserveRelativePositions = EditorGUILayout.Toggle(
                new GUIContent("Preserve Relative Positions", "Maintain hierarchy structure while placing on ground"),
                preserveRelativePositions);

            useColliders = EditorGUILayout.Toggle(
                new GUIContent("Use Colliders", "Account for collider bounds when placing objects"),
                useColliders);

            yOffset = EditorGUILayout.FloatField(
                new GUIContent("Y Offset", "Additional vertical offset after placement"),
                yOffset);

            groundLayers = LayerMaskField(
                new GUIContent("Ground Layers", "Which layers count as 'ground'"),
                groundLayers);

            raycastDistance = EditorGUILayout.FloatField(
                new GUIContent("Raycast Distance", "Maximum distance to check for ground"),
                raycastDistance);

            autoRecordUndo = EditorGUILayout.Toggle(
                new GUIContent("Auto Record Undo", "Automatically record undo for changes"),
                autoRecordUndo);

            EditorGUILayout.Space();

            // Buttons
            EditorGUI.BeginDisabledGroup(targetObject == null);

            if (GUILayout.Button("Place On Ground", GUILayout.Height(30)))
            {
                PlaceObjectsOnGround();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();

            // Quick actions for selected objects
            if (Selection.gameObjects.Length > 0)
            {
                EditorGUILayout.HelpBox(
                    $"{Selection.gameObjects.Length} object(s) selected in hierarchy",
                    MessageType.None);

                if (GUILayout.Button("Place Selected Objects", GUILayout.Height(25)))
                {
                    PlaceSelectedObjects();
                }
            }
        }

        private void PlaceObjectsOnGround()
        {
            if (targetObject == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a target object.", "OK");
                return;
            }

            if (autoRecordUndo)
            {
                Undo.RecordObject(targetObject.transform, "Place On Ground");
            }

            if (placeChildren)
            {
                PlaceHierarchyOnGround(targetObject);
            }
            else
            {
                PlaceSingleObjectOnGround(targetObject);
            }

            EditorUtility.DisplayDialog("Success", "Objects placed on ground successfully!", "OK");
        }

        private void PlaceSelectedObjects()
        {
            int successCount = 0;
            int failCount = 0;

            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj != null)
                {
                    if (autoRecordUndo)
                    {
                        Undo.RecordObject(obj.transform, "Place On Ground");
                    }

                    if (placeChildren)
                    {
                        if (PlaceHierarchyOnGround(obj))
                            successCount++;
                        else
                            failCount++;
                    }
                    else
                    {
                        if (PlaceSingleObjectOnGround(obj))
                            successCount++;
                        else
                            failCount++;
                    }
                }
            }

            string message = $"Placed {successCount} object(s) on ground.";
            if (failCount > 0)
            {
                message += $"\n{failCount} object(s) failed (no ground found).";
            }

            EditorUtility.DisplayDialog("Complete", message, "OK");
        }

        private bool PlaceHierarchyOnGround(GameObject rootObject)
        {
            // Get transforms to process based on depth setting
            List<Transform> transformsToProcess = GetTransformsAtDepth(rootObject.transform, hierarchyDepth);

            if (preserveRelativePositions)
            {
                // Place root first, then adjust children relative to root
                bool rootPlaced = PlaceSingleObjectOnGround(rootObject, transformsToProcess);

                foreach (Transform t in transformsToProcess)
                {
                    if (t != rootObject.transform)
                    {
                        if (autoRecordUndo)
                        {
                            Undo.RecordObject(t, "Place Child On Ground");
                        }
                        PlaceSingleObjectOnGround(t.gameObject, transformsToProcess);
                    }
                }

                return rootPlaced;
            }
            else
            {
                // Place each object independently
                bool anySuccess = false;

                foreach (Transform t in transformsToProcess)
                {
                    if (autoRecordUndo)
                    {
                        Undo.RecordObject(t, "Place On Ground");
                    }

                    if (PlaceSingleObjectOnGround(t.gameObject, transformsToProcess))
                    {
                        anySuccess = true;
                    }
                }

                return anySuccess;
            }
        }

        private List<Transform> GetTransformsAtDepth(Transform root, int maxDepth)
        {
            List<Transform> result = new List<Transform>();
            result.Add(root); // Always include root

            if (maxDepth == -1)
            {
                // Get all children recursively
                result.AddRange(root.GetComponentsInChildren<Transform>());
            }
            else if (maxDepth > 0)
            {
                // Get children up to specified depth
                GetChildrenRecursive(root, 0, maxDepth, result);
            }

            return result;
        }

        private void GetChildrenRecursive(Transform parent, int currentDepth, int maxDepth, List<Transform> result)
        {
            if (currentDepth >= maxDepth)
                return;

            foreach (Transform child in parent)
            {
                result.Add(child);
                GetChildrenRecursive(child, currentDepth + 1, maxDepth, result);
            }
        }

        private bool PlaceSingleObjectOnGround(GameObject obj, List<Transform> siblingTransforms = null)
        {
            Vector3 objectPosition = obj.transform.position;
            
            // Collect all colliders to ignore from this object and its children
            List<Collider> collidersToIgnoreList = new List<Collider>(obj.GetComponentsInChildren<Collider>());
            
            // Add sibling colliders if filtering is enabled
            if (filterSiblings && siblingTransforms != null)
            {
                foreach (Transform sibling in siblingTransforms)
                {
                    if (sibling != null && sibling != obj.transform)
                    {
                        collidersToIgnoreList.AddRange(sibling.GetComponentsInChildren<Collider>());
                    }
                }
            }
            
            Collider[] collidersToIgnore = collidersToIgnoreList.ToArray();

            // Start raycast from high above the object position
            Vector3 rayStart = new Vector3(
                objectPosition.x,
                objectPosition.y + raycastDistance * 0.5f,
                objectPosition.z);

            RaycastHit hit = default(RaycastHit);
            bool hitFound = false;

            // Raycast downward, ignoring hits on the object itself
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, raycastDistance, groundLayers);
            
            // Sort hits by distance to get the first valid ground hit
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (RaycastHit potentialHit in hits)
            {
                // Check if this hit is one of our own colliders
                bool isOwnCollider = false;
                foreach (Collider col in collidersToIgnore)
                {
                    if (col != null && potentialHit.collider == col)
                    {
                        isOwnCollider = true;
                        break;
                    }
                }

                // If this isn't our own collider, use it as ground
                if (!isOwnCollider)
                {
                    hit = potentialHit;
                    hitFound = true;
                    break;
                }
            }

            if (hitFound)
            {
                // Calculate the offset from the object's pivot to its bottom
                float bottomOffset = 0f;
                if (useColliders)
                {
                    Collider collider = obj.GetComponent<Collider>();
                    if (collider != null)
                    {
                        // Calculate how far below the pivot the bottom of the collider is
                        bottomOffset = obj.transform.position.y - collider.bounds.min.y;
                    }
                    else
                    {
                        Renderer renderer = obj.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            // Calculate how far below the pivot the bottom of the renderer is
                            bottomOffset = obj.transform.position.y - renderer.bounds.min.y;
                        }
                    }
                }

                // Place object so its bottom sits on the ground
                // New pivot Y = ground Y + distance from pivot to bottom + offset
                float newY = hit.point.y + bottomOffset + yOffset;
                Vector3 newPosition = new Vector3(
                    obj.transform.position.x,
                    newY,
                    obj.transform.position.z);

                obj.transform.position = newPosition;

                Debug.Log($"Placed '{obj.name}' on ground at Y={newY:F2} (hit: {hit.collider.name}, bottom offset: {bottomOffset:F2})");
                return true;
            }
            else
            {
                Debug.LogWarning($"No ground found for '{obj.name}' at position {objectPosition}");
                return false;
            }
        }

        // Custom LayerMask field
        private LayerMask LayerMaskField(GUIContent label, LayerMask layerMask)
        {
            List<string> layers = new List<string>();
            List<int> layerNumbers = new List<int>();

            for (int i = 0; i < 32; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    layers.Add(layerName);
                    layerNumbers.Add(i);
                }
            }

            int maskWithoutEmpty = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if (((1 << layerNumbers[i]) & layerMask.value) != 0)
                {
                    maskWithoutEmpty |= (1 << i);
                }
            }

            maskWithoutEmpty = EditorGUILayout.MaskField(label, maskWithoutEmpty, layers.ToArray());

            int mask = 0;
            for (int i = 0; i < layerNumbers.Count; i++)
            {
                if ((maskWithoutEmpty & (1 << i)) != 0)
                {
                    mask |= (1 << layerNumbers[i]);
                }
            }

            layerMask.value = mask;
            return layerMask;
        }

        [MenuItem("GameObject/Place On Ground %g", false, 0)]
        private static void PlaceOnGroundContextMenu()
        {
            if (Selection.gameObjects.Length == 0)
            {
                EditorUtility.DisplayDialog("Error", "Please select at least one GameObject.", "OK");
                return;
            }

            foreach (GameObject obj in Selection.gameObjects)
            {
                if (obj != null)
                {
                    Undo.RecordObject(obj.transform, "Place On Ground");
                    QuickPlaceOnGround(obj);
                }
            }
        }

        private static void QuickPlaceOnGround(GameObject obj)
        {
            Vector3 objectPosition = obj.transform.position;

            // Collect all colliders to ignore from this object and its children
            Collider[] collidersToIgnore = obj.GetComponentsInChildren<Collider>();

            // Start raycast from high above
            Vector3 rayStart = new Vector3(
                objectPosition.x,
                objectPosition.y + 500f,
                objectPosition.z);

            // Raycast downward
            RaycastHit[] hits = Physics.RaycastAll(rayStart, Vector3.down, 1000f);
            
            RaycastHit hit = default(RaycastHit);
            bool hitFound = false;

            // Sort hits by distance
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (RaycastHit potentialHit in hits)
            {
                // Check if this hit is one of our own colliders
                bool isOwnCollider = false;
                foreach (Collider col in collidersToIgnore)
                {
                    if (col != null && potentialHit.collider == col)
                    {
                        isOwnCollider = true;
                        break;
                    }
                }

                // If this isn't our own collider, use it
                if (!isOwnCollider)
                {
                    hit = potentialHit;
                    hitFound = true;
                    break;
                }
            }

            if (hitFound)
            {
                // Calculate the offset from the object's pivot to its bottom
                float bottomOffset = 0f;
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null)
                {
                    // Calculate how far below the pivot the bottom of the collider is
                    bottomOffset = obj.transform.position.y - collider.bounds.min.y;
                }
                else
                {
                    Renderer renderer = obj.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Calculate how far below the pivot the bottom of the renderer is
                        bottomOffset = obj.transform.position.y - renderer.bounds.min.y;
                    }
                }

                float newY = hit.point.y + bottomOffset;
                obj.transform.position = new Vector3(
                    obj.transform.position.x,
                    newY,
                    obj.transform.position.z);

                Debug.Log($"Placed '{obj.name}' on ground at Y={newY:F2} (Quick Place)");
            }
            else
            {
                Debug.LogWarning($"No ground found for '{obj.name}' (Quick Place)");
            }
        }
    }
}