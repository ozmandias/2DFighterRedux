using UnityEngine;

public class CombatManager : MonoBehaviour {
    LevelUI combatLevelUI;

    void Start() {
        combatLevelUI = LevelUI.GetInstance();
    }

    public void CalculateCombat(Component _attacker, Component _receiver, int _damage) {

    }

    public void TakeDamage(Component _receiver, int _damage) {
        // Debug.Log("TakeDamage");
        if(_receiver is Player) {
            // Debug.Log("Player takes damage: " + _damage);
            _receiver.GetComponent<Player>().health -= _damage;
            if(_receiver.gameObject.name == "Player" && combatLevelUI.OnPlayerSliderUpdateCallback != null) {
                combatLevelUI.OnPlayerSliderUpdateCallback.Invoke(_receiver.GetComponent<Player>().health);
            }else if(_receiver.gameObject.name == "Enemy" && combatLevelUI.OnEnemySliderUpdateCallback != null) {
                combatLevelUI.OnEnemySliderUpdateCallback.Invoke(_receiver.GetComponent<Player>().health);
            }
            if(_receiver.GetComponent<Player>().health <= 0 && _receiver.GetComponent<Player>().isKnockedOut == false) {
                if(_receiver.gameObject.name == "Player") {
                    combatLevelUI.SetEnemyAnnouncerText("Player 2 wins!");
                    combatLevelUI.ShowEnemyAnnouncerText();
                } else if(_receiver.gameObject.name == "Enemy") {
                    combatLevelUI.SetPlayerAnnouncerText("Player 1 wins!");
                    combatLevelUI.ShowPlayerAnnouncerText();
                }
                KnockOut(_receiver);
            }
        } else if(_receiver is Enemy) {
            // Debug.Log("Enemy takes damage: " + _damage);
            _receiver.GetComponent<Enemy>().health -= _damage;
            if(combatLevelUI.OnEnemySliderUpdateCallback != null) {
                combatLevelUI.OnEnemySliderUpdateCallback.Invoke(_receiver.GetComponent<Enemy>().health);
            }
            if(_receiver.GetComponent<Enemy>().health <= 0 && _receiver.GetComponent<Enemy>().isKnockedOut == false) {
                combatLevelUI.SetPlayerAnnouncerText("Player 1 wins!");
                combatLevelUI.ShowPlayerAnnouncerText();
                KnockOut(_receiver);
            }
        }
    }

    void KnockOut(Component _receiver) {
        // Debug.Log("KnockedOut");
        if(_receiver is Player) {
            // Debug.Log("Player K.O");
            _receiver.GetComponent<Player>().KnockOutPlayer();
        } else if(_receiver is Enemy) {
            // Debug.Log("Enemy K.O");
            _receiver.GetComponent<Enemy>().KnockOutEnemy();
        }
    }
}