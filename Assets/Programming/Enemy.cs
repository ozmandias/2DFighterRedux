using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {
    [SerializeField]Animator enemyAnimator;
    [SerializeField]SpriteRenderer enemyRenderer;
    [SerializeField]GameObject enemyRaycastObject;
    GameObject target;
    float targetDistance;
    [SerializeField]List<string> targetActionList = new List<string>();
    [SerializeField]List<string> enemyActionList = new List<string>();
    string[] attackAnimationNames = {"MiddlePunch", "LowPunch", "HeadKick", "LowKick"} /*{"EnemyPunch", "EnemyKick", "EnemyCrouchPunch", "EnemyCrouchKick"}*/;
    [SerializeField]int actionCount = 0;
    int reactionCount = 0;
    public int punchForce = 6;
    public int kickForce = 8;
    float speed = 1f;
    float jumpForce = 10f;
    public string attackAnimationName = "";
    float attackDuration = 0f;
    [SerializeField]bool isMoving = false;
    [SerializeField]bool isFleeing = false;
    [SerializeField]bool isAttacking = false;
    [SerializeField]bool isBlocking = false;
    [SerializeField]bool isHit = false;
    public bool isKnockedOut = false;
    [SerializeField]bool performCombos = false;
    int enemyCombos = 0;
    public int health = 100;
    float attackTimer = 0f;
    float blockTimer = 0f;
    float hitTimer = 0f;
    int enemyDifficultyLevel = 0;
    GameManager enemyGameManager;
    [SerializeField]CombatManager enemyCombatManager;
    int enemyLayer = 1 << 10;
    int enemyLayerMask;
    string viewToPlayer = "";
    Vector3 fleeDirection;
    [SerializeField]PlayerInfo enemyInfo;
    

    void Start() {
        enemyAnimator = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<Animator>();
        enemyRenderer = gameObject.transform.GetChild(0).gameObject.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        enemyGameManager = GameManager.instance;
        enemyCombatManager = gameObject.GetComponent<CombatManager>();
        enemyRaycastObject = this.gameObject;
        target = GameObject.Find("Player");

        enemyLayerMask = ~enemyLayer;

        EnemyMind();
    }

    void Update() {
        CheckDistance();
        EnemyMove();
        EnemyAttack();
        EnemyStatus();
    }

    void FixedUpdate() {
        CheckPlayer();
    }

    void EnemyMind() {
        StartCoroutine(EnemyDecision());
    }

    void CheckDistance() {
        if(isKnockedOut == false) {
            targetDistance = Vector3.Distance(target.transform.position, gameObject.transform.position);
            // print("tagetDistance: " + targetDistance);
        }
    }

    void CheckPlayer() {
        if(isKnockedOut == false) {
            float raycastDistance = 10f;
            Ray frontRay = new Ray(enemyRaycastObject.transform.position, transform.right);
            Ray backRay = new Ray(enemyRaycastObject.transform.position, -transform.right);

            RaycastHit2D frontRaycastHit = Physics2D.Raycast(enemyRaycastObject.transform.position, Vector2.right, raycastDistance, enemyLayerMask);
            RaycastHit2D backRaycastHit = Physics2D.Raycast(enemyRaycastObject.transform.position, Vector2.left, raycastDistance, enemyLayerMask);
            
            // Debug.DrawRay(frontRay.origin, frontRay.direction * raycastDistance);
            // Debug.DrawRay(backRay.origin, backRay.direction * raycastDistance);

            if(frontRaycastHit.collider != null) {
                // Debug.Log("Player ahead");
                // Debug.Log("hitObject: " + frontRaycastHit.collider.gameObject.name);
                if(frontRaycastHit.collider.gameObject.CompareTag("Player")) {
                    viewToPlayer = "front";
                    // enemyRenderer.flipX = false;
                    gameObject.transform.rotation = Quaternion.Euler(Vector3.zero);
                    if(isFleeing) {
                        fleeDirection = gameObject.transform.position + Vector3.left;
                    }
                }
            } else if(backRaycastHit.collider != null) {
                // Debug.Log("Player behind");
                if(backRaycastHit.collider.gameObject.CompareTag("Player")) {
                    viewToPlayer = "back";
                    // enemyRenderer.flipX = true;
                    gameObject.transform.rotation = Quaternion.Euler(0, 180, 0);
                    if(isFleeing) {
                        fleeDirection = gameObject.transform.position + Vector3.right;
                    }
                }
            }
        }
    }

    void EnemyMove() {
        if(isKnockedOut == false) {
            if(isMoving == true && targetDistance > 0.25f && isFleeing == false && isBlocking == false && isHit == false) {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, target.transform.position, speed * Time.deltaTime);
                Set_Move_Animation(1);
            } else if(isMoving == false && isFleeing == false && isHit == false) {
                Set_Move_Animation(0);
            }

            if(isFleeing == true && isBlocking == false) {
                gameObject.transform.position = Vector3.MoveTowards(gameObject.transform.position, fleeDirection, speed * Time.deltaTime);
                Set_Move_Animation(1);
            }

            if(enemyActionList.Exists(action => action.EndsWith("Jump"))) {
                // Debug.Log("enemyJump");
                Play_Jump_Animation();
                gameObject.transform.position = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y + jumpForce * Time.deltaTime);
                enemyActionList.Remove("Jump");
            }

            if(isBlocking) {
                AnimationClip currentAnimationClip = enemyAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
                string currentAnimationName = currentAnimationClip.name;
                if(currentAnimationName == "Move" ||
                currentAnimationName == "ChozenMove" ||
                currentAnimationName == "EliHawkMove" ||
                currentAnimationName == "PunkMove" ||
                currentAnimationName == "CyborgMove") {
                    // Debug.Log("Blocking while moving");
                    Set_Move_Animation(0);
                }
            }
        }
        
        if(gameObject.transform.position.x > 1.6f) {
            gameObject.transform.position = new Vector2(1.6f, gameObject.transform.position.y);
        } else if(gameObject.transform.position.x < -1.6f) {
            gameObject.transform.position = new Vector2(-1.6f, gameObject.transform.position.y);
        }
    }

    void EnemyAttack() {
        if(isKnockedOut == false) {
            if(enemyActionList.Count > 0 && enemyActionList.Exists(action => action.EndsWith("Block")) == false) {
                performCombos = true;
            }

            if(isAttacking == true && isBlocking == false && isHit == false) {
                attackDuration = enemyCombos * 0.1f;
                AnimationClip currentAnimationClip = enemyAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
                string currentAnimationName = currentAnimationClip.name;
                if(currentAnimationName == "Default" || currentAnimationName == "Move" ||
                currentAnimationName == "ChozenDefault" || currentAnimationName == "ChozenMove" ||
                currentAnimationName == "EliHawkDefault" || currentAnimationName == "EliHawkMove" ||
                currentAnimationName == "PunkDefault" || currentAnimationName == "PunkMove" ||
                currentAnimationName == "CyborgDefault" || currentAnimationName == "CyborgMove") {
                    // Debug.Log("EnemyAttack");
                    // Debug.Log("noAttackAnimationClipName: " + currentAnimationName);
                    // Debug.Log("Playing Animation Number: " + actionCount);
                    Set_Attack_Animation(enemyActionList[actionCount]);
                    if(actionCount < enemyActionList.Count && enemyActionList.Exists(action => action.EndsWith("Block")) == false) {
                        actionCount = actionCount + 1;
                        if(actionCount == enemyActionList.Count) {
                            // Debug.Log("ResetEnemyAttack");
                            actionCount = 0;
                            enemyActionList.Clear();
                            performCombos = false;
                        }
                    }
                }

                if(currentAnimationName == "HeadPunch" || currentAnimationName == "MiddlePunch" || currentAnimationName == "LowPunch" || currentAnimationName == "HeadKick" || currentAnimationName == "FrontKick" || currentAnimationName == "LowKick" ||
                currentAnimationName == "ChozenPunch" || currentAnimationName == "ChozenKick" || currentAnimationName == "ChozenCrouchPunch" || currentAnimationName == "ChozenCrouchKick" ||
                currentAnimationName == "EliHawkPunch" || currentAnimationName == "EliHawkKick" || currentAnimationName == "EliHawkCrouchPunch" || currentAnimationName == "EliHawkCrouchKick" ||
                currentAnimationName == "PunkDoublePunch" || currentAnimationName == "PunkKick" ||
                currentAnimationName == "CyborgDoublePunch" || currentAnimationName == "CyborgKick") {
                        attackTimer = attackTimer + Time.deltaTime;
                        attackAnimationName = currentAnimationName;
                        // Debug.Log("attackAnimationName: " + attackAnimationName);
                        if(attackTimer > currentAnimationClip.length) {
                            attackTimer = 0;
                            attackAnimationName = "";
                        }
                }
            } 
            
            if(isBlocking && isHit == false) {
                AnimationClip currentAnimationClip = enemyAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
                string currentAnimationName = currentAnimationClip.name;
                if(currentAnimationName == "Default" || /* currentAnimationName == "Move"*/
                currentAnimationName == "ChozenDefault" ||
                currentAnimationName == "EliHawkDefault" ||
                currentAnimationName == "PunkDefault" ||
                currentAnimationName == "CyborgDefault") {
                    // Debug.Log("EnemyBlock");
                    Set_Block_Animation(1);
                }

                if(currentAnimationName == "BodyBlock" ||
                currentAnimationName == "ChozenBlock" ||
                currentAnimationName == "EliHawkBlock" ||
                currentAnimationName == "PunkBlock" ||
                currentAnimationName == "CyborgBlock") {
                    blockTimer = blockTimer + Time.deltaTime;
                    if(blockTimer > currentAnimationClip.length) {
                        blockTimer = 0;
                        Set_Block_Animation(0);
                        enemyActionList.Remove("Block");
                        if(!enemyActionList.Exists(action => action.EndsWith("Block"))) {
                            isBlocking = false;
                        }
                    }
                }
            }
            // Debug.Log("actionCount: " + actionCount + ", enemyActionList.Count: " + enemyActionList.Count);
        }
    }

    void EnemyStatus() {
        if(isKnockedOut == false) {
            AnimationClip currentAnimationClip = enemyAnimator.GetCurrentAnimatorClipInfo(0)[0].clip;
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
                    // Debug.Log("Hit Animation Playing");
                    hitTimer = hitTimer + Time.deltaTime;
                    if(hitTimer > currentAnimationClip.length) {
                        hitTimer = 0;
                        isHit = false;
                    }
                    
                }
            }
        }
    }

    public void KnockOutEnemy() {
        isKnockedOut = true;
        Set_Default_Animation();
        Play_Fall_Animation();
    }

    // Values change
    public void SetEnemyInfo(PlayerInfo _enemyInfo) {
        enemyInfo = _enemyInfo;
    }

    public void AddToActionList(string _action) {
        // if(targetActionList.Exists(targetAction => targetAction.EndsWith(_action)) == false) {
            // Debug.Log("AddToActionList");
            targetActionList.Add(_action);
        // }
    }

    // Components change
    public void SetAnimator(Animator _animator) {
        enemyAnimator = _animator;
    }

    public void SetSpriteRenderer(SpriteRenderer _spriteRenderer) {
        enemyRenderer = _spriteRenderer;
    }

    // Animations
    void Set_Move_Animation(int _moveFactor) {
        // enemyAnimator.SetInteger("WalkFactor", _moveFactor);
        if(_moveFactor == 1) {
            enemyAnimator.SetBool("Move", true);
        } else if (_moveFactor == 0) {
            enemyAnimator.SetBool("Move", false);
        }
    }

    void Set_Attack_Animation(string attackAnimation) {
        enemyAnimator.Play(attackAnimation);
    }

    void Set_Default_Animation() {
        enemyAnimator.Play("Default");
    }

    void Play_Jump_Animation() {
        enemyAnimator.Play("Jump");
    }

    void Set_Block_Animation(int _blockFactor) {
        if(_blockFactor == 1) {
            enemyAnimator.SetBool("BodyBlock", true);
        } else if(_blockFactor == 0) {
            enemyAnimator.SetBool("BodyBlock", false);
        }
    }

    void Play_Hit_Animation() {
        enemyAnimator.Play("Hit");
    }

    void Play_Fall_Animation() {
        enemyAnimator.Play("Fall");
    }

    public void Resume_All_Animations() {
        enemyAnimator.speed = 1;
    }

    public void Stop_All_Animations() {
        enemyAnimator.speed = 0;
    }

    // Collisions
    void OnCollisionEnter2D(Collision2D objectCollision) {
        if(objectCollision.collider.gameObject.CompareTag("AttackCollider")) {
            if(isBlocking == false && isKnockedOut == false) {
                // Debug.Log("EnemyHit");
                isHit = true;
                Set_Default_Animation();
                Player targetFighter = target.GetComponent<Player>();
                // Debug.Log("targetFighter.attackAnimationName: " + targetFighter.attackAnimationName);
                int incomingDamage = targetFighter.attackAnimationName.Contains("Punch") ? targetFighter.punchForce : targetFighter.kickForce;
                // Debug.Log("incomingDamage: " + incomingDamage);
                enemyCombatManager.TakeDamage(this, incomingDamage);
            }
        }
    }

    IEnumerator EnemyDecision() {
        while(isKnockedOut == false) {
            yield return new WaitForSeconds(0.1f);

            if(targetActionList.Count == 0) {
                int moveFactor = Random.Range(1, 101);
                
                // Debug.Log("targetDistance: " + targetDistance);
                if(targetDistance < 0.25f) {
                    // Debug.Log("near target");
                    isMoving = false;

                    // Check actionList and generate number of attacks.
                    int attackFactor = Random.Range(1, 100);
                    // attackFactor = 51;
                    if(attackFactor > 50 && isBlocking == false && isHit == false) {
                        isAttacking = true;
                        if(performCombos == false) {
                            enemyCombos = Random.Range(1, 5);
                            for(int i=0; i<enemyCombos; i=i+1){
                                // Debug.Log("enemyCombos: " + enemyCombos + ", attackFactor: " + attackFactor);
                                int attackCombo = Random.Range(0, 4);
                                // Debug.Log("attackCombo: " + attackCombo);
                                // Debug.Log("attackAnimation: " + enemyInfo.selectedCharacter.attackAnimationNames[attackCombo]);
                                enemyActionList.Add(enemyInfo.selectedCharacter.attackAnimationNames[attackCombo]);
                                // enemyActionList.Add("BodyPunch");
                            }
                        }
                    } else if(attackFactor <= 50 || isBlocking == true || isHit == true) {
                        isAttacking = false;
                    }
                } else {
                    isAttacking = false;
                    if(moveFactor > 50 && isBlocking == false && isHit == false) {
                        // Debug.Log("move to target");
                        isMoving = true;
                    } else if(moveFactor <= 50 || isBlocking == true || isHit == true) {
                        // Debug.Log("stop");
                        isMoving = false;
                    }
                }
                // Debug.Log("isAttacking: " + isAttacking);

                int fleeFactor = Random.Range(1, 51);
                // Debug.Log("isFleeing: " + isFleeing);
                if(fleeFactor > 40 && isBlocking == false) {
                    isFleeing = true;
                } else if(fleeFactor <= 40 || isBlocking == true) {
                    isFleeing = false;
                }
            } else {
                // Debug.Log("React to Target's actions");
                for(int i=0; i<targetActionList.Count; i=i+1) {
                    switch(targetActionList[i]) {
                        case "Jump":
                            // Debug.Log("Player is jumping");
                            int jumpFactor = Random.Range(0, 2);
                            if(jumpFactor == 1) {
                                enemyActionList.Add("Jump");
                            }
                            // reactionCount = reactionCount + 1;
                            targetActionList.Remove("Jump");
                            break;

                        case "Attack":
                            // Debug.Log("Player is attacking");
                            int blockFactor = Random.Range(1, 51);
                            // blockFactor = 31;
                            if(blockFactor > 30) {
                                // Debug.Log("enemyActionList Add Block");
                                isBlocking = true;
                                enemyActionList.Add("Block");
                            }
                            // reactionCount = reactionCount + 1;
                            targetActionList.Remove("Attack");
                            break;

                        default:
                            break;
                    }
                }

                /*if(targetActionList.Exists(targetAction => targetAction.EndsWith("Jump"))) {
                    int jumpFactor = Random.Range(0, 2);
                    if(jumpFactor == 1) {
                        enemyActionList.Add("Jump");
                    }
                    targetActionList.Remove("Jump");
                }
                if(targetActionList.Exists(targetAction => targetAction.EndsWith("Attack"))) {
                    targetActionList.Remove("Attack");
                }*/

                /*if(reactionCount != targetActionList.Count) {
                } else if(reactionCount >= targetActionList.Count) {
                    yield return new WaitForSeconds(0.1f);
                    reactionCount = 0;
                    targetActionList.Clear();
                }*/
            }
        }
    }
}