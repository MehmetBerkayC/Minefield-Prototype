using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

using static Unity.Mathematics.math;

public struct GridVisualization {
    static int
        positionsID = Shader.PropertyToID("_Positions"),
        colorsID = Shader.PropertyToID("_Colors");

    ComputeBuffer positionsBuffer, colorsBuffer;

    NativeArray<float3> positions, colors;

    Grid grid;

    Material material;

    Mesh mesh;

    public void Initialize(Grid grid, Material material, Mesh mesh)
    {
        this.grid = grid;
        this.material = material;
        this.mesh = mesh;

        int instanceCount = grid.CellCount;
        positions = new NativeArray<float3>(instanceCount, Allocator.Persistent);
        colors = new NativeArray<float3>(instanceCount, Allocator.Persistent);

        positionsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
        colorsBuffer = new ComputeBuffer(instanceCount, 3 * 4);
        material.SetBuffer(positionsID, positionsBuffer);
        material.SetBuffer(colorsID, colorsBuffer);
    }

    public void Dispose()
    {
        positions.Dispose();
        colors.Dispose();
        positionsBuffer.Release();
        colorsBuffer.Release();
    }

    public void Draw() => Graphics.DrawMeshInstancedProcedural(
        mesh, 0, material, new Bounds(Vector3.zero, Vector3.one), positionsBuffer.count);


}
