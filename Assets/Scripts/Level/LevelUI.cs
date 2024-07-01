using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelUI : MonoBehaviour {

    public Text AnnouncerTextLine1;
    public Text AnnouncerTextLine2;
    public Text LevelTimer;

    public Slider[] healthSliders;

    public GameObject[] winIndicatorGrids;
    public GameObject winIndicator;

    public static LevelUI instance;
    public static LevelUI GetInstance()
    {
        return instance;
    }

    public delegate void OnPlayerSliderUpdate(int _playerHealth);
    public OnPlayerSliderUpdate OnPlayerSliderUpdateCallback;

    public delegate void OnEnemySliderUpdate(int _enemyHealth);
    public OnEnemySliderUpdate OnEnemySliderUpdateCallback;

    void Awake()
    {
        instance = this;
    }

    void Start() {
        HidePlayerAnnouncerText();
        HideEnemyAnnouncerText();

        OnPlayerSliderUpdateCallback += UpdatePlayerHealth;
        OnEnemySliderUpdateCallback += UpdateEnemyHealth;
    }

    public void AddWinIndicator(int player)
    {
        GameObject go = Instantiate(winIndicator, transform.position, Quaternion.identity) as GameObject;
        go.transform.SetParent(winIndicatorGrids[player].transform);
    }

    void UpdatePlayerHealth(int _playerHealth) {
        if(_playerHealth < 0) {
            _playerHealth = 0;
        }
        healthSliders[0].value = _playerHealth;
    }

    void UpdateEnemyHealth(int _enemyHealth) {
        if(_enemyHealth < 0) {
            _enemyHealth = 0;
        }
        healthSliders[1].value = _enemyHealth;
    }

    public void ShowPlayerAnnouncerText() {
        AnnouncerTextLine1.gameObject.SetActive(true);
    }
    public void HidePlayerAnnouncerText() {
        AnnouncerTextLine1.gameObject.SetActive(false);
    }
    public void SetPlayerAnnouncerText(string _text) {
        AnnouncerTextLine1.text = _text;
    }

    public void ShowEnemyAnnouncerText() {
        AnnouncerTextLine2.gameObject.SetActive(true);
    }
    public void HideEnemyAnnouncerText() {
        AnnouncerTextLine2.gameObject.SetActive(false);
    }
    public void SetEnemyAnnouncerText(string _text) {
        AnnouncerTextLine2.text = _text;
    }
}
