using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterManager : MonoBehaviour {
    [SerializeField] int playerCount = 0;
    [SerializeField] List<PlayerInfo> players = new List<PlayerInfo>();
    [SerializeField] List<CharacterInfo> characters = new List<CharacterInfo>();

    #region Singleton
        public static CharacterManager instance;
        void Awake() {
            if(instance != null){
                Destroy(this.gameObject);
            } else {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
        }
    #endregion

    public void SetPlayers(int _playerCount) {
        playerCount = _playerCount;
        if(playerCount == 1) {
            PlayerInfo player1 = new PlayerInfo();
            player1.playerName = "Player 1";
            player1.playerType = PlayerType.User;
            players.Add(player1);

            PlayerInfo computer = new PlayerInfo();
            computer.playerName = "Computer";
            computer.playerType = PlayerType.AI;
            players.Add(computer);
        } else if (playerCount > 1) {
            for(int i = 0; i < playerCount; i = i + 1) {
                PlayerInfo newPlayer = new PlayerInfo();
                newPlayer.playerName = "Player " + (i+1);
                if(i > 0) {
                    newPlayer.playerInputID = "" + i;
                }
                newPlayer.playerType = PlayerType.User;
                players.Add(newPlayer);
            }
        } /*else if (playerCount == 0) {
            players.Clear();
        }*/
    }
    
    public CharacterInfo GetCharacterWithID(string _id) {
        CharacterInfo returnCharacter = null;
        foreach(CharacterInfo character in characters) {
            if(string.Equals(character.id,_id)) {
                returnCharacter = character;
                break;
            }
        }
        return returnCharacter;
    }

    public PlayerInfo GetPlayer(int _playerPosition) {
        PlayerInfo returnPlayer = players[_playerPosition];
        return returnPlayer;
    }

    public void ClearData() {
        playerCount = 0;
        players.Clear();
    }
}