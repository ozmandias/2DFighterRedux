using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour {
    [SerializeField] Text pressStartText;
    [SerializeField] GameObject menuUIObject;
    bool gameStarts = false;
    bool selectGameMode = false;
    int gameMode = 0;
    [SerializeField] ButtonRef[] gameModeButtons;
    GameManager introGameManager;

    void Start() {
        introGameManager = GameManager.instance;

        gameStarts = true;
        menuUIObject.SetActive(false);
        StartCoroutine(FlickerPressStartText());
    }

    void Update() {
        Select();
        CheckSelectGameMode();
    }

    void CheckSelectGameMode() {
        if(Input.GetKeyDown(KeyCode.Return)) {
            selectGameMode = true;
            menuUIObject.SetActive(true);
            pressStartText.gameObject.SetActive(false);
        } else if(Input.GetKeyDown(KeyCode.Escape)) {
            selectGameMode = false;
            menuUIObject.SetActive(false);
        }
    }

    void Select() {
        if(selectGameMode == true) {
            if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
                if(gameMode == 0) {
                    gameMode = 1;
                } else {
                    gameMode = 0;
                }
            } else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
                if(gameMode == 1) {
                    gameMode = 0;
                } else {
                    gameMode = 1;
                }
            }
        
            for(int i = 0; i < gameModeButtons.Length; i = i + 1) {
                if(i == gameMode) {
                    gameModeButtons[i].selected = true;
                } else {
                    gameModeButtons[i].selected = false;
                }
            }

            if(Input.GetKeyDown(KeyCode.Return)) {
                Debug.Log("Select Game");
                if(gameMode == 0) {
                    introGameManager.gameMode = GameMode.Singleplayer;
                    introGameManager.totalPlayers = 1;
                    CharacterManager.instance.SetPlayers(introGameManager.totalPlayers);
                } else if (gameMode == 1) {
                    introGameManager.gameMode = GameMode.Multiplayer;
                    introGameManager.totalPlayers = 2;
                    CharacterManager.instance.SetPlayers(introGameManager.totalPlayers);
                }
                SceneManager.LoadScene("select");
            }
        }
    }

    IEnumerator FlickerPressStartText() {
        while(gameStarts) {
            if(selectGameMode == false) {
                pressStartText.gameObject.SetActive(true);
                yield return new WaitForSeconds(0.3f);
                pressStartText.gameObject.SetActive(false);
                yield return new WaitForSeconds(0.3f);
            } else {
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}