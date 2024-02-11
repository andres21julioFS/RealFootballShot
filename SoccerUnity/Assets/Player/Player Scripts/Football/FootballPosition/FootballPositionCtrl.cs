using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class FieldPositionsData
{
    public enum HorizontalPositionType
    {
        Right,Left
    }
    public enum PlayerPositionType
    {
        Forward,CenterMidfield, EdgeMidfield,CenterBack,LateralBack
    }
    [System.Serializable]
    public class Point
    {

        public bool enabled;
        public string info;
        public Vector2 point;
        public Vector2 value;
        public float radio;
        public bool useRadio=false;
        public bool snap;
        public Point(Vector2 point,Vector2 value)
        {
            this.point = point;
            enabled = true;
            this.value = value;
            info = ";";
        }
        public override string ToString()
        {
            return info;
        }
        public Point Clone()
        {
            Point clone = new Point(point,value);
            clone.enabled = enabled;
            clone.info = info;
            clone.radio = radio;
            clone.useRadio = useRadio;
            clone.snap = snap;
            return clone;
        }
    }
    public PlayerPositionType playerPositionType;
    public List<Point> points;
    [HideInInspector] public FieldPositionsData.Point selectedPoint;

    public FieldPositionsData Clone()
    {
        FieldPositionsData clone = new FieldPositionsData();
        clone.points = new List<Point>();
        foreach (var point in points)
        {
            clone.points.Add(point.Clone());
        }
        return clone;
    }
}
[System.Serializable]
public class PressureFieldPositionDatas
{
    public string name = "Default";
    public List<FieldPositionsData> FieldPositionDatas = new List<FieldPositionsData>();
}
[System.Serializable]
public class LineupFieldPositionDatas
{
    public string name = "Default";
    public List<PressureFieldPositionDatas> PressureFieldPositionDatasList = new List<PressureFieldPositionDatas>();
}
[System.Serializable]
public class LineupFieldPositionDatasList
{
    public List<LineupFieldPositionDatas> LineupFieldPositionDatas = new List<LineupFieldPositionDatas>();
}
[ExecuteInEditMode]
public class FootballPositionCtrl : MonoBehaviour
{
    public bool debug;
    public bool debugRadios,debugAllPositions;
    //Vector3 ballPosition { get => MatchComponents.ballPosition; }
    public SideOfField mySideOfField,rivalSideOfField;
    public SetupFootballField setupFootballField;
    public LineupFieldPositionDatasList LineupFieldPositionList;
    public float buttonSize = 0.1f;
    public float fieldLenght { get => MatchComponents.footballField.fieldLenght; }
    public float fieldWidth{ get => MatchComponents.footballField.fieldWidth; }
    public Vector2 newPointPosition = new Vector2(0.5f,0.5f);
    public FieldPositionsData.HorizontalPositionType horizontalPositionType;
    Vector2 normalizedPosition,normalizedBallPosition;
    float horizontalAdjustInfo;
    [HideInInspector]
    public Vector2 normailizedBallPosition;
    [HideInInspector]
    public FieldPositionsData selectedFieldPositionParameters;
    [HideInInspector]
    public FieldPositionsData.Point selectedPoint;
    [HideInInspector] public FieldPositionsData.PlayerPositionType playerPositionType;
    [HideInInspector] public string lineupName = "Default";
    [HideInInspector] public string pressureName = "Default";
    [HideInInspector] public int playerSize = 11;
    /* void Start()
    {
       string text = File.ReadAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json");
        FieldPositionParameters fieldPositionParameters = JsonUtility.FromJson<FieldPositionParameters>(text);
        FieldPositionList fieldPositionList = new FieldPositionList();
        fieldPositionList.FieldPositionParametersList.Add(fieldPositionParameters);
        LineupFieldPositionList.FieldPositionList.Add(fieldPositionList);
}*/
    public void Load()
    {
        mySideOfField.loadPlanes();
        rivalSideOfField.loadPlanes();
        List<SideOfField> sideOfFields = new List<SideOfField>();
        sideOfFields.Add(mySideOfField);
        sideOfFields.Add(rivalSideOfField);
        setupFootballField.loadFieldDimensions(sideOfFields);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos2()
    {
        if (Selection.activeGameObject != gameObject) return;

    }
#endif
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!debug || Selection.activeGameObject != gameObject) return;

        /*List<FootballPositionParameters.Point> filteredPoints = pointsFilter(normailizedBallPosition, footballPositionParameters.points);
        foreach (var filteredPoint in filteredPoints)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(getGlobalPosition(filteredPoint.point) + Vector3.up*0.5f, 0.25f);
        }*/
    }
#endif
    List<FieldPositionsData.Point> pointsFilter(Vector2 normalizedPosition, List<FieldPositionsData.Point> points)
    {
        List<FieldPositionsData.Point> filteredPoints = new List<FieldPositionsData.Point>();
        List<FieldPositionsData.Point> removedPoints = new List<FieldPositionsData.Point>();
        Vector3 position = getGlobalPosition(horizontalPositionType,normalizedPosition);
        foreach (var point1 in points)
        {
            Vector3 point1GlobalPosition = getGlobalPosition(horizontalPositionType, point1.point);
            bool addPoint = true;
            List<FieldPositionsData.Point> removePoints = new List<FieldPositionsData.Point>();
            foreach (var point2 in filteredPoints)
            {
                Vector3 point2GlobalPosition = getGlobalPosition(horizontalPositionType, point2.point);
                Vector3 dir1 = point2GlobalPosition-point1GlobalPosition;
                Plane plane1 = new Plane(dir1, point1GlobalPosition);
                if (!plane1.GetSide(position))
                {
                    removePoints.Add(point2);
                }
                Vector3 dir2 = point1GlobalPosition-point2GlobalPosition;
                Plane plane2 = new Plane(dir2, point2GlobalPosition);
                if (!plane2.GetSide(position))
                {
                    addPoint = false;
                    break;
                }
            }
            filteredPoints.RemoveAll(x=>removePoints.Contains(x));
            if (addPoint)
            {
                filteredPoints.Add(point1);
            }

        }
        return filteredPoints;
    }
    void getWeightyValue(Vector2 normalizedPosition, List<FieldPositionsData.Point> points, out Vector2 value)
    {
        //Vector2 normalizedPosition = getNormalizedPosition(position);
        Vector2 a=Vector2.zero;
        float b = 0;
        foreach (var point in points)
        {
            float d = Vector2.Distance(point.point, normalizedPosition);
            //a += point.value / d;
            b += 1 / d;
        }
        value = a / b;
    }
    Vector2 getNormalPoint2(Vector2 p)
    {
        p.y = p.y * fieldLenght / fieldWidth;
        return p;
    }
    public void getWeightyValue4(Vector2 normalizedPosition, List<FieldPositionsData.Point> points,out Vector2 value)
    {
        float totalH = 0;
        float[] hs = new float[points.Count];

        float[] weights = new float[points.Count];
        Vector2 p = getNormalPoint2(normalizedPosition);
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].enabled) continue;
            Vector2 dir = p - getNormalPoint2(points[i].point);
            //dir.y *= fieldWidth / fieldLenght;
            dir = dir.normalized * Mathf.Clamp(points[i].radio, 0, dir.magnitude);
            //Vector2 pi = points[i].point + dir;
            Vector2 pi = getNormalPoint2(points[i].point);
            if (points[i].useRadio)
            {
                pi += dir;
            }
            

            hs[i] = Mathf.Infinity;
            for (int j = 0; j < points.Count; j++)
            {
                if (i == j) continue;
                if (!points[j].enabled) continue;
                Vector2 dir2 = p - getNormalPoint2(points[j].point);
                //dir2.y *= fieldWidth / fieldLenght;
                dir2 = dir2.normalized * Mathf.Clamp(points[j].radio, 0, dir2.magnitude);
                Vector2 pj = getNormalPoint2(points[j].point);
                if (points[j].useRadio)
                {
                    pj += dir2;
                }
                float p1 = Vector2.Dot(p - pi, pj - pi);
                float p2 = Vector2.Distance(pi, pj);
                float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)));
                if (h_2 < hs[i]) hs[i] = h_2;

            }
            totalH += hs[i];
        }
        for (int i = 0; i < weights.Length; i++)
        {
            weights[i] = hs[i] / totalH;
        }
        value = getValue(weights, points, normalizedPosition);
        //value = Vector2.zero;
        return;
    }
   
    Vector2 getValue(float[] weights, List<FieldPositionsData.Point> points,Vector2 p)
    {
        Vector2 result = Vector2.zero;
        float totalweight = 0;
        for (int i = 0; i < points.Count; i++)
        {
            if (points[i].snap)
            {
            result += p * weights[i];
            }
            else
            {
            result += points[i].value * weights[i];

            }
            totalweight += weights[i];
            
            /*GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            Handles.Label(getGlobalPosition(points[i].point)+Vector3.up*2, "weight= " + weights[i].ToString("f3"), style);*/
        }
        return result;
    }
    public Vector2 getNormalizedPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector3 position)
    {
        float verticalBallDistance = mySideOfField.backPlane.GetDistanceToPoint(position)/fieldLenght;
        verticalBallDistance = Mathf.Clamp01(verticalBallDistance);
        float horizontalBallDistance;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right)){

            horizontalBallDistance = mySideOfField.rightPlane.GetDistanceToPoint(position)/fieldWidth;
        }
        else
        {
            horizontalBallDistance = mySideOfField.leftPlane.GetDistanceToPoint(position) / fieldWidth;
        }
        horizontalBallDistance = Mathf.Clamp01(horizontalBallDistance);

        normalizedBallPosition = new Vector2(horizontalBallDistance, verticalBallDistance);
        return normalizedBallPosition;
    }
    public Vector3 getGlobalPosition(FieldPositionsData.HorizontalPositionType horizontalPositionType, Vector2 normalizedPosition)
    {
        Vector3 globalPosition= mySideOfField.backTransform.TransformPoint(Vector3.forward*normalizedPosition.y*fieldLenght);
        Vector3 globalHorizontalPosition;
        if (horizontalPositionType.Equals(FieldPositionsData.HorizontalPositionType.Right))
        {
            globalHorizontalPosition = mySideOfField.rightTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth/2));

            //Debug.DrawRay(mySideOfField.rightTransform.position, globalHorizontalPosition, Color.blue);
        }
        else
        {
            globalHorizontalPosition = mySideOfField.leftTransform.TransformDirection(Vector3.forward * normalizedPosition.x * fieldWidth - Vector3.forward * (fieldWidth / 2));
            //Debug.DrawRay(mySideOfField.leftTransform.position, globalHorizontalPosition, Color.blue);
        }
        globalPosition += globalHorizontalPosition;
        //Debug.DrawLine(mySideOfField.backTransform.position, globalPosition,Color.red);
        return globalPosition;
    }
    public bool GetSelectedFieldPositionParameters(out FieldPositionsData fieldPositionDatas)
    {
        //t.selectedFieldPositionParameters = null;
        fieldPositionDatas = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        if (LineupFieldPositionDatas == null) return false;
        
        PressureFieldPositionDatas fieldPositionList = LineupFieldPositionDatas.PressureFieldPositionDatasList.Find(x => x.name.Equals(pressureName));
        if (fieldPositionList == null) return false;
        fieldPositionDatas = fieldPositionList.FieldPositionDatas.Find(x => x.playerPositionType.Equals(playerPositionType));

        return fieldPositionDatas != null;
    }
    public bool getCurrentPressureFieldPositions(out PressureFieldPositionDatas lineupFieldPositionData)
    {
        lineupFieldPositionData = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        if (LineupFieldPositionDatas == null) return false;
        lineupFieldPositionData = LineupFieldPositionDatas.PressureFieldPositionDatasList.Find(x => x.name.Equals(lineupName));
        if (lineupFieldPositionData == null) return false;
        return true;
    }
    public bool getCurrentLineup(out LineupFieldPositionDatas lineupFieldPositionData)
    {
        lineupFieldPositionData = null;
        LineupFieldPositionDatas LineupFieldPositionDatas = LineupFieldPositionList.LineupFieldPositionDatas.Find(x => x.name.Equals(lineupName));
        return LineupFieldPositionDatas != null;
    }
    /*
   public void getWeightyValue3(Vector2 p, List<FieldPositionParameters.Point> points, out Vector2 value)
   {
       float totalH = 0;
       float[] hs = new float[points.Count];

       float[] weights = new float[points.Count];
       for (int i = 0; i < points.Count; i++)
       {
           if (!points[i].enabled) continue;
           Vector2 pi = points[i].point;
           hs[i] = Mathf.Infinity;
           for (int j = 0; j < points.Count; j++)
           {
               if (i == j) continue;
               Vector2 pj = points[j].point;
               float p1 = Vector2.Dot(p-pi, pj-pi);
               float p2 = Vector2.Distance(pi,pj);
               float h_2 = Mathf.Clamp01(1 - (p1 / (p2 * p2)));
               if (h_2 < hs[i]) hs[i] = h_2;

           }
           totalH += hs[i];
       }
       for (int i = 0; i < weights.Length; i++)
       {
           weights[i] = hs[i] / totalH;
       }
       value = getValue(weights, points,p);
       //value = Vector2.zero;
       return;
   }
   void getWeightyValue2(Vector2 normalizedPosition, List<FieldPositionParameters.Point> points, out Vector2 value)
   {
       float total_sqrd_distance = 0;
       float total_angular_distance = 0;
       float[] weights = new float[points.Count];
       float[] sqrd_distances = new float[points.Count];
       float[] angular_distances = new float[points.Count];
       int i = 0;
       foreach (var point in points)
       {
           float sqr_distance = (normalizedPosition - point.point).sqrMagnitude;
           if (sqr_distance > 0)
           {
               //float angular_distance = -(Mathf.Clamp(Vector2.Dot(normalizedPosition.normalized, point.point.normalized), -1, 1)-1) * 0.5f;
               float angular_distance = -(Mathf.Clamp(Vector2.Dot(normalizedPosition.normalized, point.point.normalized), -1, 1) - 1) * 0.5f;
               //float angular_distance = Vector2.Angle(normalizedPosition, point.point);
               total_sqrd_distance += 1 / sqr_distance;
               if (angular_distance > 0) total_angular_distance += 1 / angular_distance;
               sqrd_distances[i]=sqr_distance;
               angular_distances[i] = angular_distance;
           }
           else
           {
               weights[i] = 1;
               value = getValue(weights, points, normalizedPosition);
               return;
           }
           i++;
       }
       for (int j = 0; j < points.Count; j++)
       {
           float sqrd_distance = total_sqrd_distance * sqrd_distances[j];
           float angular_distance = total_angular_distance * angular_distances[j];
           if(sqrd_distance>0 && angular_distance > 0)
           {
               //weights[j] = (1 / sqrd_distance) * 0.5f + (1 / angular_distance) * 0.5f;
               weights[j] = (1 / sqrd_distance) * 0.5f + (1 / angular_distance) * 0.5f;
           }else if (sqrd_distance > 0)
           {
               //weights[j] = (1 / sqrd_distance) * 0.5f + 0.5f;
               weights[j] = (1 / sqrd_distance) * 0.5f + 0.5f;
           }
           else
           {
               weights[j] = 0;
           }
       }
       value = getValue(weights, points, normalizedPosition);
       return;
   }*/
}
