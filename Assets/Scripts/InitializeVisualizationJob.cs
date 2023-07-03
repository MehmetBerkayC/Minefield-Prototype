using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct InitializeVisualizationJob : IJobFor
{
    [WriteOnly, NativeDisableParallelForRestriction]
    public NativeArray<float3> positions, colors;

    public int rows, columns;
    public void Execute(int index)
    {
        float3 cellPosition = GetCellPosition(index);
        int blockOffset = index * GridVisualization.blocksPerCell;

        for (int bi = 0; bi < GridVisualization.blocksPerCell; bi++)
        {
            positions[blockOffset + bi] = cellPosition + GetBlockPosition(bi);
            colors[blockOffset + bi] = 0.5f;
        }
    }

    float3 GetBlockPosition(int index)
    {
        int r = index / GridVisualization.columnsPerCell;
        int c = index - r * GridVisualization.columnsPerCell;

        return float3(c, 0f, r);
    }

    float3 GetCellPosition(int index)
    {
        int row = index / columns;
        int column = index - row * columns;
        return float3(
            column - (columns - 1) * 0.5f,
            0f,
            row - (rows - 1) * 0.5f /*turning hexagonal ->*/ - (column & 1) * 0.5f + 0.25f
        )
            * float3(GridVisualization.columnsPerCell + 1,
            0f,
            GridVisualization.rowsPerCell + 1
        )
            - float3(GridVisualization.columnsPerCell / 2,
            0f,
            GridVisualization.rowsPerCell / 2
        );

        // turning hexagonal work like:
        // in dimension Z
        // check if the column is even or not
        // if even, LSB(least significant bit) will always be 0
        // then the outcome => -(0) * 0.5 + 0.25 -> +0.25 
        // if odd the outcome => -(1) * 0.5 + 0.25 -> -0.25 
    }
}
