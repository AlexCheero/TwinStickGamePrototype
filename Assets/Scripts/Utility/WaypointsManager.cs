using UnityEngine;

public static class WaypointsManager
{
    public static Vector3[] WaypointPositions { get; private set; }

    public static void Gather()
    {
        var waypointObjs = GameObject.FindGameObjectsWithTag("Waypoint");
        WaypointPositions = new Vector3[waypointObjs.Length];
        for (int i = 0; i < waypointObjs.Length; i++)
            WaypointPositions[i] = waypointObjs[i].transform.position;
    }
}
