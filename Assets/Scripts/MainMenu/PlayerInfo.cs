using UnityEngine;

[System.Serializable] public class PlayerInfo {
    public string playerName="";
    public string playerInputID="";
    public PlayerType playerType;
    public GameObject playerPrefab;
    public CharacterInfo selectedCharacter;
    public int score=0;
}

public enum PlayerType {
    User,
    AI,
    Simulation
}