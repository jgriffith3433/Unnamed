using UnityEngine;
using System.Collections.Generic;

public class AICharacterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float rotationSpeed = 8f;
    
    [Header("Path Tracking")]
    [SerializeField] private float recordInterval = 0.25f;
    [SerializeField] private float minPointDistance = 1.2f;
    [SerializeField] private float reachDistance = 2f;
    
    [Header("Obstacle Avoidance")]
    [SerializeField] private float obstacleCheckDistance = 2.5f;
    [SerializeField] private float sideCheckDistance = 1.5f;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private int avoidanceRayCount = 5;
    
    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;
    
    private CharacterController controller;
    private List<Vector3> path = new List<Vector3>();
    private int targetWaypointIndex = 0;
    private float recordTimer = 0f;
    private Vector3 verticalVelocity;
    
    // Stuck detection
    private Vector3 positionLastFrame;
    private float timeSinceMovement = 0f;
    private const float STUCK_TIME_THRESHOLD = 2f;
    private const float MOVEMENT_THRESHOLD = 0.15f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        if (controller != null)
        {
            controller.minMoveDistance = 0f;
            controller.stepOffset = 0.4f;
            controller.slopeLimit = 50f;
        }
        
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        positionLastFrame = transform.position;
    }

    void Update()
    {
        if (player == null || controller == null) return;
        
        RecordPlayerPosition();
        UpdateTarget();
        Move();
        ApplyGravity();
    }
    
    void RecordPlayerPosition()
    {
        recordTimer += Time.deltaTime;
        
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            
            // Add point if it's far enough from the last one
            if (path.Count == 0 || Vector3.Distance(player.position, path[path.Count - 1]) >= minPointDistance)
            {
                path.Add(player.position);
                
                // Limit path size
                if (path.Count > 500)
                {
                    int removeCount = 100;
                    path.RemoveRange(0, removeCount);
                    targetWaypointIndex = Mathf.Max(0, targetWaypointIndex - removeCount);
                }
            }
        }
    }
    
    void UpdateTarget()
    {
        // Clean up reached waypoints
        while (targetWaypointIndex < path.Count)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, path[targetWaypointIndex]);
            
            if (distanceToWaypoint <= reachDistance)
            {
                targetWaypointIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Check if stuck
        float distanceMoved = Vector3.Distance(transform.position, positionLastFrame);
        positionLastFrame = transform.position;
        
        if (distanceMoved < MOVEMENT_THRESHOLD)
        {
            timeSinceMovement += Time.deltaTime;
            
            if (timeSinceMovement >= STUCK_TIME_THRESHOLD)
            {
                HandleStuck();
                timeSinceMovement = 0f;
            }
        }
        else
        {
            timeSinceMovement = 0f;
        }
    }
    
    void HandleStuck()
    {
        // When stuck, check if we're trying to reach a waypoint at a different height
        if (targetWaypointIndex < path.Count)
        {
            float heightDiff = Mathf.Abs(path[targetWaypointIndex].y - transform.position.y);
            
            // If waypoint is much higher/lower, we might be at the wrong floor
            // Skip ahead to find a waypoint at our current height
            if (heightDiff > 1f)
            {
                int searchIndex = targetWaypointIndex + 1;
                bool foundBetterWaypoint = false;
                
                // Look ahead for a waypoint closer to our height
                while (searchIndex < path.Count && searchIndex < targetWaypointIndex + 10)
                {
                    float testHeightDiff = Mathf.Abs(path[searchIndex].y - transform.position.y);
                    if (testHeightDiff < heightDiff)
                    {
                        targetWaypointIndex = searchIndex;
                        foundBetterWaypoint = true;
                        break;
                    }
                    searchIndex++;
                }
                
                if (!foundBetterWaypoint)
                {
                    // Skip ahead in the path
                    int skipAmount = Mathf.Min(3, path.Count - targetWaypointIndex);
                    targetWaypointIndex += skipAmount;
                }
            }
            else
            {
                // Normal stuck on same level - skip ahead
                int skipAmount = Mathf.Min(5, path.Count - targetWaypointIndex);
                targetWaypointIndex += skipAmount;
            }
        }
        
        // If we skipped everything, clear path and go direct to player
        if (targetWaypointIndex >= path.Count)
        {
            path.Clear();
            targetWaypointIndex = 0;
        }
    }
    
    void Move()
    {
        Vector3 targetPosition;
        
        // Determine where to move
        if (targetWaypointIndex < path.Count)
        {
            targetPosition = path[targetWaypointIndex];
        }
        else
        {
            targetPosition = player.position;
        }
        
        // Calculate movement direction
        Vector3 desiredDirection = targetPosition - transform.position;
        float verticalDifference = Mathf.Abs(desiredDirection.y);
        desiredDirection.y = 0; // Horizontal direction for obstacle checking
        
        float distance = desiredDirection.magnitude;
        
        // Only move if not too close
        if (distance > 1f)
        {
            desiredDirection.Normalize();
            
            Vector3 finalDirection;
            
            // If there's significant vertical difference (stairs/ramps), trust the waypoint path
            // Don't try to avoid obstacles - we need to go up/down
            if (verticalDifference > 0.5f && targetWaypointIndex < path.Count)
            {
                // Move directly toward waypoint, including vertical component
                finalDirection = (targetPosition - transform.position).normalized;
                finalDirection.y = 0; // Still keep horizontal for rotation, but trust the controller for vertical
                
                // Move with original direction toward the waypoint
                Vector3 move = (targetPosition - transform.position);
                move.y = 0; // Let gravity handle Y
                move = move.normalized * moveSpeed * Time.deltaTime;
                controller.Move(move);
            }
            else
            {
                // On flat ground, use obstacle avoidance
                finalDirection = GetAvoidanceDirection(desiredDirection);
                
                // Move the character
                Vector3 move = finalDirection * moveSpeed * Time.deltaTime;
                controller.Move(move);
            }
            
            // Rotate toward movement direction
            if (finalDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(finalDirection);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    Vector3 GetAvoidanceDirection(Vector3 desiredDirection)
    {
        // Check if the desired direction is blocked
        Vector3 rayOrigin = transform.position + Vector3.up * (controller.height * 0.5f);
        
        // If desired path is clear, use it
        if (!Physics.Raycast(rayOrigin, desiredDirection, obstacleCheckDistance, obstacleLayer))
        {
            return desiredDirection;
        }
        
        // Desired path is blocked, find best alternative
        float bestDot = -1f;
        Vector3 bestDirection = desiredDirection;
        bool foundClear = false;
        
        // Check multiple directions in an arc
        for (int i = 0; i < avoidanceRayCount; i++)
        {
            float angle = -90f + (180f / (avoidanceRayCount - 1)) * i;
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * desiredDirection;
            
            // Check if this direction is clear
            if (!Physics.Raycast(rayOrigin, testDirection, sideCheckDistance, obstacleLayer))
            {
                // How aligned is this with our desired direction?
                float dot = Vector3.Dot(testDirection, desiredDirection);
                
                if (dot > bestDot)
                {
                    bestDot = dot;
                    bestDirection = testDirection;
                    foundClear = true;
                }
            }
        }
        
        // If we found a clear path, use it
        if (foundClear)
        {
            return bestDirection;
        }
        
        // All paths blocked, try moving perpendicular to find a way around
        Vector3 perpendicular = Vector3.Cross(desiredDirection, Vector3.up);
        
        // Check both perpendicular directions
        if (!Physics.Raycast(rayOrigin, perpendicular, sideCheckDistance, obstacleLayer))
        {
            return perpendicular;
        }
        else if (!Physics.Raycast(rayOrigin, -perpendicular, sideCheckDistance, obstacleLayer))
        {
            return -perpendicular;
        }
        
        // Completely stuck, still try to move in desired direction
        // CharacterController collision will handle the rest
        return desiredDirection;
    }
    
    void ApplyGravity()
    {
        if (controller.isGrounded)
        {
            verticalVelocity.y = -2f;
        }
        else
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        
        controller.Move(verticalVelocity * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        if (path == null || path.Count == 0) return;

        // Draw path
        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }
        
        // Draw waypoints
        for (int i = 0; i < path.Count; i++)
        {
            if (i < targetWaypointIndex)
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Passed
            else if (i == targetWaypointIndex)
                Gizmos.color = Color.red; // Current target
            else
                Gizmos.color = Color.cyan; // Future
                
            Gizmos.DrawSphere(path[i], 0.2f);
        }
        
        // Draw current target line
        if (targetWaypointIndex < path.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, path[targetWaypointIndex]);
        }
        
        // Draw obstacle detection rays
        if (controller != null && Application.isPlaying)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * (controller.height * 0.5f);
            Vector3 forward = transform.forward;
            
            // Main forward ray
            Gizmos.color = Color.red;
            Gizmos.DrawRay(rayOrigin, forward * obstacleCheckDistance);
            
            // Side rays
            Gizmos.color = Color.yellow;
            for (int i = 0; i < avoidanceRayCount; i++)
            {
                float angle = -90f + (180f / (avoidanceRayCount - 1)) * i;
                Vector3 testDirection = Quaternion.Euler(0, angle, 0) * forward;
                Gizmos.DrawRay(rayOrigin, testDirection * sideCheckDistance);
            }
        }
    }
}
