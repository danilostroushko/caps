using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CapsHolder : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    
    public enum HolderState {
        Free,
        Red,
        Blue
    }

    public HolderState holderState;

    public List<CapsHolder> neighbors = new List<CapsHolder>();

    public Image capImage;
    public Image takeCap;

    public Sprite redCap;
    public Sprite blueCap;

    public DOTweenAnimation capAnimation;

    public PlayerCap playerCap = null;

    [HideInInspector]
    public RectTransform rectTransform;
    
    private void OnDrawGizmos() {
        if (neighbors.Count > 0) {
            Gizmos.color = Color.red;
            for (int i = 0; i < neighbors.Count; i++) {
                Gizmos.DrawLine(transform.position, Vector3.Lerp(transform.position, neighbors[i].transform.position, 0.3f));
            }
        }
        //
        if (neighbors.Count == 0) {
            Gizmos.color = Color.red;
        } else {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawSphere(transform.position, 0.05f);
    }

    void Awake() {
        rectTransform = GetComponent<RectTransform>();
        //
        capImage.enabled = false;
    }

    public void StartReplace() {
        
    }

    public void PlaceCap(PlayerCap _playerCap) {
        playerCap = _playerCap;
        playerCap.capsHolder = this;
        playerCap.capState = PlayerCap.CapState.OnField;
        //
        holderState = playerCap.playerType == PlayerCap.PlayerType.Red ? HolderState.Red : HolderState.Blue;
        //
        capImage.sprite = playerCap.playerType == PlayerCap.PlayerType.Red ? redCap : blueCap;
        //
        capImage.ChangeAlpha(1f);
        capImage.enabled = true;
    }

    public void Clear() {
        playerCap = null;
        holderState = HolderState.Free;
        capImage.ChangeAlpha(0f);
        capImage.enabled = false;
    }

    public void ShowPlace(PlayerCap.PlayerType playerType) {
        if (holderState == HolderState.Free) {
            capImage.sprite = playerType == PlayerCap.PlayerType.Red ? redCap : blueCap;
            capImage.ChangeAlpha(0.4f);
            capImage.enabled = true;
        }
    }
    //
    public void HidePlace() {
        if (holderState == HolderState.Free) {
            capImage.ChangeAlpha(1f);
            capImage.enabled = false;
        }
    }

    public void ShowCanTakeCap(PlayerCap.PlayerType playerType) {
        if (holderState == HolderState.Free || playerCap.playerType != playerType) {
            return;
        }
        //
        takeCap.sprite = playerCap.playerType == PlayerCap.PlayerType.Red ? redCap : blueCap;
        takeCap.gameObject.SetActive(true);
    }

    public void HideCanTakeCap() {
        takeCap.gameObject.SetActive(false);
    }

    public void RevertCap() {
        capImage.ChangeAlpha(1f);
    }

    public bool CanMoveCap() {
        return neighbors.FindAll(c => c.holderState == HolderState.Free).Count > 0;
    }

    public List<CapsHolder> GetFreeHoldersForMove() {
        return neighbors.FindAll(n => n.holderState == HolderState.Free);
    }

    #region Interface
    public void OnPointerDown(PointerEventData eventData) {
        if (GameController.instance.blockInput || !GameController.instance.isPlay) { return; }
        //
        if (GameController.instance.gameState == GameController.GameState.RedTakeCap) {
            if (playerCap != null && playerCap.playerType == PlayerCap.PlayerType.Blue) {
                GameController.instance.TakeCap(this);
                return;
            }
        }
        //
        if (GameController.instance.gameState == GameController.GameState.BlueTakeCap) {
            if (playerCap != null && playerCap.playerType == PlayerCap.PlayerType.Red) {
                GameController.instance.TakeCap(this);
                return;
            }
        }
        //
        //
        //
        if (playerCap != null && (int)playerCap.playerType != (int)GameController.instance.gameState) {
            return;
        }
        //
        if (holderState == HolderState.Red) {
            if (GameController.instance.redPlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count != 0) {
                return;
            }
        }
        //
        if (holderState == HolderState.Blue) {
            if (GameController.instance.bluePlayerCaps.FindAll(c => c.capState == PlayerCap.CapState.InPlayer).Count != 0) {
                return;
            }
        }
        //
        if (holderState != HolderState.Free && neighbors.FindAll(c => c.holderState == HolderState.Free).Count > 0) {
            GameController.instance.TryReplaceCap(playerCap, this);
            //
            capImage.ChangeAlpha(0.4f);
            //
            neighbors.ForEach(c => c.ShowPlace(playerCap.playerType));
        }
    }

    public void OnPointerUp(PointerEventData eventData) {
        neighbors.ForEach(c => c.HidePlace());
    }
    #endregion
}