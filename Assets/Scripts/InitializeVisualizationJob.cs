using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;

using static Unity.Mathematics.math;

[BurstCompile(FloatPrecision.Standard, FloatMode.Fast)]
struct InitializeVisualizationJob : IJobFor
{
    [WriteOnly]
    public NativeArray<float3> positions, colors;

    public int rows, columns;
    public void Execute(int index)
    {
        positions[index] = GetCellPosition(index);
        colors[index] = 0.5f;
    }

    float3 GetCellPosition(int index)
    {
        int row = index / columns;
        int column = index - row * columns;
        return float3(
            column - (columns - 1) * 0.5f,
            0f,
            row - (rows - 1) * 0.5f /*turning hexagonal ->*/ -(column & 1) * 0.5f + 0.25f
        );
        // turning hexagonal work like:
        // in dimension Z
        // check if the column is even or not
        // if even, LSB(least significant bit) will always be 0
        // then the outcome => -(0) * 0.5 + 0.25 -> +0.25 
        // if odd the outcome => -(1) * 0.5 + 0.25 -> -0.25 
    }
}
