using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;
using Unity.Mathematics;
namespace DOTS_ChaserDataCalculation
{
    [BurstCompile]
    public class GetTimeToReachPointDOTS
    {
        public static float getTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            if (playerDataComponent.useAccelerationGetTimeToReachPosition)
            {
                return accelerationGetTimeToReachPosition(ref playerDataComponent, targetPosition);
            }
            else
            {
                return linearGetTimeToReachPosition(ref playerDataComponent, targetPosition);
            }
        }
        public static float linearGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            float distance = Vector3.Distance(MyFunctions.setYToVector3(playerDataComponent.position, targetPosition.y), targetPosition);
            float t = distance / playerDataComponent.maxSpeed;
            return t;
        }
        public static float accelerationGetTimeToReachPosition(ref PlayerDataComponent playerDataComponent, Vector3 targetPosition)
        {
            //bodyRotation = Quaternion.LookRotation(MyFunctions.setY0ToVector3(ballPosition - bodyPosition));
            if (MyFunctions.Vector3IsNan(targetPosition) || targetPosition.Equals(Vector3.positiveInfinity) || targetPosition.Equals(Vector3.negativeInfinity))
            {
                return Mathf.Infinity;
            }
            
            Vector3 bodyPosition = playerDataComponent.position;
            float d4 = Vector3.Distance(bodyPosition, MyFunctions.setYToVector3(targetPosition, bodyPosition.y));
            if (d4 < playerDataComponent.scope)
            {
                return 0;
            }
            float speed = playerDataComponent.currentSpeed;
            float minSpeedForRotate = playerDataComponent.minSpeedForRotate;
            float a = playerDataComponent.acceleration;
            float da = playerDataComponent.decceleration;
            float maxAngleForRun = playerDataComponent.maxAngleForRun;
            float t1_1 = speed > minSpeedForRotate ? AccelerationPath.getT(minSpeedForRotate, speed, da) : 0;
            Vector3 direction = MyFunctions.setY0ToVector3(targetPosition - bodyPosition).normalized;
            float angle = Vector3.Angle(playerDataComponent.bodyY0Forward, direction);
            float t1 = angle > maxAngleForRun ? t1_1 : 0;
            Vector3 x1 = AccelerationPath.getX(bodyPosition, playerDataComponent.bodyY0Forward, playerDataComponent.normalizedBodyY0Forward * speed, t1, -da);
            float t2 = angle > maxAngleForRun ? Path.getT(maxAngleForRun, angle, playerDataComponent.maxSpeedRotation) : 0;
            //float t3 = angleBodyForwardDesiredVelocity > 45 ? getT(MaxForwardFullSpeed, 0, getAccelerationSkill()) : getT(MaxForwardFullSpeed, bodySpeed, getAccelerationSkill());
            Vector3 v0 = angle > maxAngleForRun ? Vector3.zero : playerDataComponent.normalizedForwardVelocity * speed;
            float v0Magnitude= v0.magnitude;
            //Vector3 x2 =  getX(x1 , ballPosition, v0, t3, getAccelerationSkill());
            float d2 = Vector3.Distance(MyFunctions.setY0ToVector3(x1), MyFunctions.setY0ToVector3(targetPosition));
            
            float d3 = d2 - playerDataComponent.scope;
            float d = AccelerationPath.getDistanceWhereStartDecelerate(v0Magnitude, playerDataComponent.maxSpeedForReachBall, a, -da, d3);
            //Vector3 direction2 = MyFunctions.setY0ToVector3(targetPosition - x1).normalized;
            float t3, t4, t8, result;
            //Vector3 x2 = x1 + direction2 * d;
            float x3 = Mathf.Abs(AccelerationPath.getX2(v0Magnitude, playerDataComponent.maxSpeed, a));
            if (x3 < d)
            {
                float t5 = AccelerationPath.getT(playerDataComponent.maxSpeed, v0Magnitude, a);
                float x4 = Mathf.Abs(AccelerationPath.getX2(playerDataComponent.maxSpeed, playerDataComponent.maxSpeedForReachBall, da));
                float x5 = d3 - x3 - x4;
                float t6 = x5 / playerDataComponent.maxSpeed;
                float t7 = AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, playerDataComponent.maxSpeed, da);
                t8 = t5 + t6 + t7;
                result = t1 + t2 + t8;
                //string debug = "A name=" + publicPlayerData.name + " | t1=" + t1 + " | t2=" + t2 + " | t5=" + (t5 + t1 + t2) + " | t6=" + (t6 + t5 + t1 + t2) + " | t7=" + (t7 + t6 + t5 + t1 + t2) + " | t8=" + (t8 + t1 + t2) + " | d=" + d + " | dStop=" + x4 + " | d3=" + d3 + " | x1=" + x1 + " | d2=" + d2 + " | v0=" + v0.magnitude + " | angle=" + angle + " | speed=" + speed + " | result=" + result;
                //DebugsList.testing.print(debug,Color.white);

                //print(debug);
            }
            else
            {
                //getT(x2, x1, v0, getAccelerationSkill(), out t3);
                //d = 3.007097f;
                AccelerationPath.getT(d, v0Magnitude, a, out t3);
                float v1 = v0Magnitude + a* t3;
                t4 = AccelerationPath.getT(playerDataComponent.maxSpeedForReachBall, v1, da);
                t8 = t3 + t4;
                //print((x5 + x4)+" "+d3);
                result = t1 + t2 + t8;
                //string debug = "B name=" + publicPlayerData.name + " | t1=" + t1 + " | t2=" + t2 + " | t3=" + t3 + " | t4=" + t4 + " | t8=" + t8 + " | d=" + d + " | d3=" + d3 + " | dStop=" + x4 + " | dStop2=" + (d3 - d) + " | v0=" + v0.magnitude + " | x1=" + x1 + " | v1=" + v1 + " | angle=" + angle + " | speed=" + speed + " | result=" + result;
                //print(debug);
                //DebugsList.testing.print(debug, Color.white);

            }
            //float t4 = Vector3.Distance(MyFunctions.setY0ToVector3(x2), MyFunctions.setY0ToVector3(ballPosition)) / MaxForwardFullSpeed;
            return result;
        }
    }
}
