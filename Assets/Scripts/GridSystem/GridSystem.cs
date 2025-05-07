using System.Collections.Generic;
using UnityEngine;

//I feel like my brain not braining when doing this.
public class GridSystem : MonoBehaviour
{
    public enum CellType
    {
        Regular,
        Obstacle,
        Path,
        Start,
        End
    }

    private struct LargeCell
    {
        public Vector2Int position;
        public int size;
        public GameObject prefab;
    }

    #region Fields
    [Header("Grid Settings")]
    [SerializeField] public int _gridWidth = 10;
    [SerializeField] public int _gridHeight = 10;
    [SerializeField] public float _cellSize = 2f;

    [Header("Cell Prefabs")]
    [SerializeField] private List<GameObject> _cell1x1Prefabs;
    [SerializeField] private List<GameObject> _cell2x2Prefabs;
    [SerializeField] private List<GameObject> _cell3x3Prefabs;
    [SerializeField] private List<GameObject> _hostileCellPrefabs;
    [SerializeField] private GameObject _path1x1CellPrefab;
    [SerializeField] private GameObject _path2x2CellPrefab;
    [SerializeField] private GameObject _path3x3CellPrefab;
    [SerializeField] private GameObject _startCellPrefab;
    [SerializeField] private GameObject _endCellPrefab;

    [Header("Cell Settings")]
    [SerializeField, Tooltip("Min height of environment cells.")] public float _minCellHeight = 0f;
    [SerializeField, Tooltip("Max height of environment cells.")] public float _maxCellHeight = 1f;
    [SerializeField] private float _largeCellProbability3x3 = 0.3f;
    [SerializeField] private float _largeCellProbability2x2 = 0.5f;

    private CellType[,] _grid;
    private bool[,] _occupied;
    private DifficultyManager _difficultyManager;
    #endregion

    #region Unity Methods
    private void Start()
    {
        if (!ValidateGridDimensions()) return;

        InitializeComponents();
        InitializeGrid();

        Vector2Int startPos = SelectStartPosition();
        Vector2Int endPos = SelectEndPosition(startPos);
        GenerateObstaclesAndPath(startPos, endPos);

        _occupied = new bool[_gridWidth, _gridHeight];
        List<LargeCell> largePathCells = PlaceLargePathCells();
        List<LargeCell> largeCells = PlaceLargeCells();

        InstantiateGrid(largePathCells, largeCells);
    }
    #endregion

    #region Initialization
    // Validation but this is for me and the designer :D
    private bool ValidateGridDimensions()
    {
        if (_gridWidth <= 1 || _gridHeight <= 1)
        {
            Debug.LogError("Grid width and height must be greater than 1.");
            return false;
        }
        return true;
    }

    private void InitializeComponents()
    {
        _difficultyManager = DifficultyManager.Instance;
        if (_difficultyManager == null)
        {
            Debug.LogError("DifficultyManager component not found.");
            return;
        }
        Pathfinder.SetGridDimensions(_gridWidth, _gridHeight);
    }

    private void InitializeGrid()
    {
        _grid = new CellType[_gridWidth, _gridHeight];
        for (int x = 0; x < _gridWidth; x++)
            for (int y = 0; y < _gridHeight; y++)
                _grid[x, y] = CellType.Regular;
    }
    #endregion

    #region Start and End Position Selection
    // With this the start and end positions are always further apart from each other.
    // No more small paths. Hell Yeah.
    private Vector2Int SelectStartPosition()
    {
        List<Vector2Int> edges = GetGridEdges();
        Vector2Int startPos = edges[Random.Range(0, edges.Count)];
        _grid[startPos.x, startPos.y] = CellType.Start;
        return startPos;
    }

    private Vector2Int SelectEndPosition(Vector2Int startPos)
    {
        List<Vector2Int> oppositeEdges = GetOppositeEdges(startPos);
        Vector2Int endPos = oppositeEdges[Random.Range(0, oppositeEdges.Count)];
        _grid[endPos.x, endPos.y] = CellType.End;
        return endPos;
    }

    private List<Vector2Int> GetGridEdges()
    {
        List<Vector2Int> edges = new List<Vector2Int>();
        for (int x = 0; x < _gridWidth; x++)
        {
            edges.Add(new Vector2Int(x, 0));
            edges.Add(new Vector2Int(x, _gridHeight - 1));
        }
        for (int y = 1; y < _gridHeight - 1; y++)
        {
            edges.Add(new Vector2Int(0, y));
            edges.Add(new Vector2Int(_gridWidth - 1, y));
        }
        return edges;
    }

    private List<Vector2Int> GetOppositeEdges(Vector2Int startPos)
    {
        List<Vector2Int> oppositeEdges = new List<Vector2Int>();
        if (startPos.y == 0)
            for (int x = 0; x < _gridWidth; x++)
                oppositeEdges.Add(new Vector2Int(x, _gridHeight - 1));
        else if (startPos.y == _gridHeight - 1)
            for (int x = 0; x < _gridWidth; x++)
                oppositeEdges.Add(new Vector2Int(x, 0));
        else if (startPos.x == 0)
            for (int y = 0; y < _gridHeight; y++)
                oppositeEdges.Add(new Vector2Int(_gridWidth - 1, y));
        else if (startPos.x == _gridWidth - 1)
            for (int y = 0; y < _gridHeight; y++)
                oppositeEdges.Add(new Vector2Int(0, y));
        return oppositeEdges;
    }
    #endregion

    #region Path Generation
    // This is the path. The logic is to generate a path is in the Pathfinder class.
    // I try to use BFS but it suck so I come back to A*.
    // I love A*.
    private void GenerateObstaclesAndPath(Vector2Int startPos, Vector2Int endPos)
    {
        List<Vector2Int> mainPath;
        int attempts = 0;
        const int maxAttempts = 5;

        do
        {
            ScatterObstacles();
            mainPath = GeneratePath(startPos, endPos);
            attempts++;
        } while ((mainPath == null || mainPath.Count == 0) && attempts < maxAttempts);

        if (mainPath == null || mainPath.Count == 0)
        {
            Debug.LogError("Failed to generate a valid path after multiple attempts.");
            return;
        }

        MarkPathOnGrid(mainPath);
        EnsureMultiplePaths(startPos, endPos);
    }

    // In hope that the path will move around the obstacles.
    // But it doesn't work as intended but it still works.
    // I guess this is a feature now.
    private void ScatterObstacles()
    {
        float obstacleProbability = _difficultyManager.GetObstacleProbability();
        for (int x = 0; x < _gridWidth; x++)
            for (int y = 0; y < _gridHeight; y++)
            {
                if (_grid[x, y] == CellType.Regular && Random.value < obstacleProbability)
                    _grid[x, y] = CellType.Obstacle;
                else if (_grid[x, y] == CellType.Obstacle && Random.value >= obstacleProbability)
                    _grid[x, y] = CellType.Regular;
            }
    }

    private List<Vector2Int> GeneratePath(Vector2Int start, Vector2Int end)
    {
        return Pathfinder.FindPath(start, end, pos => _grid[pos.x, pos.y] != CellType.Obstacle);
    }

    private void MarkPathOnGrid(List<Vector2Int> path)
    {
        foreach (var pos in path)
            if (_grid[pos.x, pos.y] != CellType.Start && _grid[pos.x, pos.y] != CellType.End)
                _grid[pos.x, pos.y] = CellType.Path;
    }

    // This is not work as intended as well.
    // Need a better solution for this.
    private void EnsureMultiplePaths(Vector2Int startPos, Vector2Int endPos)
    {
        int numAdditionalPaths = _difficultyManager.GetNumWaypoints();

        for (int i = 0; i < numAdditionalPaths; i++)
        {
            List<Vector2Int> additionalPath = GenerateAlternativePath(startPos, endPos);
            if (additionalPath != null && additionalPath.Count > 0)
                MarkPathOnGrid(additionalPath);
            else
                Debug.LogWarning($"Could not generate additional path {i + 1}. Grid may be too constrained.");
        }
    }

    private List<Vector2Int> GenerateAlternativePath(Vector2Int start, Vector2Int end)
    {
        return Pathfinder.FindPath(start, end, pos => _grid[pos.x, pos.y] == CellType.Regular);
    }
    #endregion

    #region Cell Placement
    // Placing trap and enemies and jamming big cells 2x2 and 3x3 in the path.
    private List<LargeCell> PlaceLargePathCells()
    {
        List<LargeCell> largePathCells = new List<LargeCell>();
        List<Vector2Int> pathCells = GetPathCells();

        int minHostileCells = _difficultyManager.GetMinHostileCells();
        List<Vector2Int> possibleHostilePositions = GetPossibleHostilePositions(pathCells);
        while (minHostileCells > 0 && possibleHostilePositions.Count > 0)
        {
            int index = Random.Range(0, possibleHostilePositions.Count);
            Vector2Int pos = possibleHostilePositions[index];
            GameObject prefab = _hostileCellPrefabs[Random.Range(0, _hostileCellPrefabs.Count)];
            largePathCells.Add(new LargeCell { position = pos, size = 3, prefab = prefab });
            OverrideAreaAsPath(pos.x, pos.y, 3);
            MarkOccupied(pos.x, pos.y, 3);
            minHostileCells--;
            possibleHostilePositions = GetPossibleHostilePositions(pathCells);
        }

        int minLargePathCells = _difficultyManager.GetMinLargePathCells();
        while (minLargePathCells > 0 && pathCells.Count > 0)
        {
            int size = Random.value < 0.5f ? 2 : 3;
            List<Vector2Int> possiblePositions = GetPossibleLargePathPositions(pathCells, size);
            if (possiblePositions.Count > 0)
            {
                int index = Random.Range(0, possiblePositions.Count);
                Vector2Int pos = possiblePositions[index];
                GameObject prefab = size == 2 ? _path2x2CellPrefab : _path3x3CellPrefab;
                largePathCells.Add(new LargeCell { position = pos, size = size, prefab = prefab });
                OverrideAreaAsPath(pos.x, pos.y, size);
                MarkOccupied(pos.x, pos.y, size);
                minLargePathCells--;
                pathCells = GetPathCells();
            }
            else if (size == 3)
            {
                size = 2;
                possiblePositions = GetPossibleLargePathPositions(pathCells, size);
                if (possiblePositions.Count > 0)
                {
                    int index = Random.Range(0, possiblePositions.Count);
                    Vector2Int pos = possiblePositions[index];
                    largePathCells.Add(new LargeCell { position = pos, size = size, prefab = _path2x2CellPrefab });
                    OverrideAreaAsPath(pos.x, pos.y, size);
                    MarkOccupied(pos.x, pos.y, size);
                    minLargePathCells--;
                    pathCells = GetPathCells();
                }
            }
            else
                break;
        }

        foreach (var pos in pathCells)
        {
            if (!_occupied[pos.x, pos.y])
            {
                if (Random.value < _largeCellProbability3x3 && CanPlaceLargeCellWithOverride(pos.x, pos.y, 3))
                {
                    largePathCells.Add(new LargeCell { position = pos, size = 3, prefab = _path3x3CellPrefab });
                    OverrideAreaAsPath(pos.x, pos.y, 3);
                    MarkOccupied(pos.x, pos.y, 3);
                }
                else if (Random.value < _largeCellProbability2x2 && CanPlaceLargeCellWithOverride(pos.x, pos.y, 2))
                {
                    largePathCells.Add(new LargeCell { position = pos, size = 2, prefab = _path2x2CellPrefab });
                    OverrideAreaAsPath(pos.x, pos.y, 2);
                    MarkOccupied(pos.x, pos.y, 2);
                }
            }
        }

        return largePathCells;
    }

    private List<LargeCell> PlaceLargeCells()
    {
        List<LargeCell> largeCells = new List<LargeCell>();
        for (int x = 0; x < _gridWidth; x++)
            for (int y = 0; y < _gridHeight; y++)
            {
                if (_grid[x, y] == CellType.Regular && !_occupied[x, y])
                {
                    if (Random.value < _largeCellProbability3x3 && CanPlaceLargeCell(x, y, 3, CellType.Regular))
                    {
                        GameObject prefab = _cell3x3Prefabs[Random.Range(0, _cell3x3Prefabs.Count)];
                        largeCells.Add(new LargeCell { position = new Vector2Int(x, y), size = 3, prefab = prefab });
                        MarkOccupied(x, y, 3);
                    }
                    else if (Random.value < _largeCellProbability2x2 && CanPlaceLargeCell(x, y, 2, CellType.Regular))
                    {
                        GameObject prefab = _cell2x2Prefabs[Random.Range(0, _cell2x2Prefabs.Count)];
                        largeCells.Add(new LargeCell { position = new Vector2Int(x, y), size = 2, prefab = prefab });
                        MarkOccupied(x, y, 2);
                    }
                }
            }
        return largeCells;
    }

    private List<Vector2Int> GetPathCells()
    {
        List<Vector2Int> pathCells = new List<Vector2Int>();
        for (int x = 0; x < _gridWidth; x++)
            for (int y = 0; y < _gridHeight; y++)
                if (_grid[x, y] == CellType.Path && !_occupied[x, y])
                    pathCells.Add(new Vector2Int(x, y));
        return pathCells;
    }

    private List<Vector2Int> GetPossibleHostilePositions(List<Vector2Int> pathCells)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        int size = 3;
        foreach (var pos in pathCells)
            if (CanPlaceLargeCellWithOverride(pos.x, pos.y, size))
                positions.Add(pos);
        return positions;
    }

    private List<Vector2Int> GetPossibleLargePathPositions(List<Vector2Int> pathCells, int size)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        foreach (var pos in pathCells)
            if (CanPlaceLargeCellWithOverride(pos.x, pos.y, size))
                positions.Add(pos);
        return positions;
    }

    // Allow to override the area with big cells except start and end cells.
    private bool CanPlaceLargeCellWithOverride(int x, int y, int size)
    {
        if (x < 0 || y < 0 || x + size > _gridWidth || y + size > _gridHeight) return false;
        for (int i = x; i < x + size; i++)
            for (int j = y; j < y + size; j++)
                if (_occupied[i, j] || _grid[i, j] == CellType.Start || _grid[i, j] == CellType.End)
                    return false;
        return true;
    }

    private bool CanPlaceLargeCell(int x, int y, int size, CellType requiredType)
    {
        if (x < 0 || y < 0 || x + size > _gridWidth || y + size > _gridHeight) return false;
        for (int i = x; i < x + size; i++)
            for (int j = y; j < y + size; j++)
                if (_grid[i, j] != requiredType || _occupied[i, j])
                    return false;
        return true;
    }

    private void OverrideAreaAsPath(int x, int y, int size)
    {
        for (int i = x; i < x + size; i++)
            for (int j = y; j < y + size; j++)
                if (_grid[i, j] != CellType.Start && _grid[i, j] != CellType.End)
                    _grid[i, j] = CellType.Path;
    }

    private void MarkOccupied(int x, int y, int size)
    {
        for (int i = x; i < x + size; i++)
            for (int j = y; j < y + size; j++)
                _occupied[i, j] = true;
    }
    #endregion

    #region Grid Instantiation
    // Just code to instantiate the grid.
    // Priority: big path cells > big cells > small cells
    private void InstantiateGrid(List<LargeCell> largePathCells, List<LargeCell> largeCells)
    {
        GameObject gridHolder = new GameObject("Grid");
        InstantiateLargeCells(largePathCells, gridHolder);
        InstantiateLargeCells(largeCells, gridHolder);
        InstantiateRemainingCells(gridHolder);
        InstantiateBorder(gridHolder);
    }

    private void InstantiateLargeCells(List<LargeCell> largeCells, GameObject parent)
    {
        foreach (var largeCell in largeCells)
        {
            Vector2Int pos = largeCell.position;
            int size = largeCell.size;
            Vector3 position = new Vector3((pos.x + (size - 1) / 2f) * _cellSize, 0, (pos.y + (size - 1) / 2f) * _cellSize);
            Instantiate(largeCell.prefab, position, Quaternion.identity, parent.transform);
        }
    }

    private void InstantiateRemainingCells(GameObject parent)
    {
        for (int x = 0; x < _gridWidth; x++)
            for (int y = 0; y < _gridHeight; y++)
            {
                if (_occupied[x, y]) continue;

                Vector3 position = new Vector3(x * _cellSize, 0, y * _cellSize);
                Quaternion rotation = Quaternion.identity;
                GameObject prefab = GetPrefabForCellType(_grid[x, y]);
                if (prefab != null)
                {
                    if (_grid[x, y] == CellType.Regular || _grid[x, y] == CellType.Obstacle)
                    {
                        position.y = Random.Range(_minCellHeight, _maxCellHeight);
                        rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
                    }
                    Instantiate(prefab, position, rotation, parent.transform);
                }
            }
    }

    private GameObject GetPrefabForCellType(CellType cellType)
    {
        switch (cellType)
        {
            case CellType.Regular:
            case CellType.Obstacle:
                return _cell1x1Prefabs.Count > 0 ? _cell1x1Prefabs[Random.Range(0, _cell1x1Prefabs.Count)] : null;
            case CellType.Path:
                return _path1x1CellPrefab;
            case CellType.Start:
                return _startCellPrefab;
            case CellType.End:
                return _endCellPrefab;
            default:
                return null;
        }
    }

    // Surrounding the grid with 1x1 prefabs.
    // You don't want to see the character falling off the grid.
    private void InstantiateBorder(GameObject parent)
    {
        for (int x = -1; x <= _gridWidth; x++)
            for (int y = -1; y <= _gridHeight; y++)
            {
                if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight) continue;
                if (_cell1x1Prefabs.Count == 0) continue;

                Vector3 position = new Vector3(x * _cellSize, Random.Range(_minCellHeight, _maxCellHeight), y * _cellSize);
                Quaternion rotation = Quaternion.Euler(0, Random.Range(0, 4) * 90f, 0);
                Instantiate(_cell1x1Prefabs[Random.Range(0, _cell1x1Prefabs.Count)], position, rotation, parent.transform);
            }
    }
    #endregion
    // I guess no one will read this, but if you do, pls buy me some candy, my head hurts doing this.
}