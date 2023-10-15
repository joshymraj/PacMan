using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

public class GameController : MonoBehaviour
{
    public int rows = 22;
    
    public int columns = 30;

    public int maxEnemies = 5;

    public float enemySpawnInterval = 3;

    public float powerUpSpawnInterval = 7;

    public float powerUpActiveTime = 2;

    public int totalPacmanLives = 3;

    public float timeToComplete = 120;

    [SerializeField]
    Tilemap tilemap;

    [SerializeField]
    Tile tile;

    [SerializeField]
    HUDController hudController;

    [SerializeField]
    DpadController dpad;

    [SerializeField]
    PlayerController playerController;

    [SerializeField]
    EnemyController enemyPrefab;

    [SerializeField]
    PowerUpController powerUpPrefab;
    
    Transform playerTransform;

    bool _isPacmanAlive;
    int _totalLivesLeft;

    float _totalTiles = 0;
    float _filledTiles = 0;

    int _startXPos;
    int _startYPos;

    List<Vector2Int> _currentPath;
    List<EnemyController> _currentEnemyPool;

    float _enemySpawnTimer;
    float _powerUpSpawnTimer;

    float _powerUpActiveTimer;

    float _currentEnemySpawnInterval;

    float _timeRemaining;

    bool _isPowerUpActive;

    bool _isPlayerDiedOnEnemyEncounter;

    PowerUpController _activePowerUp;

    void Start()
    {
        _currentEnemyPool = new List<EnemyController>();
        _currentPath = new List<Vector2Int>();        

        hudController.OnGameRestart = HandleGameRestart;
        hudController.OnPacmanRegenCounterDone = HandlePacmanRegenCounterDone;

        playerController.OnHitEnemy = HandleHitEnemy;
        playerController.OnPowerUpCollected = HandlePlayerPowerUpCollected;

        InitGame();
    }

    void Update()
    {
        if (_isPacmanAlive)
        {
            CheckInput();
           
            if (_enemySpawnTimer > _currentEnemySpawnInterval)
            {
                SpawnEnemy();
            }

            if (_powerUpSpawnTimer > powerUpSpawnInterval)
            {
                SpawnPowerUp();
            }

            if (_isPowerUpActive)
            {
                if(_powerUpActiveTimer > powerUpActiveTime)
                {
                    DisablePowerUp();
                }
                _powerUpActiveTimer += Time.deltaTime;
            }
            else
            {
                _enemySpawnTimer += Time.deltaTime;                
            }

            _powerUpSpawnTimer += Time.deltaTime;

            _timeRemaining -= Time.deltaTime;

            hudController.UpdateTime(_timeRemaining);

            if(_timeRemaining < 1)
            {
                TimeUp();
            }
        }        
    }

    void EnablePowerUp()
    {
        _isPowerUpActive = true;        

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            enemy.EnablePowerUp();
        }
    }

    void DisablePowerUp()
    {
        _isPowerUpActive = false;
        _powerUpActiveTimer = 0;

        foreach(EnemyController enemy in _currentEnemyPool)
        {
            enemy.DisablePowerUp();
        }
    }

    void InitGame()
    {       
        _enemySpawnTimer = 0;
        _powerUpSpawnTimer = 0;
        _filledTiles = 0;
        _currentEnemySpawnInterval = enemySpawnInterval;

        _currentPath.Clear();
        dpad.direction = DpadDirection.None;        

        GenerateLevel();

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            if (enemy != null)
            {
                Destroy(enemy.gameObject);
            }
        }

        _currentEnemyPool.Clear();

        if(_activePowerUp != null)
        {
            Destroy(_activePowerUp.gameObject);
        }

        _currentPath.Clear();

        playerController.gameObject.SetActive(true);
        playerController.Spawn(new Vector2(_startXPos - 0.5f, _startYPos - 0.5f));
        playerTransform = playerController.gameObject.transform;

        playerController.Init();
        hudController.Init();

        _isPacmanAlive = true;
        _timeRemaining = timeToComplete;
        _totalLivesLeft = totalPacmanLives - 1;                
    }

    void CheckInput()
    {
        if(dpad.direction == DpadDirection.None)
        {
            return;
        }

        if (playerController.IsTurningBack(dpad.direction))
        {
            playerController.Look(dpad.direction);

            _isPacmanAlive = false;

            foreach (EnemyController enemy in _currentEnemyPool)
            {
                enemy.Stop();
            }

            if (_totalLivesLeft == 0)
            {
                hudController.ShowGameOver();
            }
            else
            {                                
                _totalLivesLeft--;
                hudController.RemoveLife(_totalLivesLeft);
                _isPlayerDiedOnEnemyEncounter = false;
                RegenPlayer();
            }
            return;
        }

        switch (dpad.direction)
        {
            case DpadDirection.Left:                               
                if(tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(playerTransform.position.x) - 1, Mathf.FloorToInt(playerTransform.position.y), 0)))
                {
                    if (_currentPath.Count > 0)
                    {
                        FillPath();
                    }
                    return;
                }

                break;

            case DpadDirection.Right:
                if(tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(playerTransform.position.x) + 1, Mathf.FloorToInt(playerTransform.position.y), 0)))
                {
                    if (_currentPath.Count > 0)
                    {
                        FillPath();
                    }
                    return;
                }

                break;

            case DpadDirection.Up:
                if(tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(playerTransform.position.x), Mathf.FloorToInt(playerTransform.position.y) + 1, 0)))
                {
                    if (_currentPath.Count > 0)
                    {
                        FillPath();
                    }
                    return;
                }

                break;

            case DpadDirection.Down:
                if(tilemap.HasTile(new Vector3Int(Mathf.FloorToInt(playerTransform.position.x), Mathf.FloorToInt(playerTransform.position.y) - 1, 0)))
                {
                    if (_currentPath.Count > 0)
                    {
                        FillPath();
                    }
                    return;
                }                

                break;

            default:
                break;
        }

        playerController.Move(dpad.direction);        

        DrawPath();
        UpdateProgress();
    }

    void SpawnEnemy()
    {
        if(_currentEnemyPool.Count < maxEnemies && _isPacmanAlive)
        {
            Vector2 topRightCorner = new Vector2(0, 1);
            Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);

            int colStartIndex = (int)edgeVector.x + 4;
            int colEndIndex = colStartIndex + columns - 2;
            int rowStartIndex = (int)edgeVector.y - 4;
            int rowEndIndex = rowStartIndex - rows + 2;

            Vector2 spawnPoint = new Vector2(Random.Range(colStartIndex + 1, colEndIndex - 1), Random.Range(rowStartIndex - 1, rowEndIndex + 1));

            EnemyController enemyController = Instantiate(enemyPrefab, spawnPoint, Quaternion.identity);

            enemyController.index = _currentEnemyPool.Count;

            enemyController.tilemap = tilemap;

            enemyController.normalSpeed = Random.Range(3, 6);

            enemyController.powerUpSpeed = 2;

            enemyController.OnEnemyTrapped += HandleEnemyTrapped;

            enemyController.Move();

            _currentEnemyPool.Add(enemyController);
        }

        _enemySpawnTimer = 0;
    }

    void SpawnPowerUp()
    {
        Vector2 topRightCorner = new Vector2(0, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);

        int colStartIndex = (int)edgeVector.x + 4;
        int colEndIndex = colStartIndex + columns - 2;
        int rowStartIndex = (int)edgeVector.y - 4;
        int rowEndIndex = rowStartIndex - rows + 2;

        Vector2 spawnPoint = new Vector2(Random.Range(colStartIndex + 1, colEndIndex - 1), Random.Range(rowStartIndex - 1, rowEndIndex + 1));

        if(!tilemap.HasTile(new Vector3Int((int)spawnPoint.x, (int)spawnPoint.y, 0)))
        {
            _activePowerUp = Instantiate(powerUpPrefab, spawnPoint, Quaternion.identity);
            _powerUpSpawnTimer = 0;
        }        
    }

    void RegenPlayer()
    {
        playerController.StopPlayer();

        dpad.Hide();

        hudController.ShowPacmanRegenCounter();
    }

    void HandleGameRestart()
    {
        InitGame();
    }

    void HandlePacmanRegenCounterDone()
    {
        dpad.Show();

       
        if(_isPlayerDiedOnEnemyEncounter)
        {
            playerController.Look(dpad.direction);
        }
        else
        {
            switch(dpad.direction)
            {
                case DpadDirection.Left:
                    playerController.Look(DpadDirection.Right);
                    break;

                case DpadDirection.Right:
                    playerController.Look(DpadDirection.Left);
                    break;

                case DpadDirection.Up:
                    playerController.Look(DpadDirection.Down);
                    break;

                case DpadDirection.Down:
                    playerController.Look(DpadDirection.Up);
                    break;
            }
        }

        dpad.direction = DpadDirection.None;

        playerController.StartPlayer();

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            enemy.Move();
        }

        _isPacmanAlive = true;
    }

    void HandleHitEnemy(EnemyController hitEnemy)
    {
        _currentEnemyPool.Remove(hitEnemy);
        Destroy(hitEnemy.gameObject);

        _isPacmanAlive = false;

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            enemy.Stop();
        }

        if (_totalLivesLeft == 0)
        {
            hudController.ShowGameOver();
        }
        else
        {
            _totalLivesLeft--;
            hudController.RemoveLife(_totalLivesLeft);
            _isPlayerDiedOnEnemyEncounter = true;
            RegenPlayer();
        }        
    }

    void HandlePlayerPowerUpCollected(PowerUpController powerUp)
    {
        Destroy(powerUp.gameObject);
        EnablePowerUp();
    }

    void HandleEnemyTrapped(EnemyController enemy)
    {
        _currentEnemyPool.Remove(enemy);
        Destroy(enemy.gameObject);
    }

    void TimeUp()
    {
        _isPacmanAlive = false;

        hudController.ShowTimeUp();

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            enemy.Stop();
        }

        playerController.StopPlayer();
    }

    void GameWon()
    {
        _isPacmanAlive = false;

        hudController.ShowGameWon();

        foreach (EnemyController enemy in _currentEnemyPool)
        {
            enemy.Stop();
        }

        playerController.StopPlayer();
    }

    void UpdateProgress()
    {
        int percentageDone = Mathf.FloorToInt((_filledTiles / _totalTiles) * 100f);        
        hudController.UpdateProgress(percentageDone);

        if (percentageDone >= 80)
        {
            GameWon();
        }
        else if (percentageDone >= 50)
        {
            _currentEnemySpawnInterval = enemySpawnInterval * 0.5f; // Increase enemy spawn frequency after completing more than 50%.
        }
    }

    void DrawPath()
    {
        int tileXPos = Mathf.FloorToInt(playerTransform.position.x);
        int tileYPos = Mathf.FloorToInt(playerTransform.position.y);

        if (!tilemap.HasTile(new Vector3Int(tileXPos, tileYPos, 0)))
        {
            tilemap.SetTile(new Vector3Int(tileXPos, tileYPos, 0), tile);
            _filledTiles += 1;            
            _currentPath.Add(new Vector2Int(tileXPos, tileYPos));
        }
    }

    void FillPath()
    {
        int currentTileX = Mathf.FloorToInt(playerTransform.position.x);
        int currentTileY = Mathf.FloorToInt(playerTransform.position.y);
        if (!tilemap.HasTile(new Vector3Int(currentTileX, currentTileY, 0)))
        {
            tilemap.SetTile(new Vector3Int(currentTileX, currentTileY, 0), tile);
            _filledTiles += 1;
            _currentPath.Add(new Vector2Int(currentTileX, currentTileY));
        }

        int x, y;

        if (_currentPath[_currentPath.Count - 1].y == _currentPath[_currentPath.Count - 2].y)
        {
            x = _currentPath[_currentPath.Count - 1].x;
            y = _currentPath[_currentPath.Count - 1].y + 1;
            _currentPath.Clear();            
            FillTile(x, y);
        }
        else if (_currentPath[_currentPath.Count - 1].x == _currentPath[_currentPath.Count - 2].x)
        {
            x = _currentPath[_currentPath.Count - 1].x - 1;
            y = _currentPath[_currentPath.Count - 1].y;
            _currentPath.Clear();            
            FillTile(x, y);
        }      
    }

    void FillTile(int x, int y)
    {
        if (!tilemap.HasTile(new Vector3Int(x, y, 0)))
        {
            tilemap.SetTile(new Vector3Int(x, y, 0), tile);
            _filledTiles += 1;

            UpdateProgress();

            FillTile(x - 1, y + 1);
            FillTile(x - 1, y);
            FillTile(x - 1, y - 1);
            FillTile(x, y + 1);
            FillTile(x, y - 1);
            FillTile(x + 1, y + 1);
            FillTile(x + 1, y);
            FillTile(x + 1, y - 1);
        }
    }

    void GenerateLevel()
    {
        tilemap.ClearAllTiles();

        Vector2 topRightCorner = new Vector2(0, 1);
        Vector2 edgeVector = Camera.main.ViewportToWorldPoint(topRightCorner);

        int colStartIndex = (int)edgeVector.x + 3;
        int colEndIndex = colStartIndex + columns - 1;
        int rowStartIndex = (int)edgeVector.y - 3;
        int rowEndIndex = rowStartIndex - rows + 1;

        int totalLevelTiles = (rows * 2) + ((columns - 2) * 2);

        Vector3Int[] tileArray = new Vector3Int[totalLevelTiles];
        Tile[] tiles = new Tile[totalLevelTiles];

        int tileArrayCount = 0;

        for (int i = rowStartIndex - 1; i >= rowEndIndex + 1; i--)
        {
            tileArray[tileArrayCount] = new Vector3Int(colStartIndex, i, 0);
            tiles[tileArrayCount] = tile;
            tileArrayCount++;

            tileArray[tileArrayCount] = new Vector3Int(colEndIndex, i, 0);
            tiles[tileArrayCount] = tile;
            tileArrayCount++;
        }

        for (int i = colStartIndex; i <= colEndIndex; i++)
        {
            tileArray[tileArrayCount] = new Vector3Int(i, rowStartIndex, 0);
            tiles[tileArrayCount] = tile;
            tileArrayCount++;

            tileArray[tileArrayCount] = new Vector3Int(i, rowEndIndex, 0);
            tiles[tileArrayCount] = tile;
            tileArrayCount++;
        }

        tilemap.SetTiles(tileArray, tiles);

        _startXPos = colStartIndex + 2;
        _startYPos = rowStartIndex;

        _totalTiles = (rows - 2) * (columns - 2);
    }
}

public enum CharacterDirection
{
    Left,
    Right,
    Up,
    Down
}
