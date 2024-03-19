using System.Collections;
using UnityEngine;

namespace HTNAI  {

    // HTN plan runner class, responsible for executing a generated plan for an NPC
    public class PlanRunner : MonoBehaviour {

        public Coroutine ExecutePlan(Plan plan, WorldState worldState, Adventurer adventurer) {
            Coroutine planExecutionCoroutine = StartCoroutine(ExecutePlanCoroutine(plan, worldState, adventurer));
            return planExecutionCoroutine;
        }

        private IEnumerator ExecutePlanCoroutine(Plan plan, WorldState worldState, Adventurer adventurer) {
            foreach (PrimitiveTask task in plan.Tasks) {
                if (task.ConditionMet(worldState, adventurer)) {
                    adventurer.currentTask = task.taskName;
                    adventurer.UpdateTaskList(plan.Tasks);
                    yield return StartCoroutine(task.OperatorCoroutine(adventurer));
                    worldState.ApplyEffects(task.Effects, adventurer);
                    foreach (Adventurer otherAdventurer in GameObject.FindObjectsOfType<Adventurer>()) {
                        if (otherAdventurer != adventurer) {
                            otherAdventurer.GenerateAndExecutePlan();
                        }
                    } 
                }
            }
        }
    }
}
