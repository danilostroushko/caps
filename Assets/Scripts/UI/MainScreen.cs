using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScreen : MonoBehaviour {
    

    public void StartGame2Players() {
        UIRoot.instance.ShowGameScreen(GameController.GameType.PVP);
    }

    public void StartGameVS() {
        UIRoot.instance.ShowGameScreen(GameController.GameType.PVE);
    }
    
}