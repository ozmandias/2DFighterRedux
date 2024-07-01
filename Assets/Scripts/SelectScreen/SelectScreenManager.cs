using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectScreenManager : MonoBehaviour {
    [SerializeField] PlayerInterface player1Interface;
    [SerializeField] PlayerInterface player2Interface;
    public List<PlayerInterface> playerInterfaceList = new List<PlayerInterface>();
    bool player1Selected = false;
    bool player2Selected = false;
    [SerializeField] bool bothPlayersSelected = false;
    public GameObject portraitCanvas;
    public PortraitInfo[] portraits;
    PortraitInfo[,] characterGrid;
    public int gridX;
    public int gridY;
    public int numberOfPlayers = 0;
    public GameObject selectorObject;
    public GameObject portraitObject;
    public int maxPlayers = 2;
    bool characterSelect = false;

    GameManager selectGameManager;
    CharacterManager selectCharacterManager;

    void Start() {
        characterSelect = true;

        selectGameManager = GameManager.instance;
        selectCharacterManager = CharacterManager.instance;

        // create Grid.
        characterGrid = new PortraitInfo[gridX, gridY];
        portraits = portraitCanvas.GetComponentsInChildren<PortraitInfo>();
        int portraitX = 0;
        int portraitY = 0;
        // set PortraitInfo objects' positions in grid.
        for(int i = 0; i < portraits.Length; i = i + 1) {
            portraits[i].posX += portraitX;
            portraits[i].posY += portraitY;

            characterGrid[portraitX, portraitY] = portraits[i];

            if(portraitX < gridX - 1) {
                portraitX = portraitX + 1;
            } else {
                portraitX = 0;
                portraitY = portraitY + 1;
            }
        }

        // create PlayerInterfaces.
        numberOfPlayers = selectGameManager.totalPlayers;
        for(int i = 0; i <  maxPlayers; i = i + 1){
            PlayerInfo currentPlayerInfo = selectCharacterManager.GetPlayer(i);
            if(currentPlayerInfo != null) {
                PlayerInterface currentPlayerInterface = playerInterfaceList[i];
                currentPlayerInterface.playerInfo = currentPlayerInfo;

                currentPlayerInterface.activeX = 0;
                currentPlayerInterface.activeY = 0;
                currentPlayerInterface.previewPortrait = characterGrid[currentPlayerInterface.activeX, currentPlayerInterface.activeY];
                DisplayCharacter(currentPlayerInterface.previewPortrait, currentPlayerInterface.characterVisualPosition, currentPlayerInterface.playerInfo.playerName);

                StartCoroutine(FlickerSelectIndicator(currentPlayerInterface/*.selector*/));
            }
        }
    }

    void Update() {
        // for multiple players.
        for(int i = 0; i < playerInterfaceList.Count; i = i + 1) {
            CharacterSelectMove(playerInterfaceList[i]);
            SelectCharacter(playerInterfaceList[i]);
        }
        ChangeScene();
    }

    void CharacterSelectMove(PlayerInterface _playerInterface) {
        int originalActiveX = _playerInterface.activeX;
        // Debug.Log("originalActiveX: " + originalActiveX);
        int originalActiveY = _playerInterface.activeY;
        // Debug.Log("originalActiveY: " + originalActiveY);

        if(_playerInterface.playerInfo.playerType == PlayerType.User) {
            if(_playerInterface.activePortrait == null) {
                float horizontal = Input.GetAxis("Horizontal" + _playerInterface.playerInfo.playerInputID);
                float vertical = Input.GetAxis("Vertical" + _playerInterface.playerInfo.playerInputID);

                // D and A
                // Debug.Log("horizontal: " + horizontal);
                if(horizontal != 0) {
                    if(_playerInterface.selectorMovingHorizontal == false) {
                        if(horizontal > 0) {
                            Debug.Log("D Key");
                            _playerInterface.activeX = _playerInterface.activeX + 1;
                            if(_playerInterface.activeX > gridX - 1) {
                                _playerInterface.activeX = 0;
                            }
                        } else if(horizontal < 0) {
                            Debug.Log("A Key");
                            _playerInterface.activeX = _playerInterface.activeX - 1;
                            if(_playerInterface.activeX < 0) {
                                _playerInterface.activeX = gridX - 1;
                            }
                        }
                        _playerInterface.selectorMovingHorizontal = true;
                    }
                } else {
                    _playerInterface.selectorMovingHorizontal = false;
                }

                // W and S
                // Debug.Log("vertical: " + vertical);
                if(vertical != 0) {
                    if(_playerInterface.selectorMovingVertical == false) {
                        if(vertical > 0) {
                            Debug.Log("W Key");
                            _playerInterface.activeY = _playerInterface.activeY - 1;
                            if(_playerInterface.activeY < 0) {
                                _playerInterface.activeY = gridY - 1;
                            }
                        } else if(vertical < 0) {
                            Debug.Log("S Key");
                            _playerInterface.activeY = _playerInterface.activeY + 1;
                            if(_playerInterface.activeY > gridY - 1) {
                                _playerInterface.activeY = 0;
                            }
                        }
                        _playerInterface.selectorMovingVertical = true;
                    }
                } else {
                    _playerInterface.selectorMovingVertical = false;
                }
            }
        } else {
            int sameActiveXYCount = 0;
            foreach(PlayerInterface compareInterface in playerInterfaceList) {
                if(_playerInterface.playerInfo.playerName != compareInterface.playerInfo.playerName && _playerInterface.activeX == compareInterface.activeX && _playerInterface.activeY == compareInterface.activeY) {
                    sameActiveXYCount++;
                }
            }

            if(sameActiveXYCount > 0) {
                if(_playerInterface.selectorMovingHorizontal == false && _playerInterface.selectorMovingVertical == false) {
                    _playerInterface.activeX = Random.Range(0, gridX);
                    // if(_playerInterface.activeX < 0) {
                    //     _playerInterface.activeX = gridX - 1;
                    // } else if(_playerInterface.activeX > gridX - 1) {
                    //     _playerInterface.activeX = 0;
                    // }

                    _playerInterface.activeY = Random.Range(0, gridY);
                    // if(_playerInterface.activeY < 0) {
                    //     _playerInterface.activeY = gridY - 1;
                    // } else if(_playerInterface.activeY > gridY - 1) {
                    //     _playerInterface.activeY = 0;
                    // }
                }
            } else if(sameActiveXYCount == 0 && player1Selected == true) {
                // Debug.Log("Select character for AI/Simulation");
            }
        }

        _playerInterface.previewPortrait = characterGrid[_playerInterface.activeX, _playerInterface.activeY];
        if(originalActiveX != _playerInterface.activeX || originalActiveY != _playerInterface.activeY) {
            Debug.Log("previewPortrait changed.");
            DisplayCharacter(_playerInterface.previewPortrait, _playerInterface.characterVisualPosition, _playerInterface.playerInfo.playerName);
        }

        _playerInterface.selector.transform.position = _playerInterface.previewPortrait.gameObject.transform.position;
    }

    void DisplayCharacter(PortraitInfo _characterPortrait, Transform _parentTransform, string _playerName) {
        if(_parentTransform.childCount > 0) {
            Destroy(_parentTransform.GetChild(0).gameObject);
        }

        // if(_parentTransform.childCount == 0) {
            GameObject characterObject = Instantiate(selectCharacterManager.GetCharacterWithID(_characterPortrait.characterId).characterObject);
            characterObject.transform.SetParent(_parentTransform);
            characterObject.transform.localPosition = Vector3.zero;
            if(_playerName != "Player 1") {
                SpriteRenderer characterRenderer = characterObject.GetComponent<SpriteRenderer>();
                characterRenderer.flipX = true;
            }
        // }
    }

    void SelectCharacter(PlayerInterface _playerInterface) {
        if(_playerInterface.playerInfo.playerType == PlayerType.User) {
            if(_playerInterface.playerInfo.playerName == "Player 1" && Input.GetKeyDown(KeyCode.LeftCommand)) {
                if(player1Selected == false) {
                    _playerInterface.activePortrait = _playerInterface.previewPortrait;
                    _playerInterface.playerInfo.selectedCharacter = selectCharacterManager.GetCharacterWithID(_playerInterface.activePortrait.characterId);
                    player1Selected = true;
                }
            } else if (_playerInterface.playerInfo.playerName != "Player 1" && Input.GetKeyDown(KeyCode.RightCommand)) {
                if(player2Selected == false) {
                    _playerInterface.activePortrait = _playerInterface.previewPortrait;
                    _playerInterface.playerInfo.selectedCharacter = selectCharacterManager.GetCharacterWithID(_playerInterface.activePortrait.characterId);
                    player2Selected = true;
                }
            }
        } else {
            if(player1Selected == true && player2Selected == false) {
                StartCoroutine(AISelectCoroutine(_playerInterface));
            }
        }

        if(player1Selected && player2Selected) {
            bothPlayersSelected = true;
        }
    }
    
    void ChangeScene() {
        if(Input.GetKeyDown(KeyCode.Return)) {
            if(bothPlayersSelected == true) {
                StartCoroutine(StartGameCoroutine());
            }
        } else if(Input.GetKeyDown(KeyCode.Escape)) {
            StartCoroutine(LeaveGameCoroutine());
        }
    }

    IEnumerator FlickerSelectIndicator(PlayerInterface _playerInterface) {
        while(characterSelect) {
            if(_playerInterface.selector && _playerInterface.activePortrait == null) {
                if(_playerInterface.playerInfo.playerName == "Player 1") {
                    _playerInterface.selector.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                    _playerInterface.selector.SetActive(false);
                    yield return new WaitForSeconds(0.2f);
                } else {
                    _playerInterface.selector.SetActive(false);
                    yield return new WaitForSeconds(0.2f);
                    _playerInterface.selector.SetActive(true);
                    yield return new WaitForSeconds(0.2f);
                }
            } else if(_playerInterface.activePortrait) {
                _playerInterface.selector.SetActive(true);
                yield return new WaitForSeconds(0.1f);
                break;
            } else if(_playerInterface.selector == null) {
                yield return new WaitForSeconds(0.1f);
                break;
            }
        }
    }

    IEnumerator StartGameCoroutine() {
        for(int i = 0; i < playerInterfaceList.Count; i = i + 1) {
            selectGameManager.playerDictionary.Add(playerInterfaceList[i].playerInfo, int.Parse(playerInterfaceList[i].activePortrait.characterId));
        }
        foreach(KeyValuePair<PlayerInfo, int> playerKeyValuePair in selectGameManager.playerDictionary) {
            Debug.Log("PlayerInfo: " + playerKeyValuePair.Key.playerName + ", characterID: " + playerKeyValuePair.Value);
        }
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("level");
    }

    IEnumerator AISelectCoroutine(PlayerInterface _playerInterface) {
        yield return new WaitForSeconds(1f);
        _playerInterface.activePortrait = _playerInterface.previewPortrait;
        _playerInterface.playerInfo.selectedCharacter = selectCharacterManager.GetCharacterWithID(_playerInterface.activePortrait.characterId);
        player2Selected = true;
    }

    IEnumerator LeaveGameCoroutine() {
        selectGameManager.gameMode = GameMode.None;
        selectGameManager.totalPlayers = 0;
        selectGameManager.playerDictionary.Clear();
        if(selectCharacterManager != null) {
            selectCharacterManager.ClearData();
        }
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene("intro");
    }
}