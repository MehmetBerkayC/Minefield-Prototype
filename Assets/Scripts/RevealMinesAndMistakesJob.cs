using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Collections;

using static Game;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct RevealMinesAndMistakesJob : IJobFor
{
    public Grid grid;

    public void Execute(int index)
    {
        grid[index] = grid[index].With(grid[index].Is(CellState.MarkedSureOrMine) ? CellState.Revealed : CellState.Zero);
    }
}
