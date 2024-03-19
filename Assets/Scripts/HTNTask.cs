using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HTNAI  {
    public abstract class Task : MonoBehaviour {
    }

    public class CompoundTask : Task {
        public List<Method> Methods {get; set;}

        public Method FindMethod(WorldState worldState, Adventurer adventurer) {
            foreach (Method method in Methods) {
                if (method.IsConditionSatisfied(worldState, adventurer)) {
                    return method;
                }
            }
            return null;
        }
    }

    public class PrimitiveTask : Task {
        public Func<WorldState, Adventurer, bool> Precondition { get; set; }
        public List<Effect> Effects { get; set; }
        public Func<Adventurer, bool> Operator { get; set; }
        public string taskName;

        public bool ConditionMet(WorldState worldState, Adventurer adventurer) {
            return Precondition(worldState, adventurer);
        }

        public IEnumerator OperatorCoroutine(Adventurer adventurer) {
            yield return new WaitUntil(() => Operator(adventurer));
        }
    }

    public class Effect {
        public Action<WorldState, Adventurer> ApplyEffect {get; set;}
    }
}


