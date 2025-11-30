using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class AICharacterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private GameObject failedToPressButton;
    [SerializeField] private GameObject failedToPressButtonDeactivate;
    [SerializeField] private PossessableFirstPersonController playerController;
    [SerializeField] private Transform attackOriginPoint;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float attackStopDistance = 0.5f;
    [SerializeField] private float attackVerticalThreshold = 0.3f;
    
    [Header("Path Tracking")]
    [SerializeField] private float recordInterval = 0.25f;
    [SerializeField] private float minPointDistance = 1.2f;
    [SerializeField] private float reachDistance = 2f;
    [SerializeField] private float navMeshSampleDistance = 5f;
    
    private NavMeshAgent navAgent;
    private List<Vector3> playerPath = new List<Vector3>();
    private int targetWaypointIndex = 0;
    private float recordTimer = 0f;
    private bool hasAttacked = false;
    
    // Stuck detection
    private Vector3 positionLastFrame;
    private float timeSinceMovement = 0f;
    private const float STUCK_TIME_THRESHOLD = 2f;
    private const float MOVEMENT_THRESHOLD = 0.15f;

    void OnEnable()
    {
        // Set checkpoint when AI is enabled
        PlayerPrefs.SetInt("startFight", 1);
        PlayerPrefs.Save();
    }

    void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();
        
        if (navAgent != null)
        {
            navAgent.speed = moveSpeed;
            navAgent.acceleration = 8f;
            navAgent.angularSpeed = 120f;
            navAgent.stoppingDistance = 1f;
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
        if (player == null || navAgent == null) return;
        
        // Use attack origin point if assigned, otherwise use AI's position
        Vector3 attackPosition = attackOriginPoint != null ? attackOriginPoint.position : transform.position;
        
        // Check if close enough to attack
        float distanceToPlayer = Vector3.Distance(attackPosition, player.position);
        float verticalDistance = Mathf.Abs(attackPosition.y - player.position.y);
        
        //Debug.Log($"AI Distance Check - Horizontal: {distanceToPlayer:F2} (max: {attackDistance}), Vertical: {verticalDistance:F2} (max: {attackVerticalThreshold}), HasAttacked: {hasAttacked}");
        
        if (distanceToPlayer <= attackDistance && verticalDistance <= attackVerticalThreshold && !hasAttacked)
        {
            //Debug.Log("ATTACK TRIGGERED!");
            TriggerAttack();
        }
        
        // If attacking, maintain optimal distance from player
        if (hasAttacked)
        {
            if (distanceToPlayer > attackStopDistance)
            {
                // Keep moving closer to reach optimal distance
                Vector3 targetPosition = player.position;
                NavMeshHit hit;
                if (NavMesh.SamplePosition(targetPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
                {
                    navAgent.SetDestination(hit.position);
                }
            }
            else
            {
                // Stop when at optimal distance
                navAgent.isStopped = true;
            }
        }
        else
        {
            RecordPlayerPosition();
            UpdateTarget();
            CheckIfStuck();
        }
    }
    
    void TriggerAttack()
    {
        hasAttacked = true;
        
        // Trigger attack animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // Pause the playable director
        if (playableDirector != null)
        {
            playableDirector.Pause();
        }

        if (failedToPressButton != null)
        {
            failedToPressButton.SetActive(true);
        }

        if (failedToPressButtonDeactivate != null)
        {
            failedToPressButtonDeactivate.SetActive(false);
        }
        
        // Disable player movement and force camera to look at AI
        if (playerController != null)
        {
            playerController.enabled = false;
            StartCoroutine(LookAtAI());
        }
        
        // Start coroutine to reload scene after attack
        StartCoroutine(ReloadSceneAfterAttack());
    }
    
    System.Collections.IEnumerator ReloadSceneAfterAttack()
    {
        yield return new WaitForSeconds(4f);
        
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    System.Collections.IEnumerator LookAtAI()
    {
        if (playerController == null || player == null) yield break;
        
        // Get the player's camera transform
        Transform playerCamera = playerController.transform;
        if (playerController.GetComponentInChildren<Camera>() != null)
        {
            playerCamera = playerController.GetComponentInChildren<Camera>().transform;
        }
        
        // Smoothly rotate camera to look at AI
        float rotationSpeed = 3f;
        float elapsedTime = 0f;
        float duration = 0.5f;
        
        Quaternion startRotation = playerCamera.rotation;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            
            // Calculate look direction - aim 1 unit higher to see the head
            Vector3 targetPoint = transform.position + Vector3.up * 1f;
            Vector3 directionToAI = (targetPoint - playerCamera.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToAI);
            
            // Smoothly rotate
            playerCamera.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
            
            yield return null;
        }
    }
    
    void RecordPlayerPosition()
    {
        recordTimer += Time.deltaTime;
        
        if (recordTimer >= recordInterval)
        {
            recordTimer = 0f;
            
            // Add point if it's far enough from the last one
            if (playerPath.Count == 0 || Vector3.Distance(player.position, playerPath[playerPath.Count - 1]) >= minPointDistance)
            {
                playerPath.Add(player.position);
                
                // Limit path size
                if (playerPath.Count > 500)
                {
                    int removeCount = 100;
                    playerPath.RemoveRange(0, removeCount);
                    targetWaypointIndex = Mathf.Max(0, targetWaypointIndex - removeCount);
                }
            }
        }
    }
    
    void UpdateTarget()
    {
        // Clean up reached waypoints
        while (targetWaypointIndex < playerPath.Count)
        {
            float distanceToWaypoint = Vector3.Distance(transform.position, playerPath[targetWaypointIndex]);
            
            if (distanceToWaypoint <= reachDistance)
            {
                targetWaypointIndex++;
            }
            else
            {
                break;
            }
        }
        
        // Determine target position
        Vector3 targetPosition;
        
        if (targetWaypointIndex < playerPath.Count)
        {
            targetPosition = playerPath[targetWaypointIndex];
        }
        else
        {
            targetPosition = player.position;
        }
        
        // Sample the position onto the NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, navMeshSampleDistance, NavMesh.AllAreas))
        {
            // Set the destination on the NavMesh
            navAgent.SetDestination(hit.position);
        }
        else
        {
            // Fallback to direct position if not on NavMesh
            navAgent.SetDestination(targetPosition);
        }
    }
    
    void CheckIfStuck()
    {
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
        // Skip ahead in the path to get past the stuck point
        int skipAmount = Mathf.Min(5, playerPath.Count - targetWaypointIndex);
        targetWaypointIndex += skipAmount;
        
        // If we skipped everything, clear path and go direct to player
        if (targetWaypointIndex >= playerPath.Count)
        {
            playerPath.Clear();
            targetWaypointIndex = 0;
            
            // Try going directly to player
            NavMeshHit hit;
            if (NavMesh.SamplePosition(player.position, out hit, navMeshSampleDistance, NavMesh.AllAreas))
            {
                navAgent.SetDestination(hit.position);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (playerPath == null || playerPath.Count == 0) return;

        // Draw path
        Gizmos.color = Color.yellow;
        for (int i = 0; i < playerPath.Count - 1; i++)
        {
            Gizmos.DrawLine(playerPath[i], playerPath[i + 1]);
        }
        
        // Draw waypoints
        for (int i = 0; i < playerPath.Count; i++)
        {
            if (i < targetWaypointIndex)
                Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f); // Passed
            else if (i == targetWaypointIndex)
                Gizmos.color = Color.red; // Current target
            else
                Gizmos.color = Color.cyan; // Future
                
            Gizmos.DrawSphere(playerPath[i], 0.2f);
        }
        
        // Draw current target line
        if (targetWaypointIndex < playerPath.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, playerPath[targetWaypointIndex]);
        }
        
        // Draw NavMesh path
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            Vector3[] corners = navAgent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }
}
