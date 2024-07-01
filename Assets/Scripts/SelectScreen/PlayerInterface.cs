using UnityEngine;

[System.Serializable] public class PlayerInterface {
    public PortraitInfo activePortrait;
    public PortraitInfo previewPortrait;
    public GameObject selector;
    public Transform characterVisualPosition;
    public GameObject characterObject;
    public int activeX;
    public int activeY;
    public PlayerInfo playerInfo;
    public bool selectorMovingHorizontal = false;
    public bool selectorMovingVertical = false;
}