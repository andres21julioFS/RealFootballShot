using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;

[CustomEditor(typeof(FootballPositionCtrl))]
public class FootballPositionCtrlEditor : Editor
{
    [System.Serializable]
    public class FieldPointList
    {
        public List<FieldPositionsData.Point> list;
    }
    
    private void OnEnable()
    {
        var t = (target as FootballPositionCtrl);
        t.Load();
    }
   
    public override void OnInspectorGUI()
    {

        var t = (target as FootballPositionCtrl);
        base.DrawDefaultInspector();
        if (!t.debug) return;

        serializedObject.Update();
        t.playerPositionType = (FieldPositionsData.PlayerPositionType)EditorGUILayout.EnumPopup("PlayerPosition", t.playerPositionType);
        t.lineupName = EditorGUILayout.TextField("Lineup Name", t.lineupName);
        t.pressureName = EditorGUILayout.TextField("Lineup Name", t.pressureName);
        t.playerSize = EditorGUILayout.IntField("Lineup Name", t.playerSize);
        LineupFieldPositionDatas lineupFieldPositionDatas;

        if(!t.getCurrentLineup(out lineupFieldPositionDatas)) return;

        PressureFieldPositionDatas pressureFieldPositionDatas;
        if (!t.getCurrentPressureFieldPositions(out pressureFieldPositionDatas)) return;

        FieldPositionsData fieldPositionParameters;
        if (!t.GetSelectedFieldPositionParameters(out fieldPositionParameters)) return;
        
        if (GUILayout.Button("Duplicate Player Points"))
        {
            PressureFieldPositionDatas currentFieldPositionList;
            if (t.getCurrentPressureFieldPositions(out currentFieldPositionList))
            {
                FieldPositionsData cloneFieldPositionParameters = fieldPositionParameters.Clone();
                currentFieldPositionList.FieldPositionDatas.Add(cloneFieldPositionParameters);
            }
        }
        EditorGUILayout.Space(10.0f);
        EditorGUILayout.LabelField("Selected Point", EditorStyles.boldLabel);
        //FieldPositionParameters.Point selectedPoint = fieldPositionParameters.selectedPoint;
        if (selectedPoint != null)
        {
            if (fieldPositionParameters.points.Contains(selectedPoint))
            {
                selectedPoint.point = EditorGUILayout.Vector2Field("Point", selectedPoint.point);
                selectedPoint.value = EditorGUILayout.Vector2Field("Value", selectedPoint.value);
                selectedPoint.enabled = EditorGUILayout.Toggle("enabled", selectedPoint.enabled);
                selectedPoint.useRadio = EditorGUILayout.Toggle("use radio", selectedPoint.useRadio);
                selectedPoint.snap = EditorGUILayout.Toggle("snap", selectedPoint.snap);
            }
        }
        else
        {
            if (fieldPositionParameters.points.Count > 0)
            {
                selectedPoint = fieldPositionParameters.points[0];
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Add Point"))
        {
            float a = 0.05f;
            Vector2 random1 = new Vector2(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a));
            for (int i = 0; i < t.playerSize; i++)
            {
                Vector2 random2 = new Vector2(UnityEngine.Random.Range(-a, a), UnityEngine.Random.Range(-a, a));

                FieldPositionsData.Point newPoint = new FieldPositionsData.Point(t.newPointPosition + random1, t.newPointPosition + random2);
                pressureFieldPositionDatas.FieldPositionDatas[i].points.Add(newPoint);
                selectedPoint = newPoint;
            }
            
            
            SceneView.RepaintAll();
        }
        if (GUILayout.Button("Remove Point"))
        {
            if(selectedPoint!=null && fieldPositionParameters.points.Contains(selectedPoint))
            {
                fieldPositionParameters.points.Remove(selectedPoint);
                selectedPoint = null;
                SceneView.RepaintAll();
            }
        }
        if (GUILayout.Button("Save"))
        {
            string jsonString = JsonUtility.ToJson(t.LineupFieldPositionList);
            File.WriteAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json", jsonString);
        }
        if (GUILayout.Button("Load"))
        {
            string text = File.ReadAllText(Application.dataPath + "/Player/Player Scripts/Football/FootballPosition/FieldPoints.json");
            LineupFieldPositionDatasList LineupFieldPositionDatasList = JsonUtility.FromJson<LineupFieldPositionDatasList>(text);
            t.LineupFieldPositionList= LineupFieldPositionDatasList;
        }
        GUILayout.EndHorizontal();
        
        //fieldPositionParameters.selectedPoint = selectedPoint;
        //t.selectedFieldPositionParameters = fieldPositionParameters;
        serializedObject.ApplyModifiedProperties();
    }

    FieldPositionsData.Point selectedPoint;
    FieldPositionsData.Point selectedValue;
    int selectedPointIndex;
    public void OnSceneGUI()
    {
        var t = (target as FootballPositionCtrl);
        if (!t.debug) return;

        PressureFieldPositionDatas PressureFieldPositionDatas;
        if (!t.getCurrentPressureFieldPositions(out PressureFieldPositionDatas)) return;

        FieldPositionsData fieldPositionParameters;
        if (!t.GetSelectedFieldPositionParameters(out fieldPositionParameters)) return;
       
        //FieldPositionParameters.Point selectedPoint = fieldPositionParameters.selectedPoint;
        float buttonSize = t.buttonSize;
        int i = 0;
        float a = 0.25f;
        GUIStyle style2 = new GUIStyle();
        foreach (var item in fieldPositionParameters.points)
        {
            Color color = Color.red;
            if (!item.enabled) color.a = a;
            Handles.color = color;
            if (Handles.Button(t.getGlobalPosition(t.horizontalPositionType, item.point), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
            {
                selectedPoint = item;
            }
            color = Color.blue;
            if (!item.enabled) color.a = a;
           
            
            
            style2.fontSize = 14;
            color= Color.yellow;
            if (!item.enabled) color.a = a;
            style2.normal.textColor = color;
            item.info = "Point " + i;
            
            Handles.Label(t.getGlobalPosition(t.horizontalPositionType, item.point) + Vector3.up * 1f, item.info, style2);
            
            i++;
        }
        i = 0;
        Handles.color = Color.blue;
        foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
        {
            foreach (var point in FieldPositionData.points)
            {
                if (!point.snap && Handles.Button(t.getGlobalPosition(t.horizontalPositionType, point.value), Quaternion.identity, buttonSize, 1, Handles.SphereHandleCap))
                {
                    selectedPoint = point;
                    selectedPointIndex = i;
                }

                style2.fontSize = 14;
                style2.normal.textColor = Color.yellow;
                point.info = FieldPositionData.playerPositionType.ToString() + " " + selectedPointIndex;
                Handles.Label(t.getGlobalPosition(t.horizontalPositionType, point.value) + Vector3.up * 1f, point.info, style2);
            }
            i++;
        }
        EditorGUI.BeginChangeCheck();
        Vector3 pos1 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, t.newPointPosition), Quaternion.identity);
        GUIStyle style = new GUIStyle();
        style.fontSize = 14;
        style.normal.textColor = Color.yellow;
        Handles.Label(pos1 + Vector3.up * 0.5f,"New Point", style);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move point");
            t.newPointPosition = t.getNormalizedPosition(t.horizontalPositionType, pos1);
            Repaint();
            //t.Update();
        }


        EditorGUI.BeginChangeCheck();
        Vector3 pos3 = Handles.PositionHandle(t.getGlobalPosition(t.horizontalPositionType, t.normailizedBallPosition), Quaternion.identity);
        GUIStyle style3 = new GUIStyle();
        style3.fontSize = 14;
        style3.normal.textColor = Color.yellow;
        Handles.Label(pos3 + Vector3.up * 0.5f, "Ball position", style3);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Move point");
            t.normailizedBallPosition = t.getNormalizedPosition(t.horizontalPositionType, pos3);
            Repaint();
            //t.Update();
        }
        Handles.color = Color.black;
        Vector2 weightyValue = Vector2.zero;
        foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
        {
            t.getWeightyValue4(t.normailizedBallPosition, FieldPositionData.points, out weightyValue);
            Handles.SphereHandleCap(0, t.getGlobalPosition(t.horizontalPositionType, weightyValue) + Vector3.up * 0.25f, Quaternion.identity, 0.25f, EventType.Repaint);
            string info = FieldPositionData.playerPositionType.ToString();
            style.fontSize = 14;
            style.normal.textColor = Color.black;
            Handles.Label(t.getGlobalPosition(t.horizontalPositionType, weightyValue) + Vector3.up * 1f, info, style);
        }

        if (selectedPoint != null)
        {
            if (fieldPositionParameters.points.Contains(selectedPoint))
            {
                Vector3 selectedGlobalPoint = t.getGlobalPosition(t.horizontalPositionType, selectedPoint.point);
                Vector3 selectedGlobalValue = t.getGlobalPosition(t.horizontalPositionType, selectedPoint.value);
                EditorGUI.BeginChangeCheck();
                Vector3 pos = Handles.PositionHandle(selectedGlobalPoint, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Move point");

                    selectedPoint.point = t.getNormalizedPosition(t.horizontalPositionType, pos);
                    for ( i = 0; i < PressureFieldPositionDatas.FieldPositionDatas.Count; i++)
                    {
                        PressureFieldPositionDatas.FieldPositionDatas[i].points[selectedPointIndex].point = pos;
                    }
                    Repaint();
                    //t.Update();
                }
                Handles.color = Color.blue;
                foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
                {
                    FieldPositionsData.Point point = FieldPositionData.points[selectedPointIndex];
                    if (!point.snap && point.enabled)
                        Handles.SphereHandleCap(0,
                           t.getGlobalPosition(t.horizontalPositionType, point.value),
                           Quaternion.identity,
                           buttonSize * 0.9f,
                           EventType.Repaint
                       );
                }
                EditorGUI.BeginChangeCheck();
                float scaleSize = 10;
                Vector3 scale = Handles.ScaleHandle(Vector3.one* (selectedPoint.radio+0.01f)* scaleSize, selectedGlobalPoint+Vector3.up*3, Quaternion.identity, 1);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Scaled ScaleAt Point");
                    foreach (var FieldPositionData in PressureFieldPositionDatas.FieldPositionDatas)
                    {
                        FieldPositionsData.Point point = FieldPositionData.points[selectedPointIndex];
                        point.radio = Mathf.Clamp(scale.x / scaleSize, 0, Mathf.Infinity);
                    }
                    
                }
                Handles.color = Color.yellow;
                if(selectedPoint.useRadio && selectedPoint.enabled)
                    Handles.DrawWireDisc(selectedGlobalPoint, new Vector3(0, 1, 0), selectedPoint.radio* t.fieldWidth);
                if (!selectedPoint.snap && selectedPoint.enabled)
                {
                    EditorGUI.BeginChangeCheck();
                    Vector3 pos2 = Handles.PositionHandle(selectedGlobalValue, Quaternion.identity);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(target, "Move point");
                        selectedPoint.value = t.getNormalizedPosition(t.horizontalPositionType, pos2);
                        Repaint();
                        //t.Update();
                    }
                }
                
            }
        }
        else{
            if (fieldPositionParameters.points.Count > 0)
            {
                selectedPoint = fieldPositionParameters.points[0];
            }
            Repaint();
        }
        if (t.debugRadios) DrawRaidos(fieldPositionParameters,t);
        DrawPositions(fieldPositionParameters,t);
        //fieldPositionParameters.selectedPoint = selectedPoint;
        //t.selectedFieldPositionParameters = fieldPositionParameters;
    }
    void DrawPositions(FieldPositionsData fieldPositionParameters, FootballPositionCtrl t)
    {
        if (!t.debug || !t.debugAllPositions) return;
        PressureFieldPositionDatas fieldPositionList;
        t.getCurrentPressureFieldPositions(out fieldPositionList);
        if (fieldPositionList == null) return;
        foreach (var fieldPositions in fieldPositionList.FieldPositionDatas)
        {
            if (fieldPositionParameters.playerPositionType.Equals(fieldPositions.playerPositionType)) continue;
                Vector2 weightyValue;
                t.getWeightyValue4(t.normailizedBallPosition, fieldPositions.points, out weightyValue);
                Vector3 globalPosition = t.getGlobalPosition(t.horizontalPositionType, weightyValue);
                Handles.color = Color.grey;
                GUIStyle style3 = new GUIStyle();
                style3.fontSize = 14;
                style3.normal.textColor = Color.green;
                Handles.Label(globalPosition + Vector3.up * 1.5f, fieldPositions.playerPositionType.ToString(), style3);
                Handles.SphereHandleCap(0,
                   globalPosition,
                   Quaternion.identity,
                   0.5f,
                   EventType.Repaint
               );

            
        }
    }
    void DrawRaidos(FieldPositionsData fieldPositionParameters, FootballPositionCtrl t)
    {
        foreach (var point in fieldPositionParameters.points)
        {
            if (point.useRadio && point.enabled)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(t.getGlobalPosition(t.horizontalPositionType, point.point), new Vector3(0, 1, 0), point.radio * t.fieldWidth);
            }
        }
        //Repaint();
    }
}
