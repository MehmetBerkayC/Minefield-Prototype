using UnityEngine;
using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Unity.Mathematics.math;
using static Game;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct RevealRegionJob : IJob
{
    public Grid grid;

    public int2 startRowColumn;
    
    int stackSize;

    public void Execute()
    {
        var stack = new NativeArray<int2>(grid.CellCount, Allocator.Temp);
        stackSize = 0;
        PushIfNeeded(stack, startRowColumn);
        while (stackSize > 0)
        {
            int2 rc = stack[--stackSize];
            PushIfNeeded(stack, rc - int2(1, 0));
            PushIfNeeded(stack, rc + int2(1, 0));
            PushIfNeeded(stack, rc - int2(0, 1));
            PushIfNeeded(stack, rc + int2(0, 1));

            rc.x += (rc.y & 1) == 0 ? 1 : -1;
            PushIfNeeded(stack, rc - int2(0, 1));
            PushIfNeeded(stack, rc + int2(0, 1));
        }
    }

    void PushIfNeeded(NativeArray<int2> stack, int2 rc) // rc - Row Column
    {
        if(grid.TryGetCellIndex(rc.x, rc.y, out int i))
        {
            CellState state = grid[i];
            if (state.IsNot(CellState.MarkedOrRevealed))
            {
                if(state == CellState.Zero)
                {
                    stack[stackSize++] = rc;
                }
                grid[i] = state.With(CellState.Revealed);
            }
        }
    }
}
