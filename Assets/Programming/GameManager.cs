using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public int totalPlayers = 0;
    public Dictionary<PlayerInfo, int> playerDictionary = new Dictionary<PlayerInfo, int>();
    public GameMode gameMode;

    #region Singleton
        public static GameManager instance;

        void Awake() {
            if(instance == null) {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }else if(instance != this){
                Destroy(this.gameObject);
            }
        }
    #endregion

    public KeyValuePair<PlayerInfo, int> GetDictionaryData(int _dataPosition) {
        int dataPosition = 0;
        KeyValuePair<PlayerInfo, int> returnKeyValuePair = new KeyValuePair<PlayerInfo, int>(null, -1);
        foreach(KeyValuePair<PlayerInfo, int> playerKeyValuePair in playerDictionary) {
            if(dataPosition == _dataPosition) {
                returnKeyValuePair = playerKeyValuePair;
                break;
            }
            dataPosition = dataPosition + 1;
        }
        return returnKeyValuePair;
    }
}

public enum GameMode {
    Singleplayer,
    Multiplayer,
    None
}