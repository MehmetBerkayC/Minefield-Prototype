using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Game : MonoBehaviour
{
    [SerializeField] TextMeshPro minesText;

    [SerializeField, Min(1)]
    int rows = 8, columns = 21, mines  = 30;

    [SerializeField]
    Material material;
    
    [SerializeField]
    Mesh mesh;

    [System.Flags]
    public enum CellState
    {   // Grid is hexagonal, max 6 neighbours 
        Zero, One, Two, Three, Four, Five, Six,
        // store this information in the unused bits of CellState.      |
        // Values up to six require three bits,                         |
        // so we can use the fourth bit as a flag to indicate a mine... V
        Mine = 1 << 3,
        MarkedSure = 1 << 4,
        MarkedUnsure = 1 << 5,
        Revealed = 1 << 6,
        // combined masks to make checking the cell states easier
        Marked = MarkedSure | MarkedUnsure,
        MarkedOrRevealed = Marked | Revealed,
        MarkedSureOrMine = MarkedSure | Mine
    }

    Grid grid;
    GridVisualization visualization;

    int markedSureCount;

    bool isGameOver;

    private void OnEnable()
    {
        grid.Initialize(rows, columns);
        visualization.Initialize(grid, material, mesh);
        StartGame();
    }

    IEnumerator StartNewGame()
    {
        yield return new WaitForSeconds(3f);
        StartGame();
    }

    private void StartGame()
    {
        isGameOver = false;
        mines = Mathf.Min(mines, grid.CellCount);
        minesText.SetText("{0}", mines);
        markedSureCount = 0;
        grid.PlaceMines(mines);
        visualization.Update();
    }

    private void OnDisable()
    {
        grid.Dispose();
        visualization.Dispose();
    }

    private void Update()
    {
        // Remake if not right or boundaries change
        if (grid.Rows != rows || grid.Columns != columns)
        {
            OnDisable();
            OnEnable();
        }

        // Draw Grid and take Input
        PerformAction();
        visualization.Draw();
    }
    void PerformAction()
    {
        bool revealAction = Input.GetMouseButton(0);
        bool markAction = Input.GetMouseButtonDown(1);
        if (
            (revealAction || markAction)
            && visualization.TryGetHitCellIndex(
                Camera.main.ScreenPointToRay(Input.mousePosition), out int cellIndex))
        {
            if (isGameOver)
            {
                StartCoroutine("StartNewGame");
            }
            
            if (revealAction)
            {
                DoRevealAction(cellIndex);
            }
            else 
            {
                DoMarkAction(cellIndex); 
            }
        }
    }

    void DoMarkAction(int cellIndex)
    {
        CellState state = grid[cellIndex];
        if (state.Is(CellState.Revealed))
        {
            return;
        }

        if (state.IsNot(CellState.Marked))
        {
            grid[cellIndex] = state.With(CellState.MarkedSure);
            markedSureCount += 1;
        }
        else if (state.Is(CellState.MarkedSure))
        {
            grid[cellIndex] =
                state.Without(CellState.MarkedSure).With(CellState.MarkedUnsure);
            markedSureCount -= 1;
        }
        else
        {
            grid[cellIndex] = state.Without(CellState.MarkedUnsure);
        }

        minesText.SetText("{0}", mines - markedSureCount);
    }
    void DoRevealAction(int cellIndex)
    {
        CellState state = grid[cellIndex];
        if (state.Is(CellState.MarkedOrRevealed))
        {
            return;
        }

        grid.Reveal(cellIndex);
        if (state.Is(CellState.Mine))
        {
            isGameOver = true;
            minesText.SetText("Game Over");
            grid.RevealMinesAndMistakes();
        }
        else if (grid.HiddenCellCount == mines)
        {
            isGameOver = true;
            minesText.SetText("Game Won");
        }
    }
}
