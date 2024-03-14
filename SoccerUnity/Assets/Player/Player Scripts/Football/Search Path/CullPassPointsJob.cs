using FieldTriangleV2;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using CullPositionPoint;
[BurstCompile]
public struct CullPassPointsJob : IJobEntityBatch
{

    [ReadOnly] public BufferTypeHandle<LonelyPointElement> lonelyPointsHandle;
    [ReadOnly] public BufferTypeHandle<PlayerPositionElement> playerPositionElementHandle;
    [ReadOnly] public ComponentTypeHandle<CullPassPointsComponent> cullPassPointsParamsHandle;
    public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
    {

        BufferAccessor<LonelyPointElement> lonelyPointsBuffer = batchInChunk.GetBufferAccessor(lonelyPointsHandle);
        BufferAccessor<PlayerPositionElement> playerPositionElementBuffer = batchInChunk.GetBufferAccessor(playerPositionElementHandle);
        NativeArray<CullPassPointsComponent> cullPassPointsParamsBuffer= batchInChunk.GetNativeArray(cullPassPointsParamsHandle);

        for (int i = 0; i < lonelyPointsBuffer.Length; i++)
        {
            DynamicBuffer<LonelyPointElement> lonelyPoints = lonelyPointsBuffer[i];
            CullPassPointsComponent CullPassPointsParams = cullPassPointsParamsBuffer[i];
            //Debug.Log(lonelyPoints.Length);
        }
    }
}