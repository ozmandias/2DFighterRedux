using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]Animator playerAnimator;
    [SerializeField]SpriteRenderer playerRenderer;
    [SerializeField]GameObject playerRaycastObject;
    public int punchForce = 6;
    public int kickForce = 8;
    float speed = 1f;
    float jumpForce = 10f;
    public string attackAnimationName = "";
    float attackAnimationDuration = 0f;
    [SerializeField]bool isAttacking = false;
    [SerializeField]bool isBlocking = false;
    [SerializeField]bool isCrouching = false;
    [SerializeField]bool isHit = false;
    public bool isKnockedOut = false;
    int playerCombos = 0;
    float attackTimer = 0f;
    float hitTimer = 0f;
    GameManager playerGameManager;
    [SerializeField]CombatManager playerCombatManager;
    string[] currentAnimationNames = {"BodyPunch", "LowKick", "HeadKick", "HeadPunch", "LowPunch", "FrontKick"};
    public int health = 100;
    int playerLayer = 1 << 9;
    int otherPlayerLayer = 1 << 10;
    int playerLayerMask;
    int otherPlayerLayerMask;
    string playerInputID = "";
    [SerializeField]Enemy enemyFighter;
    [SerializeField]GameObject target;
    
    void Start() {
        playerAnimator = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        playerRenderer = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        playerGameManager = GameManager.instance;
        playerCombatManager = gameObject.GetComponent<CombatManager>();
        playerRaycastObject = this.gameObject;

        playerLayerMask = ~playerLayer;
        otherPlayerLayerMask = ~otherPlayerLayer;

        enemyFighter = GameObject.Find("Enemy").GetComponent<Enemy>();
        // target = GameObject.Find("Enemy");
    }

    void Update() {
        Move();
        Attack();
        PlayerStatus();
    }

    void FixedUpdate() {
        CheckEnemy();
    }

    void Move() {
        if(isHit == false && isKnockedOut == false) {
            float horizontal = Input.GetAxis("Horizontal" + playerInputID);
            // float vertical = Input.GetAxis("Vertical");
            
            Vector3 direction = new Vector3(horizontal,0, 0) * speed * Time.deltaTime;

            if(Input.GetButtonDown("Jump" + playerInputID) /*Input.GetKeyDown(KeyCode.W)*/) {
                Play_Jump_Animation();
                direction.y = direction.y + jumpForce * Time.deltaTime;
                if(enemyFighter != null) {
                    enemyFighter.AddToActionList("Jump");
                }
            }

            if(Input.GetButtonDown("Crouch" + playerInputID) /*Input.GetKey(KeyCode.S)*/) {
                isCrouching = true;
                Stop_Move_Animation();
                Play_Crouch_Animation();
            } else if(Input.GetButtonUp("Crouch" + playerInputID) /*Input.GetKeyUp(KeyCode.S)*/) {
                isCrouching = false;
                Stop_Crouch_Animation();
            }

            if(isCrouching != true && isBlocking != true){
                if((horizontal > 0 && horizontal <= 1) || (horizontal < 0 && horizontal >= -1)) {
                    Play_Move_Animation();
                } else if (horizontal == 0) {
                    Stop_Move_Animation();
                }
                gameObject.transform.position = gameObject.transform.position + direction;
            }
        }

        if(gameObject.transform.position.x > 1.6f) {
            gameObject.transform.position = new Vector3(1.6f, gameObject.transform.position.y, 0);
        } else if(gameObject.transform.position.x < -1.6f) {
            gameObject.transform.position = new Vector3(-1.6f, gameObject.transform.position.y, 0);
        }
    }

    void Attack() {
        if(isHit == false && isKnockedOut == false) {
            attackTimer = attackTimer + Time.deltaTime;

            // Check Attack
            if(Input.GetButtonDown("HeadPunch" + playerInputID) || Input.GetButtonDown("BodyPunch" + playerInputID) || Input.GetButtonDown("LowPunch" + playerInputID) || Input.GetButtonDown("HeadKick" + playerInputID) || Input.GetButtonDown("FrontKick" + playerInputID) || Input.GetButtonDown("LowKick" + playerInputID)) {
            // if(Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.F) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.X)) {
                isAttacking = true;
                playerCombos = playerCombos + 1;
                if(enemyFighter != null) {
                    enemyFighter.AddToActionList("Attack");
                }
            }

            if(isAttacking == true) {
                playerRenderer.sortingOrder = 1;
                AnimationClip currentAnimationClip = playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
                string currentAnimationName = currentAnimationClip.name;
                if(currentAnimationName == "HeadPunch" || currentAnimationName == "MiddlePunch" || currentAnimationName == "LowPunch" || currentAnimationName == "HeadKick" || currentAnimationName == "FrontKick" || currentAnimationName == "LowKick" ||
                currentAnimationName == "ChozenPunch" || currentAnimationName == "ChozenKick" || currentAnimationName == "ChozenCrouchPunch" || currentAnimationName == "ChozenCrouchKick" ||
                currentAnimationName == "EliHawkPunch" || currentAnimationName == "EliHawkKick" || currentAnimationName == "EliHawkCrouchPunch" || currentAnimationName == "EliHawkCrouchKick" ||
                currentAnimationName == "PunkDoublePunch" || currentAnimationName == "PunkKick" ||
                currentAnimationName == "CyborgDoublePunch" || currentAnimationName == "CyborgKick") {
                    // attackAnimationDuration = (playerCombos * 0.1f) + 0.4f;
                    attackAnimationName = currentAnimationName;
                    // Debug.Log("attackAnimationName: " + attackAnimationName);
                    attackAnimationDuration = currentAnimationClip.length;
                    StartCoroutine(AttackCoroutine());
                    attackTimer = 0;
                }
            } else {
                playerRenderer.sortingOrder = 0;
            }

            // Punches
            if(Input.GetButtonDown("HeadPunch" + playerInputID) /*Input.GetKeyDown(KeyCode.R)*/) {
                Play_HeadPunch_Animation();
            }
            if(Input.GetButtonDown("BodyPunch" + playerInputID) /*Input.GetKeyDown(KeyCode.F)*/) {
                Play_BodyPunch_Animation();
            }
            if(Input.GetButtonDown("LowPunch" + playerInputID) /*Input.GetKeyDown(KeyCode.C)*/) {
                Play_LowPunch_Animation();
            }

            // Kicks
            if(Input.GetButtonDown("HeadKick" + playerInputID) /*Input.GetKeyDown(KeyCode.E)*/) {
                Play_HeadKick_Animation();
            }
            if(Input.GetButtonDown("FrontKick" + playerInputID) /*Input.GetKeyDown(KeyCode.G)*/) {
                Play_FrontKick_Animation();
            }
            if(Input.GetButtonDown("LowKick" + playerInputID) /*Input.GetKeyDown(KeyCode.X)*/) {
                Play_LowKick_Animation();
            }
        }

        if(isKnockedOut == false) {
            // Check Block
            if(Input.GetButtonDown("HighBlock" + playerInputID) || Input.GetButtonDown("BodyBlock" + playerInputID)) {
                isBlocking = true;
            } else if(Input.GetButtonUp("HighBlock" + playerInputID) || Input.GetButtonUp("BodyBlock" + playerInputID)) {
                isBlocking = false;
            }

            // Blocks
            if(Input.GetButtonDown("HighBlock" + playerInputID) /*Input.GetKey(KeyCode.Tab)*/) {
                Stop_Move_Animation();
                Play_HighBlock_Animation();
            } else if(Input.GetButtonUp("HighBlock" + playerInputID) /*Input.GetKeyUp(KeyCode.Tab)*/) {
                Stop_HighBlock_Animation();
            }

            if(Input.GetButtonDown("BodyBlock" + playerInputID) /*Input.GetKey(KeyCode.LeftShift)*/) {
                Stop_Move_Animation();
                Play_BodyBlock_Animation();
            } else if(Input.GetButtonUp("BodyBlock" + playerInputID) /*Input.GetKeyUp(KeyCode.LeftShift)*/) {
                Stop_BodyBlock_Animation();
            }
        }
    }

    void CheckEnemy() {
        if(isKnockedOut == false) {
            float raycastDistance = 10f;
            Ray frontRay = new Ray(playerRaycastObject.transform.position, transform.right);
            Ray backRay = new Ray(playerRaycastObject.transform.position, -transform.right);

            RaycastHit2D frontRaycastHit = Physics2D.Raycast(playerRaycastObject.transform.position, Vector2.right, raycastDistance, playerInputID == "" ? playerLayerMask : otherPlayerLayerMask);
            RaycastHit2D backRaycastHit = Physics2D.Raycast(playerRaycastObject.transform.position, Vector2.left, raycastDistance, playerInputID == "" ? playerLayerMask : otherPlayerLayerMask);
            
            // Debug.DrawRay(frontRay.origin, frontRay.direction * raycastDistance);
            // Debug.DrawRay(backRay.origin, backRay.direction * raycastDistance);

            if(frontRaycastHit.collider != null) {
                // Debug.Log("Enemy ahead");
                // Debug.Log("hitObject: " + frontRaycastHit.collider.gameObject.name);
                if(frontRaycastHit.collider.gameObject.CompareTag("Enemy") || frontRaycastHit.collider.gameObject.CompareTag("Player")) {
                    // Debug.Log("frontRaycastHit-GameObject: " + frontRaycastHit.collider.gameObject + ", currentGameObject: " + gameObject);
                    // playerRenderer.flipX = false;
                    gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
                }
            } else if(backRaycastHit.collider != null) {
                // Debug.Log("Enemy behind");
                if(backRaycastHit.collider.gameObject.CompareTag("Enemy") || backRaycastHit.collider.gameObject.CompareTag("Player")) {
                    // Debug.Log("backRaycastHit-GameObject: " + backRaycastHit.collider.gameObject + ", currentGameObject: " + gameObject);
                    // playerRenderer.flipX = true;
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    void PlayerStatus() {
        if(isKnockedOut == false) {
            AnimationClip currentAnimationClip = playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
            string currentAnimationName = currentAnimationClip.name;
            if(isHit == true) {
                if(currentAnimationName == "Default" || currentAnimationName == "Move" ||
                currentAnimationName == "ChozenDefault" || currentAnimationName == "ChozenMove" ||
                currentAnimationName == "EliHawkDefault" || currentAnimationName == "EliHawkMove" ||
                currentAnimationName == "PunkDefault" || currentAnimationName == "PunkMove" ||
                currentAnimationName == "CyborgDefault" || currentAnimationName == "CyborgMove") {
                        // Debug.Log("Play_Hit_Animation");
                        Play_Hit_Animation();
                }
                if(currentAnimationName == "Hit" ||
                currentAnimationName == "ChozenHit" ||
                currentAnimationName == "EliHawkHit" ||
                currentAnimationName == "PunkHit" ||
                currentAnimationName == "CyborgHit") {
                        hitTimer = hitTimer + Time.deltaTime;
                        if(hitTimer > currentAnimationClip.length) {
                            hitTimer = 0;
                            isHit = false;
                        }
                }
            }
        }
    }

    public void KnockOutPlayer() {
        isKnockedOut = true;
        Set_Default_Animation();
        Play_Fall_Animation();
    }

    // Values change
    public void SetPlayerInputID(string _inputID) {
        playerInputID = _inputID;
    }

    public void SetTarget(GameObject _target) {
        target = _target;
    }

    // Components change
    public void SetAnimator(Animator _animator) {
        playerAnimator = _animator;
    }

    public void SetSpriteRenderer(SpriteRenderer _spriteRenderer) {
        playerRenderer = _spriteRenderer;
    }

    // Animations
    void Play_Move_Animation() {
        playerAnimator.SetBool("Move", true);
    }

    void Stop_Move_Animation() {
        playerAnimator.SetBool("Move", false);
    }

    void Play_Jump_Animation() {
        playerAnimator.SetTrigger("Jump");
    }

    void Play_HeadPunch_Animation() {
        playerAnimator.SetTrigger("HeadPunch");
    }

    void Play_BodyPunch_Animation() {
        playerAnimator.SetTrigger("BodyPunch");
    }

    void Play_LowPunch_Animation() {
        playerAnimator.SetTrigger("LowPunch");
    }

    void Play_HeadKick_Animation() {
        playerAnimator.SetTrigger("HeadKick");
    }

    void Play_FrontKick_Animation() {
        playerAnimator.SetTrigger("FrontKick");
    }

    void Play_LowKick_Animation() {
        playerAnimator.SetTrigger("LowKick");
    }

    void Play_Crouch_Animation() {
        playerAnimator.SetBool("Crouch", true);
    }

    void Stop_Crouch_Animation() {
        playerAnimator.SetBool("Crouch", false);
    }

    void Play_HighBlock_Animation() {
        playerAnimator.SetBool("HighBlock", true);
    }

    void Stop_HighBlock_Animation() {
        playerAnimator.SetBool("HighBlock", false);
    }

    void Play_BodyBlock_Animation() {
        playerAnimator.SetBool("BodyBlock", true);
    }

    void Stop_BodyBlock_Animation() {
        playerAnimator.SetBool("BodyBlock", false);
    }

    void Set_Default_Animation() {
        playerAnimator.Play("Default");
    }

    void Play_Hit_Animation() {
        playerAnimator.Play("Hit");
    }

    void Play_Fall_Animation() {
        playerAnimator.Play("Fall");
    }

    public void Resume_All_Animations() {
        playerAnimator.speed = 1;
    }

    public void Stop_All_Animations() {
        playerAnimator.speed = 0;
    }

    // Collisions
    void OnCollisionEnter2D(Collision2D objectCollision) {
        if(objectCollision.collider.gameObject.CompareTag("AttackCollider")) {
            if(isBlocking == false && isKnockedOut == false) {
                // Debug.Log("PlayerHit");
                isHit = true;
                Set_Default_Animation();
                int incomingDamage = 0;
                if(playerGameManager.gameMode == GameMode.Singleplayer) {
                    // Enemy enemyFighter = target.GetComponent<Enemy>();
                    // Debug.Log("enemyFighter.attackAnimationName: " + enemyFighter.attackAnimationName);
                    incomingDamage = enemyFighter.attackAnimationName.Contains("Punch") ? enemyFighter.punchForce : enemyFighter.kickForce;
                    // Debug.Log("incomingDamage: " + incomingDamage);
                } else if(playerGameManager.gameMode == GameMode.Multiplayer) {
                    Player opponentFighter = target.GetComponent<Player>();
                    // Debug.Log("opponentFighter.attackAnimationName: " + opponentFighter.attackAnimationName);
                    incomingDamage = opponentFighter.attackAnimationName.Contains("Punch") ? opponentFighter.punchForce : opponentFighter.kickForce;
                    // Debug.Log("incomingDamage:" + incomingDamage);
                }
                playerCombatManager.TakeDamage(this, incomingDamage);
            }
        }
    }

    IEnumerator AttackCoroutine() {
        yield return new WaitForSeconds(attackAnimationDuration);

        if(attackTimer < 0.1f) {
            // Debug.Log("Multiple Combo");
            yield break;
        } 
        else {
            if(playerCombos == 1) {
                // Debug.Log("Single Combo");
            }
        }

        // Debug.Log("Stop Attacking");
        isAttacking = false;
        attackAnimationName = "";
        playerCombos = 0;
        attackAnimationDuration = 0;
    }
}