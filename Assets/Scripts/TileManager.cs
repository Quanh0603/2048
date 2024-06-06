using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEngine.Events;
public class TileManager : MonoBehaviour
{
    public static int GridSize = 4;

    private readonly Transform[,] _tilePositsions = new Transform[GridSize, GridSize];

    private readonly Tile[,] _tiles = new Tile[GridSize, GridSize];

    [SerializeField] private Tile _tilePrefab;
    [SerializeField] private TileSetting _tileSetting;
    [SerializeField] private UnityEvent<int> _scoreUpdated;
    [SerializeField] private UnityEvent<int> _bestScoreUpdated;
    [SerializeField] private GameOverScreen _gameOverScreen;
    
    private bool _isAnimating;
    private int _lastXInput;
    private int _lastYInput;
    private int _score;
    private int _bestScore;
    
    // Start is called before the first frame update
    void Start()
    {
        
        GetTilePositions();
        TrySpawnTile(); 
        TrySpawnTile();
        UpdateTilePositions(true);

        _bestScore = PlayerPrefs.GetInt("BestScore", 0);
        _bestScoreUpdated.Invoke(_bestScore);
    }
    
    // Update is called once per frame
    private void Update()
    { 
        var xInput = Mathf.RoundToInt(Input.GetAxisRaw("Horizontal"));
        var yInput = Mathf.RoundToInt(Input.GetAxisRaw("Vertical"));
        if((_lastXInput == 0 && _lastYInput == 0 ) && !_isAnimating)
             TryMove(xInput,yInput);
        _lastXInput = xInput;
        _lastYInput = yInput;
    }
    public void AddScore(int value)
    {
        _score += value;
        _scoreUpdated.Invoke(_score);
        if (_score > _bestScore)
        {
            _bestScore = _score;
            _bestScoreUpdated.Invoke(_bestScore);
            PlayerPrefs.SetInt("BestScore", _bestScore);
        }
    }
    void GetTilePositions()
    {
        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        int x = 0;  
        int y = 0;
        foreach(Transform positsion in this.transform) {
            //biến lưu trữ vị trí trống hả?
            _tilePositsions[x, y] = positsion;
            x++;
            if(x >= GridSize)
            {
                x = 0;
                y++;
            }
        }
    }

    private bool TrySpawnTile()
    {
        List<Vector2Int> availableSpots = new List<Vector2Int>();
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] == null)
                {
                    availableSpots.Add(new Vector2Int(x, y));
                }
            }
        }
        
        if (!availableSpots.Any())
        {
            return false;
        }

        int randomIndex = Random.Range(0, availableSpots.Count);
        Vector2Int spot = availableSpots[randomIndex];

        var tile = Instantiate(_tilePrefab, transform.parent);
        tile.SetValue(GetRandomValue());

        _tiles[spot.x, spot.y] = tile;
        return true;
    }
    private int GetRandomValue()
    {
        var rand = Random.Range(0f, 1f);

        if (rand <= 0.8f)
        {
            return 2;
        }
        else
        {
            return 4;
        }
    }
    
    private void UpdateTilePositions(bool instant)
    {
        if (!instant)
        {
            _isAnimating = true;
            StartCoroutine(WaitForTileAnimation());
        }
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x, y] != null)
                    _tiles[x, y].SetPosition(_tilePositsions[x, y].position, instant);
            }
        }
    }

    // ReSharper disable Unity.PerformanceAnalysis
    private IEnumerator WaitForTileAnimation()
    {
        yield return new WaitForSeconds(_tileSetting.animationTime);

        if (!TrySpawnTile())
        {
            Debug.LogError("Unable to spawn tile");
        }
        UpdateTilePositions(true);

        if (!AnyMoveLeft())
        {
            _gameOverScreen.SetGameOver(true);
        }
        _isAnimating = false;

    }

    private bool AnyMoveLeft()
    {
        return CanMoveDown() || CanMoveLeft() || CanMoveRight() || CanMoveUp();
    }

    //need to update position?
    private bool _tilesUpdated;
    //x,y direction to move
    private void TryMove(int x, int y)
    {
        if (x == 0 && y == 0)
            return;
        //avoid player pressing too much
        if (Mathf.Abs(x) == 1 && Mathf.Abs(y) == 1)
        {
            
            return;
        }

        _tilesUpdated = false;
        if (x == 0)
        {
            if (y > 0)
                TryMoveUp();
            else
                TryMoveDown();
        }
        else
        {
            if (x > 0)
                TryMoveRight();
            else
                TryMoveLeft();
        }
        if (_tilesUpdated)
        {
            UpdateTilePositions(false);
        }
    }

    private bool CheckTileBetween(int x, int y, int x2, int y2)
    {
        if (x == x2)
            return TileExistVertical(x, y, y2);
        else if (y == y2)
            return TileExistHorizontal(x, y, x2);
        return true;
    }

    private bool TileExistHorizontal(int x, int y, int x2)
    {
        int minX = Math.Min(x, x2);
        int maxX = Math.Max(x, x2);
        //checking the middle of two tiles have the same value
        for (int xIndex = minX + 1; xIndex < maxX; xIndex++)
        {
            if (_tiles[xIndex, y] != null)
            {
                return true;
            }
        }
        return false;
    }

    private bool TileExistVertical(int x, int y, int y2)
    {
        int minY = Math.Min(y, y2);
        int maxY = Math.Max(y, y2);
        for (int yIndex = minY + 1 ; yIndex < maxY; yIndex++)
        {
            if (_tiles[x,yIndex] != null)
            {
                return true;
            }
        }
        return false;
    }

    private void TryMoveLeft()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (_tiles[x, y] == null) continue;
                for (int x2 = 0; x2 < x; x2++)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (CheckTileBetween(x,y,x2,y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                            break;
                        }
                        continue;
                    }
                    
                    _tilesUpdated = true;
                    _tiles[x2, y]= _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }
            }
        }
    }

    private void TryMoveRight()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = GridSize - 1; x >= 0; x--)
            {
                if (_tiles[x, y] == null) continue;
                //x2 find empty location 
                for (int x2 = GridSize - 1; x2 > x; x2--)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (CheckTileBetween(x,y,x2,y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                            break;
                        }
                        continue;
                    }

                    _tilesUpdated = true;
                    _tiles[x2, y] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                    
                }
            }
        }
    }

    private void TryMoveDown()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_tiles[x,y]==null) continue;
                for (int y2 = GridSize - 1; y2 > y; y2--)
                {
                    if (_tiles[x, y2] != null) {
                        if (CheckTileBetween(x,y,x,y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;    
                            _tilesUpdated = true;
                            break;
                        }
                        continue; 
                    }
                    
                    _tilesUpdated = true;
                    _tiles[x, y2] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }
            }
        }
    }

    private void TryMoveUp()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x,y] == null) continue;
                for (int y2 = 0; y2 < y; y2++)
                {   
                    // merging and disable the one merged
                    if (_tiles[x,y2] != null)
                    {
                        if (CheckTileBetween(x,y,x,y2))
                        {
                            continue;
                        }   
                        if (_tiles[x, y2].Merge(_tiles[x, y]))
                        {
                            _tiles[x, y] = null;
                            _tilesUpdated = true;
                            break;
                        }
                        continue;
                    }
                    
                    _tilesUpdated = true;
                    _tiles[x, y2] = _tiles[x, y];
                    _tiles[x, y] = null;
                    break;
                }
            }
        }
    }
   
    
    //---------------------//
     private bool CanMoveLeft()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = 0; x < GridSize; x++)
            {
                if (_tiles[x, y] == null) continue;
                for (int x2 = 0; x2 < x; x2++)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (CheckTileBetween(x,y,x2,y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }

                    return true;
                }
            }
        }

        return false;
    }

    private bool CanMoveRight()
    {
        for (int y = 0; y < GridSize; y++)
        {
            for (int x = GridSize - 1; x >= 0; x--)
            {
                if (_tiles[x, y] == null) continue;
                //x2 find empty location 
                for (int x2 = GridSize - 1; x2 > x; x2--)
                {
                    if (_tiles[x2, y] != null)
                    {
                        if (CheckTileBetween(x,y,x2,y))
                        {
                            continue;
                        }
                        if (_tiles[x2, y].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanMoveDown()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = GridSize - 1; y >= 0; y--)
            {
                if (_tiles[x,y]==null) continue;
                for (int y2 = GridSize - 1; y2 > y; y2--)
                {
                    if (_tiles[x, y2] != null) {
                        if (CheckTileBetween(x,y,x,y2))
                        {
                            continue;
                        }
                        if (_tiles[x, y2].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue; 
                    }
                    return true;
                }
            }
        }
        return false;
    }

    private bool CanMoveUp()
    {
        for (int x = 0; x < GridSize; x++)
        {
            for (int y = 0; y < GridSize; y++)
            {
                if (_tiles[x,y] == null) continue;
                for (int y2 = 0; y2 < y; y2++)
                {   
                    // merging and disable the one merged
                    if (_tiles[x,y2] != null)
                    {
                        if (CheckTileBetween(x,y,x,y2))
                        {
                            continue;
                        }   
                        if (_tiles[x, y2].CanMerge(_tiles[x, y]))
                        {
                            return true;
                        }
                        continue;
                    }
                    return true;
                }
            }
        }
        return false;
    }
}
