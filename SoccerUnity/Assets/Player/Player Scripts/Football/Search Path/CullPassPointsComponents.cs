using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
namespace CullPositionPoint
{
    public struct CullPassPointsComponent : IComponentData
    {
        public int teamRedSize, teamBlueSize;
    }
    public struct PlayerPositionElement : IBufferElementData
    {
        public Vector2 position;

        public PlayerPositionElement(Vector2 position)
        {
            this.position = position;
        }
    }
}
