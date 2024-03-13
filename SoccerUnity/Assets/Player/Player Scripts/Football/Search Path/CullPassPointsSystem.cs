using FieldTriangleV2;
using NextMove_Algorithm;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

[UpdateAfter(typeof(NextMoveSystem))]
public class CullPassPointsSystem : SystemBase
{
    EntityQuery cullPassPointsQuery;
    public CullPassPoints CullPassPoints;
    protected override void OnCreate()
    {
        var description1 = new EntityQueryDesc()
        {
            All = new ComponentType[]{typeof(LonelyPointElement), typeof(CullPassPointsComponent) }
        };
        cullPassPointsQuery = this.GetEntityQuery(description1);
    }
    protected override void OnUpdate()
    {
        if (!CullPassPoints.test)
        {
            CullPassPoints.PlacePoints();
        }
        else
        {
            CullPassPoints.PlacePoints2();
        }
        var CullPassPointsJob = new CullPassPointsJob();

        CullPassPointsJob.lonelyPointsHandle = this.GetBufferTypeHandle<LonelyPointElement>(true);
        CullPassPointsJob.cullPassPointsParamsHandle = this.GetComponentTypeHandle<CullPassPointsComponent>(true);
        Dependency = CullPassPointsJob.ScheduleParallel(cullPassPointsQuery, 1, this.Dependency);
        Dependency.Complete();
    }
}
