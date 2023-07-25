using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

using static Game;

public struct Grid {
    public int Rows { get; private set; }
    
    public int Columns { get; private set; }

    public int CellCount => states.Length;

    //A NativeArray is really just a wrapper that points at an array in the "native" code
    //(inside the unity engine itself outside of the mono runtime).
    //And that array is technically in a completely different part of memory...
    //not the stack, nor the heap.
    NativeArray<CellState> states;

    public void Initialize (int rows, int columns)
    {
        Rows = rows;
        Columns = columns;
        states = new NativeArray<CellState>(Rows * Columns, Allocator.Persistent);
    }

    public void Dispose() => states.Dispose();    

    public CellState this[int i]
    {
        get => states[i];
        set => states[i] = value;
    }

    public int GetCellIndex(int row, int column) => row * Columns + column;

    public bool TryGetCellIndex ( int row, int column, out int index)
    {
        bool valid = 0 <= row && row < Rows && 0 <= column && column < Columns;
        index = valid ? GetCellIndex(row, column) : -1;
        return valid;
    }
    public void GetRowColumn (int index, out int row, out int column)
	{
		row = index / Columns;
		column = index - row * Columns;
	}

    public void PlaceMines(int mines) => new PlaceMinesJob
    {
        grid = this,
        mines = mines,
        seed = Random.Range(1, int.MaxValue)
    }.Schedule().Complete();
}
