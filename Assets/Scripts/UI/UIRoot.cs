using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIRoot : MonoBehaviour {

    public static UIRoot instance;

    public MainScreen mainScreen;
    public GameScreen gameScreen;

    void Awake() {
        instance = this;

        ShowMainScreen();
    }
    //
    public void ShowMainScreen() {
        mainScreen.gameObject.SetActive(true);
        gameScreen.gameObject.SetActive(false);
    }

    public void ShowGameScreen(GameController.GameType gameType) {
        mainScreen.gameObject.SetActive(false);
        gameScreen.gameObject.SetActive(true);
        //
        GameController.instance.gameType = gameType;
    }
}