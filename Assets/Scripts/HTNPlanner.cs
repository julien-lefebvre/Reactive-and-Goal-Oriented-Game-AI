using System.Collections.Generic;

namespace HTNAI  {

    // HTN planner class, responsible for generating plans for the NPCs
    public class HTNPlanner {

        private Stack<PlannerState> decompHistory = new Stack<PlannerState>();
        private Stack<Task> tasksToProcess;
        private Plan plan;
        private WorldState currentWorldState;
        public Plan GeneratePlan(Task rootTask, WorldState globalWorldState, Adventurer adventurer) {
            
            tasksToProcess = new Stack<Task>();
            plan = new Plan();
            currentWorldState = new WorldState(globalWorldState);
            
            tasksToProcess.Push(rootTask);

            while (tasksToProcess.Count > 0) {
                Task currentTask = tasksToProcess.Pop();
                if (currentTask is CompoundTask compoundTask) {
                    Method satisfiedMethod = compoundTask.FindMethod(currentWorldState, adventurer);

                    if (satisfiedMethod != null) {
                        RecordPlannerState(currentTask, plan, satisfiedMethod);
                        foreach (Task subtask in satisfiedMethod.Subtasks) {
                            tasksToProcess.Push(subtask);
                        }
                    } 
                    else {
                        RestoreToLastDecomposedTask();
                    }
                }
                else if (currentTask is PrimitiveTask primitiveTask) {
                    if (primitiveTask.ConditionMet(currentWorldState, adventurer)) {
                        currentWorldState.ApplyEffects(primitiveTask.Effects, adventurer);
                        plan.Tasks.Add(primitiveTask);
                    }
                    else {
                        RestoreToLastDecomposedTask();
                    }
                }
            }

            return plan;
        }

        private void RecordPlannerState(Task task, Plan plan, Method methodChosen) {
            decompHistory.Push(new PlannerState {
                TasksToProcess = new Stack<Task>(tasksToProcess),
                FinalPlan = new Plan(plan),
                MethodChosen = methodChosen,
                DecomposedTask = task 
            });
        }

        private void RestoreToLastDecomposedTask() {
            if (decompHistory.Count > 0) {
                PlannerState lastState = decompHistory.Pop();
                tasksToProcess = lastState.TasksToProcess;
                plan = lastState.FinalPlan;
            }
        }

        private class PlannerState {
            public Stack<Task> TasksToProcess {get; set;}
            public Plan FinalPlan {get; set;}
            public Method MethodChosen {get; set;}
            public Task DecomposedTask {get; set;}
        }
    }
}

