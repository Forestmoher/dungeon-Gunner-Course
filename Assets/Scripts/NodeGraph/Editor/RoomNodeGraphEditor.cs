using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;

public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle _roomNodeStyle;
    private static RoomNodeGraphSO _currentRoomNodeGraph;
    private RoomNodeTypeListSO _roomNodeTypeList;
    private RoomNodeSO _currentRoomNode = null;

    //Node layout values
    private const float _nodeWidth = 160f;
    private const float _nodeHeight = 75f;
    private const int _nodePadding = 25;
    private const int _nodeBorder = 12;
    private const float _connetingLineWidth = 3f;

    [MenuItem("Room Node Graph Editor", menuItem = "Window/Dungeon Editor/Room Node Graph Editor")]
    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
    }

    private void OnEnable()
    {
        //Define node layout style
        _roomNodeStyle = new GUIStyle();
        _roomNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;
        _roomNodeStyle.normal.textColor = Color.white;
        _roomNodeStyle.padding = new RectOffset(_nodePadding, _nodePadding, _nodePadding, _nodePadding);
        _roomNodeStyle.border = new RectOffset(_nodeBorder, _nodeBorder, _nodeBorder, _nodeBorder);

        _roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Open the room node graph editor window if a room node graph scriptable object asset is double clicked in the inspector
    /// </summary>
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceID, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceID) as RoomNodeGraphSO;
        if (roomNodeGraph != null)
        {
            OpenWindow();

            _currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }

    private void OnGUI()
    {
        // If a scriptable object of type RoomNodeGraphSO has been selected, then process
        if (_currentRoomNodeGraph != null)
        {
            //draw line if being dragged
            DrawDraggedLine();

            //Process events
            ProcessEvents(Event.current);

            //Draw room nodes
            DrawRoomNodes();
        }

        if (GUI.changed) Repaint();
    }

    private void DrawDraggedLine()
    {
        if (_currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(_currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, _currentRoomNodeGraph.linePosition,
                _currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, _currentRoomNodeGraph.linePosition, Color.white, null, _connetingLineWidth);
        }
    }

    private void ProcessEvents(Event currentEvent)
    {
        //get room node that mouse is currently over if it's null or not currently being dragged
        if(_currentRoomNode == null || _currentRoomNode.isLeftClickDragging == false)
        {
            _currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }
        //if mouse is over room node, or we are currently dragging a line from the node
        if(_currentRoomNode == null || _currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        //else process room node events
        else
        {
            _currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// Check to see if mouse is over room node, if so return node, else return null 
    /// </summary>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        for (int i = _currentRoomNodeGraph.roomNodeList.Count - 1; i >= 0; i--)
        {
            if (_currentRoomNodeGraph.roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return _currentRoomNodeGraph.roomNodeList[i];
            }
        }
        return null;
    }

    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;
            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;
            default:
                break;
        }
    }

    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if(currentEvent.button == 1)//right click
        {
            ShowContextMenu(currentEvent.mousePosition);
        }
    }

    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();

        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);

        menu.ShowAsContext();
    }

    private void CreateRoomNode(object mousePositionObject)
    {
        CreateRoomNode(mousePositionObject, _roomNodeTypeList.list.Find(x => x.isNone));
    }

    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        //create room node scriptable object asset
        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();

        //add room node to current room node graph room node list
        _currentRoomNodeGraph.roomNodeList.Add(roomNode);

        //set room node values
        roomNode.Initialize(new Rect(mousePosition, new Vector2(_nodeWidth, _nodeHeight)), _currentRoomNodeGraph, roomNodeType);

        //add room node to room node graph scriptable object asset database (adds object as child)
        AssetDatabase.AddObjectToAsset(roomNode, _currentRoomNodeGraph);

        AssetDatabase.SaveAssets();
    }

    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if(currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
    }

    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if(_currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    private void DragConnectingLine(Vector2 delta)
    {
        _currentRoomNodeGraph.linePosition += delta;
    }

    private void DrawRoomNodes()
    {
        //loop through all nodes and draw them
        foreach(RoomNodeSO roomNode in _currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(_roomNodeStyle);
        }

        GUI.changed = true;
    }


}
