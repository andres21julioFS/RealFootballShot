using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Burst;
using DOTS_ChaserDataCalculation;

[BurstCompile]
public class ParabolicPassDOTS
{
    public static void getPassResult(ref GetPerfectPassComponent GetPerfectPassComponent, ref DynamicBuffer<PlayerAttackElement> PlayerAttackElementBuffer, ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer,ref PlayerDataComponent receiverPlayerData, ref PlayerDataComponent kickerPlayerData, ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer, ref AreaPlaneElement defenseGoalAreaPlanesBuffer, ref AreaPlaneElement fullFieldAreaPlanesBuffer,ref PathComponent pathComponent,ref DynamicBuffer<SegmentedPathElement> SegmentedPathElements)
    {
        Vector3 firstPoint;
        float firstTime;
        bool somePartnerIsAhead;
        if (!getFirstPoint(ref pathComponent,ref GetPerfectPassComponent,ref PlayerAttackElementBuffer,ref PlayerDefenseElementBuffer,ref ChaserDataElementBuffer,ref defenseGoalAreaPlanesBuffer, out firstPoint, out firstTime, out somePartnerIsAhead,ref kickerPlayerData))
        {
            GetPerfectPassComponent.parabolicGetPassV0.result.v0 = GetPerfectPassComponent.straightGetPassV0.result.v0;
            GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude = GetPerfectPassComponent.straightGetPassV0.result.v0Magnitude;
            GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe = true;
            GetPerfectPassComponent.parabolicGetPassV0.result.foundedResult = true;
            GetPerfectPassComponent.parabolicGetPassV0.result.noPartnerIsAhead = !somePartnerIsAhead;
            return;
        }
        Vector3 v0 = getFirstMinVy(firstPoint, firstTime,ref GetPerfectPassComponent.parabolicGetPassV0);
        float v0Magnitude = Mathf.Clamp(v0.magnitude, 0, kickerPlayerData.maxKickForce);
        //GetPerfectPassJob.resetChaserDatas(ref ChaserDataElementBuffer);
        searchMinVy(ref pathComponent,ref GetPerfectPassComponent,ref receiverPlayerData,ref PlayerDefenseElementBuffer,ref ChaserDataElementBuffer,ref fullFieldAreaPlanesBuffer,v0.normalized * v0Magnitude,ref kickerPlayerData,ref SegmentedPathElements);
        GetPerfectPassComponent.parabolicGetPassV0.result.noPartnerIsAhead = !somePartnerIsAhead;
    }
    static bool getFirstPoint(ref PathComponent pathComponent,ref GetPerfectPassComponent GetPerfectPassComponent,ref DynamicBuffer<PlayerAttackElement> PlayerAttackElementBuffer,ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer,ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer, ref AreaPlaneElement defenseGoalAreaPlanesBuffer, out Vector3 point, out float time, out bool somePartnerIsAhead, ref PlayerDataComponent kickerPlayerData)
    {
        Vector3 pos0 = GetPerfectPassComponent.straightGetPassV0.Pos0;
        Vector3 posf = GetPerfectPassComponent.straightGetPassV0.Posf;
        GetPassV0Element parabolicGetPassV0 = GetPerfectPassComponent.parabolicGetPassV0;
        
        GetPassV0Element straightGetPassV0 = GetPerfectPassComponent.straightGetPassV0;
        float lastDistance = Mathf.Infinity;
        bool thereIsResult = false;
        float drag = GetPerfectPassComponent.straightGetPassV0.k;
        point = Vector3.zero;
        time = 0;
        somePartnerIsAhead = false;
        for (int i = 0; i < PlayerAttackElementBuffer.Length; i++)
        {
            PlayerDataComponent playerDataComponent = PlayerAttackElementBuffer[i].PlayerDataComponent;
            float d = MyFunctions.DistancePointAndFiniteLine(MyFunctions.setY0ToVector3(playerDataComponent.position), MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(posf));
            if (d < playerDataComponent.bodyBallRadio + 0.05f)
            {
                StraightXZDragPathDOTS.getXZV0(ref parabolicGetPassV0,ref pathComponent, GetPerfectPassComponent.straightGetPassV0.result.receiverReachTargetPositionTime, parabolicGetPassV0.Pos0, parabolicGetPassV0.Posf, kickerPlayerData.maxKickForce);
                float t = StraightXZDragPathDOTS.getT(parabolicGetPassV0.Pos0, playerDataComponent.position, straightGetPassV0.result.v0Magnitude, drag);
                if (t < GetPerfectPassComponent.partnerIsAheadMinTime)
                {
                    float height = playerDataComponent.height + GetPerfectPassComponent.parabolicGetPassV0.ballRadio;

                    time = t;
                    Vector3 p = MyFunctions.GetClosestPointOnFiniteLine(MyFunctions.setY0ToVector3(playerDataComponent.position), MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(posf));
                    point = p + Vector3.up * height;
                    lastDistance = Vector3.Distance(MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(p));
                    thereIsResult = true;
                    somePartnerIsAhead = true;
                }
            }
        }
        for (int i = 0; i < PlayerDefenseElementBuffer.Length; i++)
        {
            PlayerDataComponent playerDataComponent = PlayerDefenseElementBuffer[i].PlayerDataComponent;
            ChaserDataElement ChaserDataElement = ChaserDataElementBuffer[i];

            float d = Vector3.Distance(MyFunctions.setY0ToVector3(pos0), MyFunctions.setY0ToVector3(ChaserDataElement.OptimalPoint));
            if (ChaserDataElement.ReachTheTarget && d < lastDistance)
            {
                point = getJumpHeightPoint(ChaserDataElement.OptimalPoint, GetPerfectPassComponent.rivalsHeightOffset,ref playerDataComponent,ref defenseGoalAreaPlanesBuffer);
                time = ChaserDataElement.OptimalTime;
                lastDistance = d;
                thereIsResult = true;
            }
        }
        GetPerfectPassComponent.parabolicGetPassV0 = parabolicGetPassV0;
        return thereIsResult;
    }
    static Vector3 getJumpHeightPoint(Vector3 point, float rivalsHeightOffset,ref PlayerDataComponent playerDataComponent, ref AreaPlaneElement defenseGoalAreaPlanesBuffer)
    {
        float chaserHeight = playerDataComponent.normalMaximumJumpHeight;
        if (playerDataComponent.isGoalkeeper&& defenseGoalAreaPlanesBuffer.pointIsInside(point))
        {
            chaserHeight = playerDataComponent.goalkeeperMaximumJumpHeight;
        }
        float d = chaserHeight + rivalsHeightOffset - point.y;
        return point + Vector3.up * d;
    }
    public static Vector3 ParabolaWithDrag_GetV0(float t, Vector3 pos0, Vector3 posf,float k,float g)
    {
        Vector2 XZ0 = new Vector2(pos0.x, pos0.z);
        Vector2 XZf = new Vector2(posf.x, posf.z);
        Vector2 dir = XZf - XZ0;
        float distanceXZ = Vector3.Distance(XZ0, XZf);
        float e = (1 - Mathf.Exp(-k * t));
        Vector2 vxz0 = dir.normalized * (distanceXZ * k / e);

        float distanceY = posf.y - pos0.y;
        float vf = g / k;
        float vy0 = ((distanceY + vf * t) * k / e) - vf;
        return new Vector3(vxz0.x, vy0, vxz0.y);
    }
    static Vector3 getFirstMinVy(Vector3 firstPoint, float time, ref GetPassV0Element GetPassV0Element)
    {
        Vector3 v0 = ParabolaWithDrag_GetV0(time, GetPassV0Element.Pos0, firstPoint, GetPassV0Element.k, GetPassV0Element.g);
        return v0;
    }
    static void searchMinVy(ref PathComponent pathComponent, ref GetPerfectPassComponent GetPerfectPassComponent,ref PlayerDataComponent receiverPlayerDataComponent, ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer, ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer, ref AreaPlaneElement fullFieldAreaPlanesBuffer, Vector3 firstVy, ref PlayerDataComponent kickerPlayerData,ref DynamicBuffer<SegmentedPathElement> SegmentedPathElements)
    {
        float left, right;
        //left = firstVy.y;

        StraightXZDragPathDOTS.getXZV0(ref GetPerfectPassComponent.parabolicGetPassV0,ref pathComponent, GetPerfectPassComponent.parabolicGetPassV0.result.receiverReachTargetPositionTime, GetPerfectPassComponent.parabolicGetPassV0.Pos0, GetPerfectPassComponent.parabolicGetPassV0.Posf, kickerPlayerData.maxKickForce);
        
        float maxReceiverHeight = GetPerfectPassComponent.isReceiverHeadPass ? receiverPlayerDataComponent.normalMaximumJumpHeight : receiverPlayerDataComponent.maxReceiverHeight;
        Vector3 heightPoint = MyFunctions.setY0ToVector3(GetPerfectPassComponent.parabolicGetPassV0.Posf) + Vector3.up * maxReceiverHeight;
        float t = StraightXZDragPathDOTS.getT(GetPerfectPassComponent.parabolicGetPassV0.Pos0, GetPerfectPassComponent.parabolicGetPassV0.Posf, GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude, GetPerfectPassComponent.straightGetPassV0.k);
        float maxVy = ParabolaWithDrag_GetV0(t, GetPerfectPassComponent.parabolicGetPassV0.Pos0, heightPoint, GetPerfectPassComponent.parabolicGetPassV0.k, GetPerfectPassComponent.parabolicGetPassV0.g).y;

        right = maxVy;

        int attempts = 0;
        left = Mathf.Clamp(firstVy.y, 0, maxVy);
        float testVy = left;
        float increment = GetPerfectPassComponent.parabolicGetPassV0.searchVyIncrement;
        Vector3 testV0 = Vector3.zero;
        //NativeArray<bool> playersWithOptimalPoint2 = new NativeArray<bool>(PlayerDefenseElementBuffer.Length, Allocator.Temp);
        for (int i = 0; i < ChaserDataElementBuffer.Length; i++)
        {
            ChaserDataElement chaserDataElement = ChaserDataElementBuffer[i];
            chaserDataElement.CalculateOptimalPointAux = chaserDataElement.CalculateOptimalPoint;
            ChaserDataElementBuffer[i] = chaserDataElement;
        }
        while (left <= right && attempts <= GetPerfectPassComponent.parabolicGetPassV0.maxAttempts)
        {
            GetPerfectPassJob.resetChaserDatas2(ref ChaserDataElementBuffer);
            testV0 = MyFunctions.setY0ToVector3(GetPerfectPassComponent.parabolicGetPassV0.result.v0) + Vector3.up * testVy;
            calculateChaserDatas(ref pathComponent,ref PlayerDefenseElementBuffer,ref ChaserDataElementBuffer,ref fullFieldAreaPlanesBuffer, testV0,ref SegmentedPathElements);
            bool noRivalReachTheTargetBeforeMe = GetPerfectPassJob.checkIfNoRivalReachTheTargetBeforeMe(ref ChaserDataElementBuffer, GetPerfectPassComponent.parabolicGetPassV0.result.ballReachTargetPositionTime,ref GetPerfectPassComponent,ref GetPerfectPassComponent.parabolicGetPassV0.result);
            if (noRivalReachTheTargetBeforeMe)
            {
                GetPerfectPassComponent.parabolicGetPassV0.result.v0 = testV0;
                GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude = testV0.magnitude;
                GetPerfectPassComponent.parabolicGetPassV0.result.foundedResult = true;
                GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe = true;
                return;
            }
            left = testVy;
            testVy += increment;
            if (testVy > right)
            {
                break;
            }
            //Debug.Log("centralV0=" + centralV0 + "  | distance=" + distance + " |target=" + target + " | left ="+left + " |right="+ right);
            attempts++;
        }
        GetPerfectPassComponent.parabolicGetPassV0.result.v0 = testV0;
        GetPerfectPassComponent.parabolicGetPassV0.result.v0Magnitude = testV0.magnitude;
        GetPerfectPassComponent.parabolicGetPassV0.result.foundedResult = false;
        GetPerfectPassComponent.parabolicGetPassV0.result.noRivalReachTheTargetBeforeMe = false;
    }
    static void calculateChaserDatas(ref PathComponent pathComponent, ref DynamicBuffer<PlayerDefenseElement> PlayerDefenseElementBuffer, ref DynamicBuffer<ChaserDataElement> ChaserDataElementBuffer, ref AreaPlaneElement fullAreaPlanesBuffer, Vector3 v0,ref DynamicBuffer<SegmentedPathElement> SegmentedPathElements)
    {
        PathComponent pathComponent2 = pathComponent;
        pathComponent2.currentPath.V0 = v0;
        pathComponent2.currentPath.v0Magnitude = v0.magnitude;
        pathComponent2.currentPath.normalizedV0 = v0.normalized;
        pathComponent2.currentPath.t0 = 0;
        pathComponent2.currentPath.pathType = PathType.Parabolic;
        pathComponent2.currentPath.index = 0;
        
        GetPerfectPassJob.calculateChaserDatas(ref pathComponent2, ref PlayerDefenseElementBuffer, ref ChaserDataElementBuffer, ref fullAreaPlanesBuffer,ref SegmentedPathElements);
    }
}
