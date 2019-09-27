using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public static GameController instance;

    public enum GameType {
        PVP,
        PVE,
        EVE
    }

    public enum GameState {
        Red,
        Blue,
        RedTakeCap,
        BlueTakeCap
    }

    public GameType gameType;
    public GameState gameState = GameState.Red;

    public int capsCount = 12;

    public bool isPlay = false;

    [HideInInspector]
    public GameScreen gameScreen;

    public RectTransform redCapsHolder;
    public RectTransform blueCapsHolder;

    public RectTransform draggableCap;
    public Sprite redCapImage;
    public Sprite blueCapImage;

    public List<CapsHolder> capsHolders = new List<CapsHolder>();

    [FoldoutGroup("Combinations")]
    public List<Combination> horizontals = new List<Combination>();

    [FoldoutGroup("Combinations")]
    public List<Combination> verticals = new List<Combination>();

    [FoldoutGroup("Combinations")]
    public List<Combination> diagonals = new List<Combination>();

    public List<PlayerCap> redPlayerCaps = new List<PlayerCap>();
    public List<PlayerCap> bluePlayerCaps = new List<PlayerCap>();

    private bool capIsDraged = false;
    public bool isReplaceCap = false;
    public int canTakeCaps = 0;
    private PlayerCap draggedCap;
    public CapsHolder prevCapHolder;

    [HideInInspector]
    public bool blockInput = false;

    private bool AITurn = false;

    private void Awake() {
        instance = this;
        gameScreen = GetComponent<GameScreen>();
    }
    //
    private void Start() {
        
    }
    //
    private void OnEnable() {
        StartGame();
    }
    //
    private void Update() {
        if (blockInput || !isPlay) { return; }
        //
        if (capIsDraged && !AITurn) {
            if (Input.GetMouseButton(0)) {
                draggableCap.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            //
            if (Input.GetMouseButtonUp(0)) {
                capIsDraged = false;

                if (!isReplaceCap) {
                    capsHolders.ForEach(c => c.HidePlace());
                }

                CapsHolder capsHolderForPlace = null;
                if (CanPlaceCap(out capsHolderForPlace)) {
                    PlaceCap(draggedCap, capsHolderForPlace);
                } else if (!isReplaceCap) {
                    RevertCapToPlayer(draggedCap.playerType);
                } else {
                    RevertCapToPrev();
                }
            }
            
        }
    }
    //
    private void StartGame() {
        isPlay = true;
        //
        redPlayerCaps.Clear();
        bluePlayerCaps.Clear();

        capsHolders.ForEach(c => c.Clear());

        for (int i = 0; i < capsCount; i++) {
            redPlayerCaps.Add(new PlayerCap() { capState = PlayerCap.CapState.InPlayer, playerType = PlayerCap.PlayerType.Red });
            bluePlayerCaps.Add(new PlayerCap() { capState = PlayerCap.CapState.InPlayer, playerType = PlayerCap.PlayerType.Blue });
        }
        //
        gameScreen.UpdateCapsCount();
        //
        gameState = GameState.Red;
        //
        gameScreen.redTitle.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 1f, 5)
            .SetDelay(1f).OnComplete(() => {
                if (redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0) { gameScreen.redPuls.SetActive(true); }
            });
        //
        gameScreen.PlayerTurn((int)gameState);
    }
    //
    public void RestartGame() {
        AITurn = false;
        //
        redPlayerCaps.Clear();
        bluePlayerCaps.Clear();

        capsHolders.ForEach(c => c.Clear());

        for (int i = 0; i < capsCount; i++) {
            redPlayerCaps.Add(new PlayerCap() { capState = PlayerCap.CapState.InPlayer, playerType = PlayerCap.PlayerType.Red });
            bluePlayerCaps.Add(new PlayerCap() { capState = PlayerCap.CapState.InPlayer, playerType = PlayerCap.PlayerType.Blue });
        }
        //
        gameScreen.UpdateCapsCount();
        //
        gameState = GameState.Red;
        //
        gameScreen.redTitle.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 1f, 5)
            .SetDelay(1f).OnComplete(() => {
                if (redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0) { gameScreen.redPuls.SetActive(true); }
            });
        //
        gameScreen.PlayerTurn((int)gameState);
        //
        isPlay = true;
    }
    //
    private void GameEnd() {
        isPlay = false;
        //
        int redTakenCaps = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.Taken).Count;
        int blueTakenCaps = redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.Taken).Count;
        //
        if (redTakenCaps == blueTakenCaps) {
            gameScreen.ShowGameEnd("draw");
        } else {
            gameScreen.ShowGameEnd("player " + (redTakenCaps > blueTakenCaps ? "1" : "2") + " wins");
        }
    }
    //
    private void PlaceCap(PlayerCap draggedCap, CapsHolder capsHolderForPlace) {
        RectTransform targetRect = capsHolderForPlace.rectTransform;
        Vector2 target = Utils.SwitchToRectTransform(targetRect, GetComponent<RectTransform>());
        blockInput = true;
        draggableCap.DOAnchorPos(target, 0.1f)
            .OnComplete(() => {
                draggableCap.gameObject.SetActive(false);
                //
                if (isReplaceCap) {
                    prevCapHolder.Clear();
                    prevCapHolder = null;
                    isReplaceCap = false;
                }
                //
                capsHolderForPlace.PlaceCap(draggedCap);
                //
                gameScreen.UpdateCapsCount();
                //
                gameScreen.redPuls.SetActive(false);
                gameScreen.bluePuls.SetActive(false);
                //
                if (CheckCombination(capsHolderForPlace, out canTakeCaps)) {
                    gameScreen.ShowTakeOut();
                    capsHolders.ForEach(c => c.ShowCanTakeCap(gameState == GameState.RedTakeCap ? PlayerCap.PlayerType.Blue : PlayerCap.PlayerType.Red));
                    if (gameState == GameState.BlueTakeCap && gameType == GameType.PVE) {
                        AITurn = true;
                        AITakeCap();
                    }
                } else {
                    NextPlayerMove();
                }
                //
                blockInput = false;
            });
    }
    //
    public void TakeCap(CapsHolder takenCap) {
        canTakeCaps--;
        //
        if (canTakeCaps == 0) {
            capsHolders.ForEach(c => c.HideCanTakeCap());
            gameScreen.HideTakeOut();
        }
        //
        draggableCap.GetComponent<Image>().sprite = takenCap.playerCap.playerType == PlayerCap.PlayerType.Red ? redCapImage : blueCapImage;
        draggableCap.anchoredPosition = Utils.SwitchToRectTransform(takenCap.rectTransform, GetComponent<RectTransform>());
        draggableCap.gameObject.SetActive(true);
        //
        RectTransform targetRect = takenCap.playerCap.playerType == PlayerCap.PlayerType.Red ? gameScreen.takenBlue : gameScreen.takenRed;
        Vector2 target = Utils.SwitchToRectTransform(targetRect, GetComponent<RectTransform>());
        blockInput = true;
        draggableCap.DOAnchorPos(target, 0.5f).OnComplete(() => {
            draggableCap.gameObject.SetActive(false);
            //
            takenCap.playerCap.capState = PlayerCap.CapState.Taken;
            takenCap.playerCap.capsHolder = null;
            takenCap.Clear();
            takenCap.HideCanTakeCap();
            //
            gameScreen.UpdateCapsCount();
            //
            if (canTakeCaps == 0) {
                gameState = gameState == GameState.RedTakeCap ? GameState.Red : GameState.Blue;
                NextPlayerMove();
            }
            //
            if (gameType == GameType.PVE && gameState == GameState.BlueTakeCap) {
                AITurn = true;
                AITakeCap();
            }
            //
            blockInput = false;
        });
    }
    //
    private void NextPlayerMove() {
        gameState = gameState == GameState.Red ? GameState.Blue : GameState.Red;
        //
        if (!PlayersHave3Caps((PlayerCap.PlayerType)gameState)) {
            GameEnd();
            return;
        }
        //
        if (!PlayerHaveCaps((PlayerCap.PlayerType)gameState)) {
            GameEnd();
            return;
        }
        //
        if (!PlayerCanMove((PlayerCap.PlayerType)gameState)) {
            GameEnd();
            return;
        }
        //
        switch (gameState) {
            case GameState.Red:
                AITurn = false;
                //
                gameScreen.redTitle.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 1f, 5)
                    .SetDelay(0.2f)
                    .OnComplete(() => {
                        if (redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0) { gameScreen.redPuls.SetActive(true); }
                    });
                gameScreen.PlayerTurn((int)gameState);
                break;
            case GameState.Blue:
                gameScreen.blueTitle.rectTransform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 1f, 5)
                    .SetDelay(0.2f)
                    .OnComplete(() => {
                        if (bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0 && gameType != GameType.PVE) { gameScreen.bluePuls.SetActive(true); }
                    });
                gameScreen.PlayerTurn((int)gameState);
                //
                if (gameType == GameType.PVE) {
                    AITurn = true;
                    AIGetAction();
                }
                break;
        }
    }
    //
    private bool PlayersHave3Caps(PlayerCap.PlayerType playerType) {
        if (playerType == PlayerCap.PlayerType.Red) {
            int red = 0;
            if (redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0) {
                red = 3;
            } else {
                red = redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField).Count;
            }
            return red >= 3;
        }
        //
        if (playerType == PlayerCap.PlayerType.Blue) {
            int blue = 0;
            if (bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count > 0) {
                blue = 3;
            } else {
                blue = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField).Count;
            }
            return blue >= 3;
        }
        //
        return true;
    }
    //
    private bool PlayerHaveCaps(PlayerCap.PlayerType playerType) {
        if (playerType == PlayerCap.PlayerType.Red) {
            return redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField || c.capState == PlayerCap.CapState.InPlayer).Count > 0;
        }
        //
        if (playerType == PlayerCap.PlayerType.Blue) {
            return bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField || c.capState == PlayerCap.CapState.InPlayer).Count > 0;
        }
        //
        return true;
    }
    //
    private bool PlayerCanMove(PlayerCap.PlayerType playerType) {
        if (!AllCapsOnField()) { return true; }
        if (playerType == PlayerCap.PlayerType.Red) {
            List<PlayerCap> redResult = redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField);
            int redMoveCaps = 0;
            for (int i = 0; i < redResult.Count; i++) {
                if (redResult[i].capsHolder.CanMoveCap()) {
                    redMoveCaps++;
                }
            }
            return redMoveCaps > 0;
        }
        //
        if (playerType == PlayerCap.PlayerType.Blue) {
            List<PlayerCap> blueResult = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField);
            int blueMoveCaps = 0;
            for (int i = 0; i < blueResult.Count; i++) {
                if (blueResult[i].capsHolder.CanMoveCap()) {
                    blueMoveCaps++;
                }
            }
            
            return blueMoveCaps > 0;
        }
        //
        return true;
    }
    //
    private bool AllCapsOnField() {
        int red = redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count;
        int blue = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count;
        return red == 0 && blue == 0;
    }
    //
    #region Place caps
    public void StartDragToField(int _playerType) {
        if (!isPlay) { return; }
        //
        PlayerCap.PlayerType playerType = (PlayerCap.PlayerType)_playerType;
        PlayerCap playerCap = null;
        //
        if ((int)playerType != (int)gameState) {
            return;
        }
        //
        draggedCap = null;
        //
        switch (playerType) {
            case PlayerCap.PlayerType.Red:
                playerCap = redPlayerCaps.Find(c => c.capState == PlayerCap.CapState.InPlayer);
                if (playerCap != null) {
                    draggedCap = playerCap;
                    draggableCap.GetComponent<Image>().sprite = redCapImage;
                }
                break;
            case PlayerCap.PlayerType.Blue:
                playerCap = bluePlayerCaps.Find(c => c.capState == PlayerCap.CapState.InPlayer);
                if (playerCap != null) {
                    draggedCap = playerCap;
                    draggableCap.GetComponent<Image>().sprite = blueCapImage;
                }
                break;
            default:
                draggedCap = null;
                draggableCap.gameObject.SetActive(false);
                break;
        }
        //
        if (draggedCap != null) {
            capIsDraged = true;
            capsHolders.ForEach(c => c.ShowPlace(draggedCap.playerType));
            draggableCap.gameObject.SetActive(true);
        }
    }
    //
    private bool CanPlaceCap(out CapsHolder capsHolder) {
        if (isReplaceCap) {
            for (int i = 0; i < prevCapHolder.neighbors.Count; i++) {
                if (prevCapHolder.neighbors[i].holderState == CapsHolder.HolderState.Free) {
                    if (RectTransformUtility.RectangleContainsScreenPoint(prevCapHolder.neighbors[i].rectTransform, Input.mousePosition, Camera.main)) {
                        capsHolder = prevCapHolder.neighbors[i];
                        return true;
                    }
                }
            }
        } else {
            for (int i = 0; i < capsHolders.Count; i++) {
                if (capsHolders[i].holderState == CapsHolder.HolderState.Free) {
                    if (RectTransformUtility.RectangleContainsScreenPoint(capsHolders[i].rectTransform, Input.mousePosition, Camera.main)) {
                        capsHolder = capsHolders[i];
                        return true;
                    }
                }
            }
        }
        //
        capsHolder = null;
        return false;
    }
    //
    private void RevertCapToPlayer(PlayerCap.PlayerType playerType) {
        RectTransform targetRect = playerType == PlayerCap.PlayerType.Red ? redCapsHolder : blueCapsHolder;
        Vector2 target = Utils.SwitchToRectTransform(targetRect, GetComponent<RectTransform>());
        blockInput = true;
        draggableCap.DOAnchorPos(target, 0.3f)
            .OnComplete(() => {
                draggableCap.gameObject.SetActive(false);
                blockInput = false;
            });
    }
    //
    private void RevertCapToPrev() {
        RectTransform targetRect = prevCapHolder.rectTransform;
        Vector2 target = Utils.SwitchToRectTransform(targetRect, GetComponent<RectTransform>());
        blockInput = true;
        draggableCap.DOAnchorPos(target, 0.3f)
            .OnComplete(() => {
                draggableCap.gameObject.SetActive(false);
                //
                prevCapHolder.RevertCap();
                prevCapHolder = null;
                isReplaceCap = false;

                blockInput = false;
            });
    }
    //
    public void TryReplaceCap(PlayerCap _draggedCap, CapsHolder capsHolder) {
        draggedCap = _draggedCap;
        draggableCap.GetComponent<Image>().sprite = draggedCap.playerType == PlayerCap.PlayerType.Red ? redCapImage : blueCapImage;
        draggableCap.gameObject.SetActive(true);
        //
        prevCapHolder = capsHolder;
        //
        isReplaceCap = true;
        capIsDraged = true;
    }
    //
    public void StopReplace() {
        isReplaceCap = false;
    }
    #endregion
    #region AI
    private void AIGetAction() {
        int inPlayerCaps = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count;
        if (inPlayerCaps > 0) {
            AIPlaceCapToField();
        } else {
            AIMoveCap();
        }
    }
    public void AIPlaceCapToField() {
        StartDragToField((int)GameState.Blue);
        //
        CapsHolder holder;
        if (!GetFreeHoldersInCombination(PlayerCap.PlayerType.Red, out holder)) {
            if (!GetFreeHoldersInCombination(PlayerCap.PlayerType.Blue, out holder)) {
                holder = capsHolders.Find(h => h.holderState == CapsHolder.HolderState.Free);
            }
        }

        draggableCap.position = gameScreen.placeCapBlue.position;
        draggableCap.DOMove(holder.transform.position, 0.5f).SetDelay(0.5f)
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                capIsDraged = false;

                if (!isReplaceCap) {
                    capsHolders.ForEach(c => c.HidePlace());
                }
                
                PlaceCap(draggedCap, holder);
            });
    }
    //
    private void AIMoveCap() {
        List<PlayerCap> canMoveCaps = bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField && c.capsHolder.CanMoveCap());
        canMoveCaps.Shuffle();

        CapsHolder holder = canMoveCaps[Random.Range(0, canMoveCaps.Count)].capsHolder;
        List<CapsHolder> freeHolders = holder.GetFreeHoldersForMove();

        CapsHolder targetHolder = freeHolders[Random.Range(0, freeHolders.Count)];

        TryReplaceCap(holder.playerCap, holder);

        holder.capImage.ChangeAlpha(0.4f);
        holder.neighbors.ForEach(c => c.ShowPlace(PlayerCap.PlayerType.Blue));

        draggableCap.position = holder.transform.position;
        draggableCap.DOMove(targetHolder.transform.position, 0.5f).SetDelay(0.5f)
            .SetEase(Ease.OutSine)
            .OnComplete(() => {
                capIsDraged = false;

                if (!isReplaceCap) {
                    capsHolders.ForEach(c => c.HidePlace());
                }

                PlaceCap(draggedCap, targetHolder);
                holder.neighbors.ForEach(c => c.HidePlace());
            });
    }
    //
    private void AITakeCap() {
        List<PlayerCap> canTakeCaps = redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.OnField);
        canTakeCaps.Shuffle();
        //
        PlayerCap takedCap = canTakeCaps[Random.Range(0, canTakeCaps.Count)];
        DOVirtual.DelayedCall(1f, () => {
            TakeCap(takedCap.capsHolder);
        });
    }
    #endregion
    #region Combinations
    private bool CheckCombination(CapsHolder lastPlaced, out int _count) {
        Combination combination;
        bool result = false;
        int count = 0;
        if (FindInCombination(lastPlaced, horizontals, out combination) == true) {
            result = true;
            count++;
        }

        if (FindInCombination(lastPlaced, verticals, out combination) == true) {
            result = true;
            count++;
        }

        if (FindInCombination(lastPlaced, diagonals, out combination) == true) {
            result = true;
            count++;
        }

        if (result == true) {
            gameState = lastPlaced.playerCap.playerType == PlayerCap.PlayerType.Red ? GameState.RedTakeCap : GameState.BlueTakeCap;
        }

        _count = count;

        return result;
    }
    //
    private bool GetFreeHoldersInCombination(PlayerCap.PlayerType playerType, out CapsHolder _capHolder) {
        CapsHolder capHolder = null;

        if (Random.value <= Random.Range(0f, 1f)) {
            _capHolder = null;
            return false;
        }

        CapsHolder.HolderState holder = playerType == PlayerCap.PlayerType.Red ? CapsHolder.HolderState.Red : CapsHolder.HolderState.Blue;

        if (FindAIHolderInCombination(horizontals, holder, out capHolder) == true) {
            _capHolder = capHolder;
            return true;
        }
        //
        if (FindAIHolderInCombination(verticals, holder, out capHolder) == true) {
            _capHolder = capHolder;
            return true;
        }
        //
        if (FindAIHolderInCombination(diagonals, holder, out capHolder) == true) {
            _capHolder = capHolder;
            return true;
        }
        //

        //
        _capHolder = capHolder;
        return false;
    }
    //
    private bool FindAIHolderInCombination(List<Combination> combinations, CapsHolder.HolderState holderState, out CapsHolder capHolder) {
        for (int c = 0; c < combinations.Count; c++) {
            int caps = combinations[c].capsHolders.FindAll(h => h.holderState == holderState).Count;
            List<CapsHolder> free = combinations[c].capsHolders.FindAll(h => h.holderState == CapsHolder.HolderState.Free);
            if (caps == 2 && free.Count == 1) {
                capHolder = free[0];
                return true;
            }
        }
        //
        capHolder = null;
        return false;
    }
    //
    private bool FindInCombination(CapsHolder lastPlaced, List<Combination> combinations, out Combination combination) {
        for (int i = 0; i < combinations.Count; i++) {
            if (combinations[i].capsHolders.Contains(lastPlaced)) {
                if (combinations[i].capsHolders.All(c => c.holderState != CapsHolder.HolderState.Free && c.playerCap.playerType == lastPlaced.playerCap.playerType)) {
                    combination = combinations[i];
                    return true;
                }
            }
        }
        //
        combination = new Combination();
        return false;
    }
    #endregion
}
//
[System.Serializable]
public struct Combination {

    public List<CapsHolder> capsHolders;

}