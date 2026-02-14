using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class UI_MazeGame : MonoBehaviour
{

    [Header("UI Prefabs")]
    [SerializeField] GameObject wallPrefab;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject keyPrefab;
    [SerializeField] GameObject finishPrefab;
    [SerializeField] Sprite finishFlagIcon;

    [Header("Scene Plug ins")]
    [SerializeField] Transform gridParent;
    [SerializeField] Camera gameCam;
    [SerializeField] BasicInteract interactionControl;
    [Space(10)]
    [SerializeField] TMP_Text endGametmp;
    [SerializeField] TMP_Text timertmp;

    [Space(10)]
    public UnityEvent onGameWon;

    [Space(15)]
    [Header("Maze Tuning")]
    [SerializeField] Vector2Int grid = new Vector2Int(13, 9);
    [SerializeField, Range(0f, 0.5f)] float loopChance = 0.25f;
    [SerializeField, Range(0f, 1f)] float deadEndReconnectChance = 0.35f;
    [SerializeField] int keysSpawned = 1;

    blockTypes[,] map;

    [Space(15)]
    [Header("Gameplay Settings")]
    [SerializeField] float timeToBeat = 60f;
    [SerializeField] Gradient timeToColor;
    [Space(10)]
    [SerializeField] float timeBetweenMoves = 1f;
    [SerializeField] float delayTime = .25f;


    GameObject playerModel;
    Image finishModel;

    Vector2Int playerPos;
    bool moveCD;

    int totalKeysCollected = 0;
    float currentTime = 0;

    [Header("SFX Settings")]
    //[SerializeField] AudioSource musicSource;
    [Space(10)]
    [SerializeField] string moveSFX;
    [SerializeField] string keyGrabSFX;
    [SerializeField] string mazeCompleteSFX;
    [SerializeField] string mazeFailSFX;
    [SerializeField] string gateUnlockedSFX;

    [Header("Debug")]
    [SerializeField] bool gameIsActive;
    
    CanvasGroup canvGroup;

    private void Awake()
    {
        canvGroup = GetComponent<CanvasGroup>();
        gameCam.gameObject.SetActive(false);

    }

    void Start()
    {
        InitializeMapCreation();
    }

    private void Update()
    {
        if (!gameIsActive) return;
        PlayerMovement();
        TimerSystem();
    }

    private void OnDrawGizmosSelected()
    {
        int count = onGameWon.GetPersistentEventCount();

        if (count > 0)
        {

            Gizmos.color = Color.green;

            for (int i = 0; i < count; i++)
            {
                Object target = onGameWon.GetPersistentTarget(i);


                if (target is Component comp)
                {
                    Gizmos.DrawLine(transform.position, comp.transform.position);
                }
                else if (target is GameObject go)
                {
                    Gizmos.DrawLine(transform.position, go.transform.position);
                }
            }
        }
    }

    #region #### Map Creation ####
    public enum blockTypes
    {
        Empty,
        Wall,
        Player,
        Finish,
        Key
    }

    [ContextMenu("Spawn Maze")]
    void InitializeMapCreation()
    {

        clearMapGrid();

        currentTime = timeToBeat;
        timertmp.text = timeToBeat.ToString();
        timertmp.color = timeToColor.Evaluate(0);

        playerPos = Vector2Int.one;
        totalKeysCollected = 0;

        GenerateMaze();
        map[1, 1] = blockTypes.Player;
        BuildUIMap();
    }


    [ContextMenu("Clear Map")]
    void clearMapGrid()
    {
        if (gridParent == null) return;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            while (gridParent.childCount > 0)
            {
                DestroyImmediate(gridParent.GetChild(0).gameObject);
            }
        }
#endif

        // Play mode
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject);
        }
    }

    // ==========================
    // MAZE GENERATION
    // ==========================
    void GenerateMaze()
    {
        map = new blockTypes[grid.x, grid.y];

        // Start fully walled
        for (int x = 0; x < grid.x; x++)
        {
            for (int y = 0; y < grid.y; y++)
            {
                map[x, y] = blockTypes.Wall;
            }
        }

        Vector2Int start = new Vector2Int(RandomOdd(grid.x), RandomOdd(grid.y));
        Carve(start.x, start.y);

        // Player start is always fixed
        map[1, 1] = blockTypes.Player;

        Vector2Int finishPos = SelectDeadEndForFinish();
        map[finishPos.x, finishPos.y] = blockTypes.Finish;

        // Now mutate the maze
        CreateLoops();
        ReconnectDeadEnds();
        SpawnKeys();
    }


    void Carve(int x, int y)
    {
        map[x, y] = blockTypes.Empty;

        List<Vector2Int> directions = new List<Vector2Int>
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right
        };

        Shuffle(directions);

        foreach (var dir in directions)
        {
            int nx = x + dir.x * 2;
            int ny = y + dir.y * 2;

            if (InBounds(nx, ny) && map[nx, ny] == blockTypes.Wall)
            {
                map[x + dir.x, y + dir.y] = blockTypes.Empty;
                Carve(nx, ny);
            }
        }
    }

    void CreateLoops()
    {
        for (int x = 2; x < grid.x - 2; x++)
        {
            for (int y = 2; y < grid.y - 2; y++)
            {
                if (map[x, y] != blockTypes.Wall) continue;
                if (IsProtectedWall(x, y)) continue;
                if (Random.value > loopChance) continue;

                int connections = 0;

                if (map[x + 1, y] == blockTypes.Empty) connections++;
                if (map[x - 1, y] == blockTypes.Empty) connections++;
                if (map[x, y + 1] == blockTypes.Empty) connections++;
                if (map[x, y - 1] == blockTypes.Empty) connections++;

                if (map[x + 1, y + 1] == blockTypes.Empty) connections++;
                if (map[x - 1, y - 1] == blockTypes.Empty) connections++;
                if (map[x - 1, y + 1] == blockTypes.Empty) connections++;
                if (map[x + 1, y - 1] == blockTypes.Empty) connections++;


                // EXACTLY 2 connections prevents corner erosion
                if (connections <= 3)
                {
                    map[x, y] = blockTypes.Empty;
                }
            }
        }
    }

    void ReconnectDeadEnds()
    {
        for (int x = 2; x < grid.x - 2; x++)
        {
            for (int y = 2; y < grid.y - 2; y++)
            {
                if (map[x, y] != blockTypes.Empty) continue;

                List<Vector2Int> neighbors = GetEmptyNeighbors(x, y);

                if (neighbors.Count == 1 && Random.value < deadEndReconnectChance)
                {
                    List<Vector2Int> walls = GetWallNeighbors(x, y);

                    // Remove any protected walls
                    walls.RemoveAll(w => IsProtectedWall(w.x, w.y));

                    if (walls.Count > 0)
                    {
                        Vector2Int chosen = walls[Random.Range(0, walls.Count)];
                        map[chosen.x, chosen.y] = blockTypes.Empty;
                    }
                }
            }
        }
    }

    bool IsProtectedWall(int x, int y)
    {
        return x <= 1 ||
               y <= 1 ||
               x >= grid.x - 1 - 1 ||
               y >= grid.y - 1 - 1;
    }

    List<Vector2Int> GetEmptyNeighbors(int x, int y)
    {
        List<Vector2Int> list = new();
        TryAdd(x + 1, y, blockTypes.Empty, list);
        TryAdd(x - 1, y, blockTypes.Empty, list);
        TryAdd(x, y + 1, blockTypes.Empty, list);
        TryAdd(x, y - 1, blockTypes.Empty, list);
        return list;
    }

    List<Vector2Int> GetWallNeighbors(int x, int y)
    {
        List<Vector2Int> list = new();
        TryAdd(x + 1, y, blockTypes.Wall, list);
        TryAdd(x - 1, y, blockTypes.Wall, list);
        TryAdd(x, y + 1, blockTypes.Wall, list);
        TryAdd(x, y - 1, blockTypes.Wall, list);
        return list;
    }

    bool IsDeadEnd(int x, int y)
    {
        if (map[x, y] != blockTypes.Empty)
            return false;

        int connections = 0;

        if (map[x + 1, y] == blockTypes.Empty) connections++;
        if (map[x - 1, y] == blockTypes.Empty) connections++;
        if (map[x, y + 1] == blockTypes.Empty) connections++;
        if (map[x, y - 1] == blockTypes.Empty) connections++;

        return connections == 1;
    }

    Vector2Int SelectDeadEndForFinish()
    {
        List<Vector2Int> deadEnds = new();

        for (int x = 2; x < grid.x - 2; x++)
        {
            for (int y = 2; y < grid.y - 2; y++)
            {
                if (!IsDeadEnd(x, y)) continue;

                // Must not be adjacent to player
                if (Mathf.Abs(x - 1) + Mathf.Abs(y - 1) <= 1)
                    continue;

                deadEnds.Add(new Vector2Int(x, y));
            }
        }

        if (deadEnds.Count == 0)
            return new Vector2Int(grid.x - 3, grid.y - 3);

        return deadEnds[Random.Range(0, deadEnds.Count)];
    }



    void TryAdd(int x, int y, blockTypes type, List<Vector2Int> list)
    {
        if (InBounds(x, y) && map[x, y] == type)
            list.Add(new Vector2Int(x, y));
    }

    void SpawnKeys()
    {
        List<Vector2Int> validSpots = new();

        for (int x = 1; x < grid.x - 1; x++)
        {
            for (int y = 1; y < grid.y - 1; y++)
            {
                if (map[x, y] == blockTypes.Empty &&
                    !(x == 1 && y == 1)) // avoid player start
                {
                    validSpots.Add(new Vector2Int(x, y));
                }
            }
        }

        Shuffle(validSpots);

        int placed = 0;
        foreach (var pos in validSpots)
        {
            map[pos.x, pos.y] = blockTypes.Key;
            placed++;

            if (placed >= keysSpawned)
                break;
        }
    }

    // ==========================
    // UI BUILD
    // ==========================
    void BuildUIMap()
    {
        for (int y = grid.y - 1; y >= 0; y--) // top → bottom
        {
            for (int x = 0; x < grid.x; x++) // left → right
            {
                SpawnTile(map[x, y],x,y);
            }
        }
    }
    // ==========================
    // HELPERS
    // ==========================
    bool InBounds(int x, int y)
    {
        return x > 0 && y > 0 && x < grid.x - 1 && y < grid.y - 1;
    }

    int RandomOdd(int max)
    {
        int value = Random.Range(1, max - 1);
        return value % 2 == 0 ? value + 1 : value;
    }

    void Shuffle(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
    void SpawnTile(blockTypes type, int x, int y)
    {

        switch (type)
        {
            case blockTypes.Wall:
                Instantiate(wallPrefab,gridParent).name = $"Wall [{x}, {y}]";
                break;
            case blockTypes.Player:
                playerModel = Instantiate(playerPrefab, gridParent);
                break;
            case blockTypes.Finish:
                finishModel = Instantiate(finishPrefab, gridParent).GetComponent<Image>();
                finishModel.gameObject.name = $"Finish [{x}, {y}]";
                break;
            case blockTypes.Key:
                Instantiate(keyPrefab, gridParent).name = $"Key [{x}, {y}]";
                break;
            case blockTypes.Empty:
                GameObject temp = new GameObject($"Empty [{x}, {y}]", typeof(RectTransform));
                temp.transform.SetParent(gridParent,false);
                break;
        }
    }

    #endregion


    #region #### Gameplay Section ####

    public void triggerMiniGame()
    {
        gameCam.gameObject.SetActive(true);
        PlayerController.instance.SetPlayerLogicState(false);
        gameIsActive = true;
        //musicSource.Play();
    }


    void PlayerMovement()
    {        
        if (moveCD) return;
        
        Vector2Int dir = Vector2Int.zero;

        if (InputManager.Move.x != 0)
        {
            dir = InputManager.Move.x > 0 ? Vector2Int.right : Vector2Int.left;
        }
        else if (InputManager.Move.y != 0)
        {
            dir = InputManager.Move.y > 0 ? Vector2Int.up : Vector2Int.down;
        }
        else
            return;

        Vector2Int target = playerPos + dir;

        if (InBounds(target.x, target.y) &&
            map[target.x, target.y] != blockTypes.Wall)
        {

            if (totalKeysCollected != keysSpawned && map[target.x, target.y] == blockTypes.Finish)
            {
                return;
            }

            SoundManager.instance?.playSound(moveSFX, transform,1,true);

            StartCoroutine(moveCooldown(target));
        }
    }

    IEnumerator gameEndSequence(string gameEndText, bool gameWon = false)
    {
        canvGroup.alpha = 0;
        gameIsActive = false;
        endGametmp.gameObject.SetActive(true);

        endGametmp.text = gameEndText;

        yield return new WaitForSeconds(1);

        PlayerController.instance.SetPlayerLogicState(true);
        gameCam.gameObject.SetActive(false);
        interactionControl.enabled = false;

        if (gameWon)
        {
            SoundManager.instance?.playSound(mazeCompleteSFX,transform);
            onGameWon?.Invoke();
        }
        else
        {
            SoundManager.instance?.playSound(mazeFailSFX, transform);
        }
        
        yield return new WaitForSeconds(2f);
        
        if (!gameWon)
        {
            InitializeMapCreation();
            canvGroup.alpha = 1;
            interactionControl.enabled = true;

        }

        //musicSource.Pause();


        endGametmp.gameObject.SetActive(false);
        StopAllCoroutines();
    }

    // Add SFX in Here
    IEnumerator moveCooldown(Vector2Int targetPos)
    {
        moveCD = true;

        yield return new WaitForSeconds(delayTime);

        int targetIndex = GridToUIIndex(targetPos);

        if (gridParent.GetChild(targetIndex).CompareTag("Finish"))                              //  <---     Game has finished
        {
            StartCoroutine(gameEndSequence("Fire Wall Cracked", true));
        }

        if (gridParent.GetChild(targetIndex).CompareTag("Key"))
        {
            gridParent.GetChild(targetIndex).GetComponent<Image>().enabled = false;
            gridParent.GetChild(targetIndex).tag = "Untagged";
            totalKeysCollected++;


            if (totalKeysCollected == keysSpawned)
            {
                SoundManager.instance?.playSound(gateUnlockedSFX, transform);
                finishModel.sprite = finishFlagIcon;
            }
            else
            {
                SoundManager.instance?.playSound(keyGrabSFX, transform);
            }

        }


        gridParent.GetChild(targetIndex).SetSiblingIndex(playerModel.transform.GetSiblingIndex());
        playerModel.transform.SetSiblingIndex(targetIndex);

        // Update logical position AFTER move
        playerPos = targetPos;

        yield return new WaitForSeconds(timeBetweenMoves);

        moveCD = false;
    }


    int lastSecond = 0;
    int currentSec = 0;

    void TimerSystem()
    {
        if (currentTime > 0f)
        {
            currentTime -= Time.deltaTime;

            // Clamp so it never goes below 0
            currentTime = Mathf.Clamp(currentTime, 0f, timeToBeat);

            timertmp.color = timeToColor.Evaluate(1 - (currentTime / timeToBeat));

            if(currentTime >= 10f) timertmp.text = currentTime.ToString("F0");
            else
            {
                timertmp.text = currentTime.ToString("F2");
                
                currentSec = Mathf.CeilToInt(currentTime);
                
                if(currentSec != lastSecond)
                {
                    SoundManager.instance?.playSound("Maze.Timer",transform, 1f - (currentSec / 10f));
                    lastSecond = currentSec;
                }
            }
        }

        if(currentTime == 0f)
        {
            StartCoroutine(gameEndSequence("Hack Failed"));
        }
    }

    int GridToUIIndex(Vector2Int pos)
    {
        int rowFromTop = (grid.y - 1) - pos.y;
        return rowFromTop * grid.x + pos.x;
    }

    #endregion
}
