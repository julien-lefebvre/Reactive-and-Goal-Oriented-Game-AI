using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

namespace HTNAI {

    // Adventurer class, responsible for managing everything related to the adventurer NPCs, 
    // initializing HTN planning, defining operators, update, etc
    public class Adventurer : MonoBehaviour {

        public int adventurerID;
        public string currentTask;
        public int hitCount = 0;
        public TextMeshProUGUI taskListText; 
        private NavMeshAgent navMeshAgent;
        private Transform target;
        private Vector3 parentPosition;
        private Transform visual;
        private WorldStateManager worldStateManager;
        private HTNPlanner htnPlanner;
        private PlanRunner planRunner;
        private Plan plan;
        private Transform chestTransform;
        private Transform holdPoint;
        private int hitLimit;
        private bool chestPickedUp = false;
        private bool attackCoolDown = false;

        private void Awake() {
            navMeshAgent = GetComponent<NavMeshAgent>();
            parentPosition = transform.position;
            visual = transform.GetChild(0);
            // Adventurers 0  and 2 will be the melee adventurers, 1 and 3 are range
            if (adventurerID == 0 || adventurerID == 2) {
                hitLimit = 4;
            } else {
                hitLimit = 2;
            }
        }

        private void Start() {
            worldStateManager = GameObject.FindGameObjectWithTag("WorldState").GetComponent<WorldStateManager>();
            htnPlanner = new HTNPlanner();
            planRunner = gameObject.AddComponent<PlanRunner>();
            chestTransform = GameObject.FindGameObjectWithTag("Chest").transform;
            holdPoint = transform.GetChild(transform.childCount - 1);

            // Initial HTN planning
            GenerateAndExecutePlan();
        }

        private void Update() {
            if (chestPickedUp) {
                chestTransform.position = holdPoint.position;
            } else {
                chestTransform.position = new Vector3(chestTransform.position.x, 0.5f, chestTransform.position.z);
            }

        }

        private void FixedUpdate() {
            if (!worldStateManager.GlobalWorldState.AdventurersWin) {
                if (hitCount >= hitLimit) { // Adventurer died
                    worldStateManager.GlobalWorldState.AdventurersAlive[adventurerID] = false;
                    gameObject.GetComponent<PlanRunner>().StopAllCoroutines();
                    StopAllCoroutines();
                    UpdateTaskList(new List<PrimitiveTask>{});
                    chestPickedUp = false;
                    chestTransform.position = new Vector3(chestTransform.position.x, 0.5f, chestTransform.position.z);
                    
                    // For the others, replan
                    foreach (Adventurer adventurer in GameObject.FindObjectsOfType<Adventurer>()) {
                        if (adventurer != this && worldStateManager.GlobalWorldState.AdventurersAlive[adventurer.adventurerID]) {
                            adventurer?.GenerateAndExecutePlan();
                        }
                    } 
                    Destroy(gameObject);
                    
                } 

                // Walking animation       
                if (navMeshAgent.velocity.magnitude > 0.5) {
                    parentPosition = transform.position;
                    float verticalOffset = Mathf.Sin(Time.time * 20) * 0.1f;
                    visual.position = parentPosition + new Vector3(0.0f, verticalOffset, 0.0f);
                }
                
                // Range adventurers sensor update the distance with minotaur
                if (adventurerID == 1 || adventurerID == 3) {
                    Transform minotaur = GameObject.FindGameObjectWithTag("Minotaur").transform;
                    if (Vector3.Distance(minotaur.position, transform.position) > 7f) {
                        worldStateManager.GlobalWorldState.AdventurersWithinLongRange[adventurerID] = false;
                    }
                }
            }
        }

        public void GenerateAndExecutePlan() {
            gameObject.GetComponent<PlanRunner>().StopAllCoroutines();
            StopAllCoroutines();
            navMeshAgent.SetDestination(transform.position);

            // Give proper root task to different adventurer types
            Task rootTask = HTNDomain.BeMeleeAdventurer;
            if (adventurerID == 1 || adventurerID == 3) {
                rootTask = HTNDomain.BeRangeAdventurer;
            }
            
            // Generate the plan
            plan = htnPlanner.GeneratePlan(rootTask, worldStateManager.GlobalWorldState, this);
            if (plan != null) { // Execute the plan
                UpdateTaskList(plan.Tasks);
                planRunner.ExecutePlan(plan, worldStateManager.GlobalWorldState, this);
            }
        }

        /// ----------------------
        /// Operators for tasks
        /// ----------------------
        
        public bool GoToChestOperator() {
            target = GameObject.FindGameObjectWithTag("Chest").transform;
            navMeshAgent.SetDestination(target.position);
            if (!navMeshAgent.pathPending) {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GoToMinotaurOperator() {
            target = GameObject.FindGameObjectWithTag("Minotaur").transform;
            navMeshAgent.SetDestination(target.position);
            if (!navMeshAgent.pathPending) {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool GoToCornerOperator() {
            float smallestDistance = float.MaxValue;
            foreach (var corner in GameObject.FindGameObjectsWithTag("Corner")) {
                if (Vector3.Distance(corner.transform.position, transform.position) < smallestDistance) {
                    target = corner.transform;
                }
            }
            navMeshAgent.SetDestination(target.position);
            if (!navMeshAgent.pathPending) {
                if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) {
                    if (!navMeshAgent.hasPath || navMeshAgent.velocity.sqrMagnitude == 0f) {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool PickUpChestOperator() {
            StartCoroutine(PickUpChestCoroutine());
            return chestPickedUp;
        }

        private IEnumerator PickUpChestCoroutine() {
            yield return new WaitForSeconds(2f);
            chestPickedUp = true;
        }

        public void DropChest() {
            chestPickedUp = false;
            worldStateManager.GlobalWorldState.AdventurersHaveChest[adventurerID] = false;
            worldStateManager.GlobalWorldState.ChestPickedUp = false;
            foreach (Adventurer adventurer in GameObject.FindObjectsOfType<Adventurer>()) {
                adventurer.GenerateAndExecutePlan();
            } 
        }

        public bool GetWithinLongRangeOperator() {
            target = GameObject.FindGameObjectWithTag("Minotaur").transform;
            navMeshAgent.SetDestination(target.position);
            if (Vector3.Distance(target.position, transform.position) <= 7f) {
                navMeshAgent.SetDestination(transform.position);
                return true;
            }   
            return false;
        }

        private IEnumerator AttackCooldownCoroutine() {
            yield return new WaitForSeconds(1f);
            attackCoolDown = false;
        }

        public bool MeleeAttackOperator() {
            attackCoolDown = true;
            StartCoroutine(AttackCooldownCoroutine());
            return !attackCoolDown;
        }

        public bool RangeAttackOperator() {
            attackCoolDown = true;
            StartCoroutine(AttackCooldownCoroutine());
            return !attackCoolDown;
        }

        public void UpdateTaskList(List<PrimitiveTask> tasks) {
            string taskListString = "";

            for (int i = 0; i < tasks.Count; i++) {
                string taskText = tasks[i].taskName;

                if (taskText == currentTask) {
                    // Highlight the currently executed task in yellow
                    taskText = "<color=yellow>" + taskText + "</color>";
                }

                taskListString += taskText + "\n";
            }

            taskListText.text = taskListString;
        }
    } 
}
