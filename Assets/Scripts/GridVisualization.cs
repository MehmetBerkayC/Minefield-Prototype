using UnityEngine;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public struct GridVisualization {
    static int
        positionsID = Shader.PropertyToID("_Positions"),
        colorsID = Shader.PropertyToID("_Colors");

    ComputeBuffer positionsBuffer, colorsBuffer;

    NativeArray<float3> positions, colors, ripples;

    Grid grid;

    Material material;

    Mesh mesh;

    int rippleCount;

    public const int
        rowsPerCell = 7,
        columnsPerCell = 5,
        blocksPerCell = rowsPerCell * columnsPerCell;

    public void Initialize(Grid grid, Material material, Mesh mesh)
    {
        this.grid = grid;
        this.material = material;
        this.mesh = mesh;

        int instanceCount = grid.CellCount * blocksPerCell;
        positions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
        colors = new NativeArray<float3>(instanceCount, Allocator.Persistent);
        ripples = new NativeArray<float3>(10, Allocator.Persistent);
        rippleCount = 0;

        positionsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
        colorsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
        material.SetBuffer(positionsID, positionsBuffer);
        material.SetBuffer(colorsID, colorsBuffer);

        new InitializeVisualizationJob
        {
            positions = positions,
            colors = colors,
            rows = grid.Rows,
            columns = grid.Columns
        }.ScheduleParallel(grid.CellCount,
                           grid.Columns,
                           default).Complete();
        positionsBuffer.SetData(positions);
        colorsBuffer.SetData(colors);
    }

    public bool TryGetHitCellIndex(Ray ray, out int cellIndex)
    {
        Vector3 p = ray.origin - ray.direction * (ray.origin.y / ray.direction.y);

        float x = p.x + columnsPerCell / 2 + 1.5f;
        x /= columnsPerCell + 1;
        x += (grid.Columns - 1) * 0.5f;
        int c = Mathf.FloorToInt(x);

        float z = p.z + rowsPerCell / 2f + 1.5f;
        z /= rowsPerCell + 1;
        z += (grid.Rows - 1) * 0.5f + (c & 1) * 0.5f - 0.25f;
        int r = Mathf.FloorToInt(z);

        bool valid = grid.TryGetCellIndex(r, c, out cellIndex) &&
                x - c > 1f / (columnsPerCell + 1) &&
                z - r > 1f / (rowsPerCell + 1);

        if(valid && rippleCount < ripples.Length)
        {
            ripples[rippleCount++] = new float3(p.x, p.z, 0f);
        }
        return valid;
    }

    public void Update()
    {
        float dt = Time.deltaTime;
        for (int i = 0; i < rippleCount; i++)
        {
            float3 ripple = ripples[i];
            if (ripple.z < 1f)
            {
                ripple.z = Mathf.Min(ripple.z + dt, 1f);
                ripples[i] = ripple;
            }
            else
            {
                ripples[i] = ripples[--rippleCount];
                i -= 1;
            }
        }

        new UpdateVisualizationJob
        {
            positions = positions,
            colors = colors,
            ripples = ripples,
            rippleCount = rippleCount,
            grid = grid
        }.ScheduleParallel(grid.CellCount, grid.Columns, default).Complete();
        positionsBuffer.SetData(positions);
        colorsBuffer.SetData(colors);
    }

    public void Dispose()
    {
        positions.Dispose();
        colors.Dispose();
        ripples.Dispose();
        positionsBuffer.Release();
        colorsBuffer.Release();
    }

    public void Draw()
    {
        if (rippleCount > 0)
        {
            Update();
        }
        Graphics.DrawMeshInstancedProcedural(
        mesh, 0, material, new Bounds(Vector3.zero, Vector3.one), positionsBuffer.count);
    }

}
