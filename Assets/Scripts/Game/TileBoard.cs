using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TileBoard : MonoBehaviour
{
    [SerializeField] private Tile tilePrefab;
    [SerializeField] private TileImage[] tileImages; // Array of TileImage
    public GameManager gameManager;
    private bool hasWon = false;
    private TileGrid grid;
    private List<Tile> tiles;
    private bool waiting;
    private Vector2 touchStartPos;
    private Vector2 touchEndPos;
    private float swipeThreshold = 50f;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();
        tiles = new List<Tile>(16);
    }

    public int[,] GetBoardState()
    {
        int width = grid.Width;
        int height = grid.Height;
        int[,] boardState = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileCell cell = grid.GetCell(x, y);
                boardState[x, y] = cell.Occupied ? cell.tile.imageData.number : 0; // Updated to use imageData
            }
        }

        return boardState;
    }

    public void SetBoardState(int[,] boardState)
    {
        ClearBoard();

        int width = grid.Width;
        int height = grid.Height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int number = boardState[x, y];
                if (number != 0)
                {
                    Tile tile = CreateTileAtPosition(number, x, y);
                    tiles.Add(tile);
                }
            }
        }
    }

    private Tile CreateTileAtPosition(int number, int x, int y)
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        TileImage image = GetTileImageByNumber(number);
        tile.SetImage(image); // Set the tile's image
        TileCell cell = grid.GetCell(x, y);
        tile.Spawn(cell);
        return tile;
    }

    private TileImage GetTileImageByNumber(int number)
    {
        foreach (TileImage image in tileImages)
        {
            if (image.number == number)
            {
                return image;
            }
        }
        return null;
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;
        }

        foreach (var tile in tiles)
        {
            Destroy(tile.gameObject);
        }

        tiles.Clear();
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);
        tile.SetImage(tileImages[0]); // Set the image for the new tile
        tile.Spawn(grid.GetRandomEmptyCell());
        tiles.Add(tile);
    }

    private void Update()
    {
        if (waiting) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStartPos = touch.position;
                    break;

                case TouchPhase.Ended:
                    touchEndPos = touch.position;
                    HandleSwipe();
                    break;
            }
        }
    }

    void HandleSwipe()
    {
        Vector2 swipeDelta = touchEndPos - touchStartPos;

        if (swipeDelta.magnitude > swipeThreshold)
        {
            float x = Mathf.Abs(swipeDelta.x);
            float y = Mathf.Abs(swipeDelta.y);

            bool moved = false;

            if (x > y)
            {
                if (swipeDelta.x > 0)
                {
                    moved = Move(Vector2Int.right, grid.Width - 2, -1, 0, 1);
                }
                else
                {
                    moved = Move(Vector2Int.left, 1, 1, 0, 1);
                }
            }
            else
            {
                if (swipeDelta.y > 0)
                {
                    moved = Move(Vector2Int.up, 0, 1, 1, 1);
                }
                else
                {
                    moved = Move(Vector2Int.down, 0, 1, grid.Height - 2, -1);
                }
            }

            if (moved)
            {
                SaveState();
            }
        }
    }

    private bool Move(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)
    {
        bool changed = false;

        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }

        return changed;
    }

    private bool MoveTile(Tile tile, Vector2Int direction)
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)
        {
            if (adjacent.Occupied)
            {
                if (CanMerge(tile, adjacent.tile))
                {
                    MergeTiles(tile, adjacent.tile);
                    return true;
                }

                break;
            }

            newCell = adjacent;
            adjacent = grid.GetAdjacentCell(adjacent, direction);
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);
            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        return a.imageData == b.imageData && !b.locked; // Updated to use imageData
    }

    private void MergeTiles(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);

        // Update to use TileImage instead of TileState
        int newIndex = Mathf.Clamp(IndexOf(b.imageData.number) + 1, 0, tileImages.Length - 1);
        TileImage newImage = tileImages[newIndex];

        b.SetImage(newImage);
        GameManager.Instance.IncreaseScore(newImage.number);
    }

    private int IndexOf(int number)
    {
        for (int i = 0; i < tileImages.Length; i++)
        {
            if (number == tileImages[i].number)
            {
                return i;
            }
        }

        return -1;
    }

    private IEnumerator WaitForChanges()
    {
        waiting = true;

        yield return new WaitForSeconds(0.1f);

        waiting = false;

        foreach (var tile in tiles)
        {
            tile.locked = false;
        }

        if (tiles.Count != grid.Size)
        {
            CreateTile();
        }
        if(!hasWon && CheckForGameWin())
        {
            hasWon = true;
            GameManager.Instance.GameWin();
        }

        if (CheckForGameOver())
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            SaveState();
        }
    }
    private void SaveState()
    {
        if (gameManager != null)
        {
            int[,] boardState = GetBoardState();
            gameManager.SaveState(); // Pass the board state if needed
        }
    }


    public bool CheckForGameOver()
    {
        if (tiles.Count != grid.Size)
        {
            return false;
        }

        foreach (var tile in tiles)
        {
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            if ((up != null && CanMerge(tile, up.tile)) ||
                (down != null && CanMerge(tile, down.tile)) ||
                (left != null && CanMerge(tile, left.tile)) ||
                (right != null && CanMerge(tile, right.tile)))
            {
                return false;
            }
        }

        return true;
    }
    public bool CheckForGameWin()
    {
        foreach (var tile in tiles)
        {
            if (tile != null && tile.imageData.number == 2024)
            {
                return true;
            }
        }

        return false;
    }

}
