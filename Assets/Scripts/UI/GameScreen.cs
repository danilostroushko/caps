using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class GameScreen : MonoBehaviour {

    public TextMeshProUGUI redTitle;
    public RectTransform placeCapRed;
    public TextMeshProUGUI redCapsCount;
    public TextMeshProUGUI redCapsTakedCount;
    public RectTransform takenRed;
    public GameObject redPuls;

    public TextMeshProUGUI blueTitle;
    public RectTransform placeCapBlue;
    public TextMeshProUGUI blueCapsCount;
    public TextMeshProUGUI blueCapsTakedCount;
    public RectTransform takenBlue;
    public GameObject bluePuls;

    public CanvasGroup takeOutPopup;

    public TextMeshProUGUI gameResultText;

    void Start() {

    }

    public void UpdateCapsCount() {
        redCapsCount.text = GameController.instance.redPlayerCaps.FindAll(c => c.playerType == PlayerCap.PlayerType.Red && c.capState == PlayerCap.CapState.InPlayer).Count.ToString();
        redCapsTakedCount.text = GameController.instance.bluePlayerCaps.FindAll(c => c.playerType == PlayerCap.PlayerType.Blue && c.capState == PlayerCap.CapState.Taken).Count.ToString();

        blueCapsCount.text = GameController.instance.bluePlayerCaps.FindAll(c => c.playerType == PlayerCap.PlayerType.Blue && c.capState == PlayerCap.CapState.InPlayer).Count.ToString();
        blueCapsTakedCount.text = GameController.instance.redPlayerCaps.FindAll(c => c.playerType == PlayerCap.PlayerType.Red && c.capState == PlayerCap.CapState.Taken).Count.ToString();
    }
    //
    public void GoToMenu() {
        UIRoot.instance.ShowMainScreen();
    }
    //
    public void RestartGame() {
        GameController.instance.RestartGame();
    }
    //
    public void ShowTakeOut() {
        takeOutPopup.alpha = 0f;
        takeOutPopup.gameObject.SetActive(true);
        takeOutPopup.DOFade(1f, 0.5f);
    }
    //
    public void HideTakeOut() {
        takeOutPopup.DOFade(0f, 0.5f).OnComplete(() => {
            takeOutPopup.gameObject.SetActive(false);
        });
    }

    public void PlayerTurn(int player) {
        gameResultText.text = "Player " + (player + 1) + " turn";
    }

    public void ShowGameEnd(string resultText) {
        gameResultText.text = resultText;
        gameResultText.gameObject.SetActive(true);
    }
}