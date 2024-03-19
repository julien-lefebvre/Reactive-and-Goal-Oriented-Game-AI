using HTNAI;
using UnityEngine;
using UnityEngine.AI;

// Reactive AI behaviour for the minotaur NPC
public class MinotaurAI : MonoBehaviour {

    private Transform treasureChest;
    private float detectionRadius = 8f;
    private float attackRange = 3f;
    private float maxDistanceFromChest = 5f;
    private NavMeshAgent navMeshAgent;
    private Transform target;
    private State currentState;
    private Vector3 parentPosition;
    private Transform visual; 
    private float rotationSpeed = 1080f;
    public float cooldownDuration = 1f;
    private bool isCooldown = false;
    public float totalRotation = 0f;
    private bool hitRegistered = false;

    private WorldStateManager worldStateManager;
    private enum State {
        Idle,
        Pursuit,
        Attack,
    }

    private void Start() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        worldStateManager = GameObject.FindGameObjectWithTag("WorldState").GetComponent<WorldStateManager>();
        treasureChest = GameObject.FindGameObjectWithTag("Chest").transform;
        parentPosition = transform.position;
        visual = transform.GetChild(0);
        currentState = State.Idle;
    }

    private void FixedUpdate() {
        if (!worldStateManager.GlobalWorldState.AdventurersWin) {
            switch (currentState) {
                case State.Idle:
                    UpdateIdleState();
                    break;
                
                case State.Pursuit:
                    UpdatePursuitState();
                    break;

                case State.Attack:
                    UpdateAttackState();
                    break;
            }

            if (navMeshAgent.velocity.magnitude > 0.5) {
                parentPosition = transform.position;
                float verticalOffset = Mathf.Sin(Time.time * 20) * 0.1f;
                visual.position = parentPosition + new Vector3(0.0f, verticalOffset, 0.0f);
            }
        }
    }
    
    private void UpdateIdleState() {
        // Calculate the distance from the Minotaur to the chest
        float distanceToChest = Vector3.Distance(transform.position, treasureChest.position);

        // If the Minotaur has moved too far from the chest, adjust its position
        if (distanceToChest > maxDistanceFromChest) {
            Vector3 directionToChest = (treasureChest.position - transform.position).normalized;
            Vector3 newPosition = treasureChest.position - directionToChest * maxDistanceFromChest;

            // Set the destination to the adjusted position
            navMeshAgent.SetDestination(newPosition);
        }

        float closestDistance = float.MaxValue;
        Transform closestAdventurer = null;

        Collider[] adventurers = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var adventurer in adventurers) {
            if (adventurer.CompareTag("Adventurer")) {

                Transform adventurerTransform = adventurer.transform;
                // Calculate the distance to the adventurer
                float distanceToAdventurer = Vector3.Distance(transform.position, adventurerTransform.position);

                // Update closest adventurer if this one is closer
                if (distanceToAdventurer < closestDistance) {
                    closestDistance = distanceToAdventurer;
                    closestAdventurer = adventurerTransform;
                }
            }
        }

        if (closestAdventurer != null) {
            target = closestAdventurer;
            currentState = State.Pursuit;
            return;
        }
    }

    private void UpdatePursuitState() {
        if (target == null) {
            currentState = State.Idle;
            return;
        }

        navMeshAgent.SetDestination(target.position);

        if (Vector3.Distance(transform.position, target.position) < attackRange) {
            currentState = State.Attack;
        }

        // Check for adventurers in detection radius
        Collider[] adventurers = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var adventurerCollider in adventurers) {
            if (adventurerCollider.CompareTag("Adventurer")) {
                Transform adventurerTransform = adventurerCollider.transform;

                // Calculate the distance to the adventurer
                float distanceToAdventurer = Vector3.Distance(transform.position, adventurerTransform.position);

                // If a new adventurer is closer than the current target, switch targets
                if (distanceToAdventurer < Vector3.Distance(transform.position, target.position)) {
                    target = adventurerTransform;
                }
            }
        }

        if (Vector3.Distance(transform.position, target.position) > detectionRadius) {
            currentState = State.Idle;
            target = null;
        }
    }

    private void UpdateAttackState() {
        if (!isCooldown) {
            if (!hitRegistered) {
                foreach (Adventurer adventurer in GameObject.FindObjectsOfType<Adventurer>()) {
                    if (Vector3.Distance(adventurer.transform.position, transform.position) <= attackRange) {
                        if (worldStateManager.GlobalWorldState.AdventurersHaveChest[adventurer.adventurerID]) {
                            adventurer.DropChest();
                        }
                        adventurer.hitCount++;
                    }
                }
                hitRegistered = true;
            }

            // Rotate the Minotaur around its Y-axis based on the rotation speed
            float rotationThisFrame = rotationSpeed * Time.fixedDeltaTime;
            visual.Rotate(Vector3.up, rotationThisFrame);

            // Update the total rotation
            totalRotation += rotationThisFrame;

            // Check if the Minotaur has completed the desired rotation (360 or 720 degrees)
            if (totalRotation >= 360) {
                // Set cooldown
                isCooldown = true;
                cooldownDuration = 1f; // Set the cooldown duration

                // Reset the total rotation for the next attack
                totalRotation = 0f;

                // Reset the rotation to 0
                visual.rotation = transform.rotation;
            }

        } else {
            // Minotaur is in cooldown, decrement the cooldown timer
            cooldownDuration -= Time.fixedDeltaTime;

            // If cooldown is complete, return to Pursuit state
            if (cooldownDuration <= 0) {
                isCooldown = false;
                hitRegistered = false;
                ReturnToPursuit();
            }
        }
    }

    private void ReturnToPursuit() {
        currentState = State.Pursuit;
    }

}
