using System;
using System.Collections.Generic;
using UnityEngine;

namespace HTNAI {

    // HTN Domain class, responsible for defining the domain (tasks, methods, effects, etc) available for our NPCs in their planning
    public static class HTNDomain {

        public static CompoundTask BeMeleeAdventurer {get; private set;}
        public static CompoundTask BeRangeAdventurer {get; private set;}
        public static CompoundTask DistractMinotaur {get; private set;}
        public static CompoundTask RangeDistractMinotaur {get; private set;}
        public static CompoundTask SeekChest {get; private set;}
        public static PrimitiveTask GoToChestTask {get; private set;}
        public static PrimitiveTask GoToMinotaurTask {get; private set;}
        public static PrimitiveTask GoToCornerTask {get; private set;}
        public static PrimitiveTask PickUpChestTask {get; private set;}
        public static PrimitiveTask MeleeAttackTask {get; private set;}
        public static PrimitiveTask RangeAttackTask {get; private set;}
        public static PrimitiveTask GetWithinLongRangeTask {get; private set;}

        static HTNDomain() {

            GoToChestTask = new GameObject().AddComponent<PrimitiveTask>();
            GoToChestTask.taskName = "Go to Chest";
            GoToChestTask.Precondition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] != Location.Chest && !worldState.ChestPickedUp;};
            GoToChestTask.Effects = new List<Effect> {new Effect {ApplyEffect = (worldState, adventurer) => {worldState.AdventurersLocations[adventurer.adventurerID] = Location.Chest;}}};
            GoToChestTask.Operator = adventurer => {return adventurer.GoToChestOperator();};

            GoToMinotaurTask = new GameObject().AddComponent<PrimitiveTask>();
            GoToMinotaurTask.taskName = "Go to Minotaur";
            GoToMinotaurTask.Precondition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] != Location.Minotaur;};
            GoToMinotaurTask.Effects = new List<Effect> {new Effect {ApplyEffect = (worldState, adventurer) => {worldState.AdventurersLocations[adventurer.adventurerID] = Location.Minotaur;}}};
            GoToMinotaurTask.Operator = adventurer => {return adventurer.GoToMinotaurOperator();};

            GoToCornerTask = new GameObject().AddComponent<PrimitiveTask>();
            GoToCornerTask.taskName = "Go to Corner";
            GoToCornerTask.Precondition = (worldState, adventurer) => {return  worldState.ChestPickedUp && worldState.AdventurersHaveChest[adventurer.adventurerID];};
            GoToCornerTask.Effects = new List<Effect> {new Effect {ApplyEffect = (worldState, adventurer) => {worldState.AdventurersWin = true;}}};
            GoToCornerTask.Operator = adventurer => {return adventurer.GoToCornerOperator();};

            GetWithinLongRangeTask = new GameObject().AddComponent<PrimitiveTask>();
            GetWithinLongRangeTask.taskName = "Get within Range";
            GetWithinLongRangeTask.Precondition = (worldState, adventurer) => {return !worldState.AdventurersWithinLongRange[adventurer.adventurerID];};
            GetWithinLongRangeTask.Effects = new List<Effect> {new Effect {ApplyEffect = (worldState, adventurer) => {worldState.AdventurersWithinLongRange[adventurer.adventurerID] = true;}}};
            GetWithinLongRangeTask.Operator = adventurer => {return adventurer.GetWithinLongRangeOperator();};

            PickUpChestTask = new GameObject().AddComponent<PrimitiveTask>();
            PickUpChestTask.taskName = "Pick Up Chest";
            PickUpChestTask.Precondition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] == Location.Chest && !worldState.ChestPickedUp;};
            PickUpChestTask.Effects = new List<Effect> {
                new Effect {ApplyEffect = (worldState, adventurer) => {worldState.ChestPickedUp = true;}}, 
                new Effect{ApplyEffect = (worldState, adventurer) => {worldState.AdventurersHaveChest[adventurer.adventurerID] = true;}}};
            PickUpChestTask.Operator = adventurer => {return adventurer.PickUpChestOperator();};

            MeleeAttackTask = new GameObject().AddComponent<PrimitiveTask>();
            MeleeAttackTask.taskName = "Melee Attack";
            MeleeAttackTask.Precondition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] == Location.Minotaur && !worldState.AdventurersHaveChest[adventurer.adventurerID];};
            MeleeAttackTask.Effects = new List<Effect> {};
            MeleeAttackTask.Operator = adventurer => {return adventurer.MeleeAttackOperator();};

            RangeAttackTask = new GameObject().AddComponent<PrimitiveTask>();
            RangeAttackTask.taskName = "Range Attack";
            RangeAttackTask.Precondition = (worldState, adventurer) => {return worldState.AdventurersWithinLongRange[adventurer.adventurerID] && !worldState.AdventurersHaveChest[adventurer.adventurerID];};
            RangeAttackTask.Effects = new List<Effect> {};
            RangeAttackTask.Operator = adventurer => {return adventurer.RangeAttackOperator();};

            DistractMinotaur = new GameObject().AddComponent<CompoundTask>();
            DistractMinotaur.Methods = new List<Method>{
                new Method{
                    Condition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] != Location.Minotaur;},
                    Subtasks = new List<Task>{MeleeAttackTask, GoToMinotaurTask}
                }, 
                new Method{
                    Condition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] == Location.Minotaur;},
                    Subtasks = new List<Task>{MeleeAttackTask}
                }
            };

            RangeDistractMinotaur = new GameObject().AddComponent<CompoundTask>();
            RangeDistractMinotaur.Methods = new List<Method>{
                new Method{
                    Condition = (worldState, adventurer) => {return !worldState.AdventurersWithinLongRange[adventurer.adventurerID];},
                    Subtasks = new List<Task>{RangeAttackTask, GetWithinLongRangeTask}
                }, 
                new Method{
                    Condition = (worldState, adventurer) => {return worldState.AdventurersWithinLongRange[adventurer.adventurerID];},
                    Subtasks = new List<Task>{RangeAttackTask}
                }
            };

            SeekChest = new GameObject().AddComponent<CompoundTask>();
            SeekChest.Methods = new List<Method>{
                new Method{
                    Condition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] != Location.Chest;},
                    Subtasks = new List<Task>{GoToCornerTask, PickUpChestTask, GoToChestTask}
                }, 
                new Method{
                    Condition = (worldState, adventurer) => {return worldState.AdventurersLocations[adventurer.adventurerID] == Location.Chest;},
                    Subtasks = new List<Task>{GoToCornerTask, PickUpChestTask}
                }
            };

            BeMeleeAdventurer = new GameObject().AddComponent<CompoundTask>();
            BeMeleeAdventurer.Methods = new List<Method>{
                new Method {
                    Condition = (worldState, adventurer) => {return !worldState.ChestPickedUp;},
                    Subtasks = new List<Task>{SeekChest}
                },
                new Method {
                    Condition = (worldState, adventurer) => {return worldState.ChestPickedUp && !worldState.AdventurersHaveChest[adventurer.adventurerID];},
                    Subtasks = new List<Task>{DistractMinotaur}
                }
            };

            BeRangeAdventurer = new GameObject().AddComponent<CompoundTask>();
            BeRangeAdventurer.Methods = new List<Method>{
                new Method {
                    Condition = (worldState, adventurer) => {return worldState.AdventurersAlive[0] || worldState.AdventurersAlive[2] || (worldState.ChestPickedUp && !worldState.AdventurersHaveChest[adventurer.adventurerID]);},
                    Subtasks = new List<Task>{RangeDistractMinotaur}
                },
                new Method {
                    Condition = (worldState, adventurer) => {return !worldState.ChestPickedUp;},
                    Subtasks = new List<Task>{SeekChest}
                }
            };

        }
    }
}