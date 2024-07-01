using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {
    public GameObject[] fighters;
    GameManager levelGameManager;
    public GameObject player1Object;
    public GameObject player2Object;

    void Start() {
        levelGameManager = GameManager.instance;

        int fighterCount = levelGameManager.playerDictionary.Count;
        if(fighterCount == 1) {
            fighterCount = 2;
        }
        fighters = new GameObject[fighterCount];

        for(int i = 0; i < fighterCount; i = i + 1){
            // assign playerGameObjects.
            KeyValuePair<PlayerInfo, int> fighterKeyValuePair = levelGameManager.GetDictionaryData(i);
            // Debug.Log("playerName: " + fighterKeyValuePair.Key.playerName);
            if(fighterKeyValuePair.Key.playerName == "Player 1") {
                fighters[i] = player1Object;
            } else {
                fighters[i] = player2Object;
            }

            // show/hide characters.
            GameObject fighterArtObject = fighters[i].transform.GetChild(0).gameObject;
            // Debug.Log("fighterArtObject: " + fighterArtObject.name);
            int charactersCount = fighterArtObject.transform.childCount;
            // Debug.Log("charactersCount: " + charactersCount);
            for(int j = 0; j < charactersCount; j = j + 1) {
                GameObject currentCharacter = fighterArtObject.transform.GetChild(j).gameObject;
                // Debug.Log("Character: " + currentCharacter.name);

                if(j == fighterKeyValuePair.Value) {
                    // Debug.Log("Selected Character: " + currentCharacter.name);
                    currentCharacter.SetActive(true);
                    currentCharacter.transform.SetAsFirstSibling();
                    if(fighterKeyValuePair.Key.playerType == PlayerType.User) {
                        if(fighterKeyValuePair.Key.playerName=="Player 1") {
                            Player playerProgramming = fighters[i].GetComponent<Player>();
                            playerProgramming.SetAnimator(currentCharacter.GetComponent<Animator>());
                            playerProgramming.SetSpriteRenderer(currentCharacter.GetComponent<SpriteRenderer>());
                            playerProgramming.SetPlayerInputID(fighterKeyValuePair.Key.playerInputID);
                            playerProgramming.SetTarget(GameObject.Find("Enemy"));
                        } else {
                            Destroy(fighters[i].GetComponent<Enemy>());
                            Player otherPlayerProgramming = fighters[i].AddComponent<Player>();
                            otherPlayerProgramming.SetAnimator(currentCharacter.GetComponent<Animator>());
                            otherPlayerProgramming.SetSpriteRenderer(currentCharacter.GetComponent<SpriteRenderer>());
                            otherPlayerProgramming.SetPlayerInputID(fighterKeyValuePair.Key.playerInputID);
                            otherPlayerProgramming.SetTarget(GameObject.Find("Player"));
                        }
                    } else {
                        Enemy enemyProgramming = fighters[i].GetComponent<Enemy>();
                        enemyProgramming.SetEnemyInfo(fighterKeyValuePair.Key);
                        enemyProgramming.SetAnimator(currentCharacter.GetComponent<Animator>());
                        enemyProgramming.SetSpriteRenderer(currentCharacter.GetComponent<SpriteRenderer>());
                    }
                } else {
                    currentCharacter.SetActive(false);
                }
            }
        }
    }

    void Update() {

    }

    public void ResumeGame() {
        Debug.Log("ResumeGame");
        for(int i=0; i<fighters.Length; i=i+1) {
            KeyValuePair<PlayerInfo, int> fighterKeyValuePair = levelGameManager.GetDictionaryData(i);
            if(fighterKeyValuePair.Key.playerType == PlayerType.User) {
                Player fighterPlayer = fighters[i].GetComponent<Player>();
                fighterPlayer.enabled = true;
                fighterPlayer.Resume_All_Animations();
            } else {
                Enemy fighterEnemy = fighters[i].GetComponent<Enemy>();
                fighterEnemy.enabled = true;
                fighterEnemy.Resume_All_Animations();
            }
        }
    }

    public void PauseGame() {
        Debug.Log("PauseGame");
        for(int i=0; i<fighters.Length; i=i+1) {
            KeyValuePair<PlayerInfo, int> fighterKeyValuePair = levelGameManager.GetDictionaryData(i);
            if(fighterKeyValuePair.Key.playerType == PlayerType.User) {
                Player fighterPlayer = fighters[i].GetComponent<Player>();
                fighterPlayer.Stop_All_Animations();
                fighterPlayer.enabled = false;
            } else {
                Enemy fighterEnemy = fighters[i].GetComponent<Enemy>();
                fighterEnemy.Stop_All_Animations();
                fighterEnemy.enabled = false;
            }
        }
    }
}