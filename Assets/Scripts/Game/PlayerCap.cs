using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerCap {
    
    public enum PlayerType {
        Red,
        Blue
    }

    public enum CapState {
        InPlayer,
        OnField,
        Taken
    }

    [SerializeField]
    public PlayerType playerType;

    [SerializeField]
    public CapState capState;

    [SerializeField]
    public CapsHolder capsHolder;

}