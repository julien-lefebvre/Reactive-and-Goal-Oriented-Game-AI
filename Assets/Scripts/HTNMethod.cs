using System;
using System.Collections.Generic;

namespace HTNAI  {
    public class Method {
        public Func<WorldState, Adventurer, bool> Condition {get; set;}
        public List<Task> Subtasks {get; set;}

        public bool IsConditionSatisfied(WorldState worldState, Adventurer adventurer) {
            return Condition(worldState, adventurer);
        }
    }
}
