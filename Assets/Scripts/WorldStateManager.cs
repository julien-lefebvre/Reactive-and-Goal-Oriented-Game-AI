using HTNAI;
using UnityEngine;

public class WorldStateManager : MonoBehaviour {

    public WorldState GlobalWorldState;
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject UIcanvas;

    void Awake() {
        GlobalWorldState = new WorldState();
        UIcanvas.SetActive(true);
    }

    private void Update() {
        if (GlobalWorldState.AdventurersWin) {
            winScreen.SetActive(true);
        }

        bool minotaurWins = true;
        foreach (bool alive in GlobalWorldState.AdventurersAlive) {
            if (alive) {
                minotaurWins = false;
                break;
            }
        }

        if (minotaurWins) {
            loseScreen.SetActive(true);
        }
    }

}
