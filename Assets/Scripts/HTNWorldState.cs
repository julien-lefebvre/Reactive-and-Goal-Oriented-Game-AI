using System.Collections.Generic;

namespace HTNAI  {

    public enum Location {
        Chest,
        Minotaur, 
        Other
    }

    public class WorldState {

        public List<Location> AdventurersLocations;
        public bool ChestPickedUp;
        public List<bool> AdventurersHaveChest;
        public bool AdventurersWin;
        public List<bool> AdventurersAlive;
        public List<bool> AdventurersWithinLongRange;

        public WorldState() {
            AdventurersLocations = new List<Location>{Location.Other, Location.Other, Location.Other, Location.Other};
            ChestPickedUp = false;
            AdventurersHaveChest = new List<bool>{false, false, false, false};
            AdventurersWin = false;
            AdventurersAlive = new List<bool>{true, true, true, true};
            AdventurersWithinLongRange = new List<bool>{false, false, false, false};
        }

        public WorldState(WorldState worldState) {
            AdventurersLocations = new List<Location>(worldState.AdventurersLocations);
            ChestPickedUp = worldState.ChestPickedUp;
            AdventurersHaveChest = new List<bool>(worldState.AdventurersHaveChest);
            AdventurersWin = worldState.AdventurersWin;
            AdventurersAlive = new List<bool>(worldState.AdventurersAlive);
            AdventurersWithinLongRange = new List<bool>(worldState.AdventurersWithinLongRange);
        }

        public void ApplyEffects(List<Effect> effects, Adventurer adventurer) {
            foreach (Effect effect in effects) {
                effect.ApplyEffect(this, adventurer);
            }
        }

    }
    
}
