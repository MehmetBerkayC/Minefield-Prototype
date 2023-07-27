using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;
using static Game;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct PlaceMinesJob : IJob
{
    public Grid grid;

    public int mines, seed;

    public void Execute()
    {
        grid.RevealedCellCount = 0;
        int candidateCount = grid.CellCount;
        var candidates = 
            new NativeArray<int>(candidateCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);

        for (int i = 0; i < grid.CellCount; i++)
        {
            grid[i] = CellState.Zero;
            candidates[i] = i;
        }

        Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)seed);

        for (int m = 0; m < mines; m++)
        {
            int candidateIndex = random.NextInt(candidateCount--);
            SetMine(candidates[candidateIndex]);
            candidates[candidateIndex] = candidates[candidateCount];
        }
    }

    void SetMine(int mineIndex)
    {
        grid[mineIndex] = grid[mineIndex].With(CellState.Mine);
        grid.GetRowColumn(mineIndex, out int r, out int c);
        Increment(r - 1, c);
        Increment(r + 1, c);
        Increment(r, c - 1);
        Increment(r, c + 1);

        int rowOffset = (c & 1) == 0 ? 1 : -1;
        Increment(r + rowOffset, c - 1);
        Increment(r + rowOffset, c + 1);
    }

    void Increment(int r, int c)
    {
        if(grid.TryGetCellIndex(r, c, out int i))
        {
            grid[i] += 1;
        }
    }
}
