using System.Collections.Generic;
using UnityEngine;
using WFC;

[RequireComponent(typeof(TilePalette))]
public class TilePlacer : MonoBehaviour
{
    public delegate void OnPlacedDelegate(int tileId, Vector3 position, float yRotation);
    public event OnPlacedDelegate OnPlaced;
    
    [SerializeField]
    private float _snapSize = 1.0f;
    [SerializeField]
    private int _dimension;

    public float SnapSize => _snapSize;
    public int Dimension => _dimension;
    
    private Camera _cam;
    private TilePalette _palette;
    private List<Transform> _markers;
    private int _currentMarkerIdx;
    
    public Dictionary<Vector3, Tile> PlacedTiles;
    
    private Transform CurrentMarker => _markers[_currentMarkerIdx];
    
    void Start()
    {
        _cam = FindObjectOfType<Camera>();
        _palette = GetComponent<TilePalette>();
        PlacedTiles = new Dictionary<Vector3, Tile>();

        const string markersHolderName = "markers";
        var markersHolder = transform.Find(markersHolderName);
        if (markersHolder == null)
        {
            markersHolder = new GameObject(markersHolderName).transform;
            markersHolder.SetParent(transform);
        }
        _markers = new List<Transform>(_palette.Palette.Count);
        for (var i = 0; i < _palette.Palette.Count; i++)
        {
            var marker = Instantiate(_palette.Palette[i], markersHolder, true);
            marker.name = "marker " + i;
            marker.gameObject.SetActive(false);
            SetMarkerColor(marker.transform, Color.green);
            _markers.Add(marker.transform);
            //Destroy(marker);
        }

        _currentMarkerIdx = 0;
        CurrentMarker.gameObject.SetActive(true);

        var halfSnap = _snapSize / 2;
        CreateBoundaryMarker(WFCHelper.GridPosToPos(new Vector2Int(0, 0), _dimension) +
                             new Vector3(-halfSnap, 0, -halfSnap));
        CreateBoundaryMarker(WFCHelper.GridPosToPos(new Vector2Int(_dimension - 1, 0), _dimension) +
                             new Vector3(halfSnap, 0, -halfSnap));
        CreateBoundaryMarker(WFCHelper.GridPosToPos(new Vector2Int(0, _dimension - 1), _dimension) +
                             new Vector3(-halfSnap, 0, halfSnap));
        CreateBoundaryMarker(WFCHelper.GridPosToPos(new Vector2Int(_dimension - 1, _dimension - 1), _dimension) +
                             new Vector3(halfSnap, 0, halfSnap));
    }

    private void SetMarkerColor(Transform marker, Color color)
    {
        var renderers = marker.GetComponentsInChildren<MeshRenderer>();
        foreach (var renderer in renderers)
        {
            foreach (var mat in renderer.materials)
                mat.color = color;
        }
    }

    private void CreateBoundaryMarker(Vector3 position)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        marker.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        marker.position = position;
    }

    void Update()
    {
        if (_snapSize <= 0)
            return;
       
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        var t = -ray.origin.y / ray.direction.y;
        var x = SnapValue(ray.origin.x + t * ray.direction.x);
        var z = SnapValue(ray.origin.z + t * ray.direction.z);
       
        var markerPosition = new Vector3(x, 0, z);
        var gridPos = WFCHelper.PosToGridPos(markerPosition, _dimension);

        var isPosValid = WFCHelper.IsGridPosValid(gridPos, _dimension);
        SetMarkerColor(CurrentMarker, isPosValid ? Color.green : Color.red);
        CurrentMarker.position = markerPosition;
        if (Input.GetMouseButtonDown(0) && isPosValid)
        {
            if (Input.GetKey(KeyCode.LeftShift))
                DeleteTile(markerPosition);
            else
                PlaceTile(_currentMarkerIdx, markerPosition, CurrentMarker.gameObject.transform.eulerAngles.y, true);
        }
        if (Input.GetMouseButtonDown(1))
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                CurrentMarker.gameObject.transform.Rotate(Vector3.up, 90.0f);
            }
            else
            {
                CurrentMarker.gameObject.SetActive(false);
                _currentMarkerIdx = (_currentMarkerIdx + 1) % _markers.Count;
                CurrentMarker.gameObject.SetActive(true);
            }
        }
    }

    private float SnapValue(float value)
    {
        var abs = Mathf.Abs(value);
        var sign = Mathf.Sign(value);
        var rem = abs % _snapSize;
        var snapped = abs - rem;
        if (rem / _snapSize > 0.5f)
            snapped += _snapSize;
        return snapped * sign;
    }

    public void PlaceTile(int idx, Vector3 position, float yRotation, bool manually = false)
    {
        if (PlacedTiles.ContainsKey(position))
            DeleteTile(position);
        
        var prototype = _palette.Palette[idx];
        var tile = Instantiate(prototype, position, prototype.transform.rotation);
        tile.name += yRotation;
        tile.transform.eulerAngles = new Vector3(0, yRotation, 0);
        PlacedTiles[position] = tile;

        if (manually)
            OnPlaced?.Invoke(tile.TileId, position, yRotation);
    }

    private void DeleteTile(Vector3 position)
    {
        if (!PlacedTiles.ContainsKey(position))
        {
            Debug.Log("wrong position");
            return;
        }
        
        Destroy(PlacedTiles[position].gameObject);
        PlacedTiles.Remove(position);
    }
    
    public void Clear()
    {
        foreach (var tilePos in PlacedTiles.Keys)
            Destroy(PlacedTiles[tilePos].gameObject);
        PlacedTiles.Clear();
    }
}
