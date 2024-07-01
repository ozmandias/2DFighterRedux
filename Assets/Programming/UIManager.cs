using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour {
    [SerializeField]GameObject pauseMenu;
    [SerializeField]GameObject confirmExit;
    [SerializeField]LevelManager uiLevelManager;
    GameManager uiGameManager;
    bool pauseGame = false;

    public delegate void OnPauseGame();
    public OnPauseGame OnPauseGameCallback;

    public delegate void OnResumeGame();
    public OnResumeGame OnResumeGameCallback;

    void Start() {
        uiGameManager = GameManager.instance;

        HidePauseMenu();
        HideConfirmExit();

        OnPauseGameCallback = OnPauseGameCallback + uiLevelManager.PauseGame;
        OnResumeGameCallback = OnResumeGameCallback + uiLevelManager.ResumeGame;
    }

    void Update() {
        TogglePauseMenu();
    }

    public void TogglePauseMenu() {
        if(Input.GetKeyDown(KeyCode.Escape)) {
            if(pauseMenu.active == false && confirmExit.active == false) {
                pauseGame = true;
                ShowPauseMenu();
                if(OnPauseGameCallback != null) {
                    OnPauseGameCallback.Invoke();
                }
            } else if(pauseMenu.active == false && confirmExit.active == true) {
                HideConfirmExit();
            } else {
                UIResumeGame();
            }
        }
    }

    public void ShowPauseMenu() {
        pauseMenu.SetActive(true);
    }
    public void HidePauseMenu() {
        pauseMenu.SetActive(false);
    }

    public void ShowConfirmExit() {
        confirmExit.SetActive(true);
        HidePauseMenu();
    }
    public void HideConfirmExit() {
        confirmExit.SetActive(false);
        if(pauseGame == true) {
            ShowPauseMenu();
        }
    }

    public void UIResumeGame() {
        pauseGame = false;
        HidePauseMenu();
        if(OnResumeGameCallback != null) {
            OnResumeGameCallback.Invoke();
        }
    }
    public void UIExitGame() {
        StartCoroutine(ExitGameCoroutine());
    }

    IEnumerator ExitGameCoroutine() {
        uiGameManager.playerDictionary.Clear();
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("select");
    }
}