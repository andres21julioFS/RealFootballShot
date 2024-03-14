using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NextMove_Algorithm;
using Unity.Entities;
using FieldTriangleV2;
using Unity.Collections;
using CullPositionPoint;
public class CullPassPoints : MonoBehaviour
{
     [System.Serializable]
    public class CullPassPointsParams{
        public int entitySize=10;
        public int entityPointSize=10;
    }
    public CullPassPointsParams cullPassPointsParams;
    public SearchLonelyPointsManager SearchLonelyPointsManager;
    public string teamName = "Red";
    public bool test;
    List<Entity> entities=new List<Entity>();
    EntityManager entityManager;
    List<PublicPlayerData> players=new List<PublicPlayerData>();
    void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        CullPassPointsSystem cullPassPointsSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<CullPassPointsSystem>();
        cullPassPointsSystem.CullPassPoints = this;
        
        createEntities();
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(PlayerAddedToTeam);
    }
    void PlayerAddedToTeam(PlayerAddedToTeamEventArgs playerAddedToTeamEventArgs)
    {
        bool aux = false; ;
        foreach (var entity in entities)
        {
            CullPassPointsComponent CullPassPointsComponent = entityManager.GetComponentData<CullPassPointsComponent>(entity);
            DynamicBuffer<PlayerPositionElement> PlayerPositionElements = entityManager.GetBuffer<PlayerPositionElement>(entity);
            if (playerAddedToTeamEventArgs.TeamName.Equals("Red"))
            {
                PlayerPositionElements.Insert(CullPassPointsComponent.teamRedSize,new PlayerPositionElement(Vector2.zero));
                if(!aux)
                    players.Insert(CullPassPointsComponent.teamRedSize,playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamRedSize++;
            }
            else
            {
                PlayerPositionElements.Insert(CullPassPointsComponent.teamRedSize+ CullPassPointsComponent.teamBlueSize, new PlayerPositionElement(Vector2.zero));
                if (!aux)
                    players.Insert(CullPassPointsComponent.teamRedSize + CullPassPointsComponent.teamBlueSize, playerAddedToTeamEventArgs.publicPlayerData);
                CullPassPointsComponent.teamBlueSize++;
            }
            entityManager.SetComponentData<CullPassPointsComponent>(entity, CullPassPointsComponent);
            aux = true;
        }
    }
    
    void createEntities()
    {
        for (int i = 0; i < cullPassPointsParams.entitySize; i++)
        {
            EntityArchetype entityArchetype = entityManager.CreateArchetype(typeof(LonelyPointElement), typeof(CullPassPointsComponent), typeof(PlayerPositionElement));
            Entity entity = entityManager.CreateEntity(entityArchetype);
            DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(entity);
            for (int j = 0; j < cullPassPointsParams.entityPointSize; j++)
            {
                lonelyPointElements.Add(new LonelyPointElement());
            }
            entities.Add(entity);
        }
    }
    // Update is called once per frame
    public void PlacePoints()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.searchLonelyPointsEntitys[teamName];
        /*DynamicBuffer<EdgeElement> edges = entityManager.GetBuffer<EdgeElement>(searchLonelyPointsEntity);
        DynamicBuffer<TriangleElement> triangles = entityManager.GetBuffer<TriangleElement>(searchLonelyPointsEntity);
        DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);*/
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        //print(bufferSizeComponent.lonelyPointsResultSize);

        DynamicBuffer<LonelyPointElement> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[0]);
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {
            
            lonelyPointElements2[i % cullPassPointsParams.entityPointSize] = lonelyPointElements[i];
            if (i % cullPassPointsParams.entityPointSize >= cullPassPointsParams.entityPointSize - 1)
            {
                entityIndex++;
                if (entityIndex >= cullPassPointsParams.entitySize) break;
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[entityIndex]);
            }
        }
    }
    public void PlacePoints2()
    {
        Entity searchLonelyPointsEntity = SearchLonelyPointsManager.searchLonelyPointsEntitys[teamName];
        /*DynamicBuffer<EdgeElement> edges = entityManager.GetBuffer<EdgeElement>(searchLonelyPointsEntity);
        DynamicBuffer<TriangleElement> triangles = entityManager.GetBuffer<TriangleElement>(searchLonelyPointsEntity);
        DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);*/
        BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
        int entityIndex = 0;
        //print(bufferSizeComponent.lonelyPointsResultSize);

        DynamicBuffer<LonelyPointElement> lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[0]);
        lonelyPointElements2.Clear();
        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
        {
            
            lonelyPointElements2.Add(lonelyPointElements[i]);
            if (i % cullPassPointsParams.entityPointSize >= cullPassPointsParams.entityPointSize - 1)
            {
                entityIndex++;
                if (entityIndex >= cullPassPointsParams.entitySize) break;
                lonelyPointElements2 = entityManager.GetBuffer<LonelyPointElement>(entities[entityIndex]);
                lonelyPointElements2.Clear();
            }
        }
    }
    private void TestPlayers()
    {
        print("eooo");
        Team teamRed = Teams.getTeamByName("Red");
        Team teamBlue = Teams.getTeamByName("Blue");
        foreach (var publicPlayerData in teamRed.publicPlayerDatas)
        {
            print(publicPlayerData.playerID);
        }
        foreach (var publicPlayerData in teamBlue.publicPlayerDatas)
        {
            print(publicPlayerData.playerID);
        }
        print("aaaaa");
        foreach (var publicPlayerData in players)
        {
            print(publicPlayerData.playerID);
        }
    }
}
