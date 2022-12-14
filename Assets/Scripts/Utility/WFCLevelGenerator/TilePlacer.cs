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
    [SerializeField]
    private Material _markerMaterial;
    [SerializeField]
    private float _markerColorAlpha = 0.2f;

    private Color _greenMarkerColor;
    private Color _redMarkerColor;
    private Color _yellowMarkerColor;

    public float SnapSize => _snapSize;
    public int Dimension => _dimension;
    
    private Camera _cam;
    private TilePalette _palette;
    private List<Transform> _markers;
    private int _currentMarkerIdx;
    
    public Dictionary<Vector3, Tile> PlacedTiles;
    
    private Transform CurrentMarker => _markers[_currentMarkerIdx];

    void Awake()
    {
        _greenMarkerColor = Color.green;
        _redMarkerColor = Color.red;
        _yellowMarkerColor = Color.yellow;
        _greenMarkerColor.a = _redMarkerColor.a = _yellowMarkerColor.a = _markerColorAlpha;
    }
    
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
            var markerRenderers = marker.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in markerRenderers)
                renderer.material = _markerMaterial;
            SetMarkerColor(marker.transform, _greenMarkerColor);
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

    private void SetMarkerColor(Component marker, Color color)
    {
        foreach (var renderer in marker.GetComponentsInChildren<MeshRenderer>())
            renderer.material.color = color;
    }

    private void CreateBoundaryMarker(Vector3 position)
    {
        var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
        marker.localScale = new Vector3(0.25f, 0.25f, 0.25f);
        marker.position = position;
    }

    private Vector2Int _previousPlacePos;
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
        var markerColor = _greenMarkerColor;
        if (!isPosValid)
            markerColor = _redMarkerColor;
        else if (PlacedTiles.ContainsKey(markerPosition))
            markerColor = _yellowMarkerColor;
        SetMarkerColor(CurrentMarker, markerColor);
        //shift marker slightly towards camera so it can be rendered on top of already placed tile
        var markerToCamDirection = (_cam.transform.position - markerPosition).normalized;
        CurrentMarker.position = markerPosition + markerToCamDirection * 0.1f;
        var haveLmbInput = Input.GetMouseButtonDown(0) || Input.GetMouseButton(0) && _previousPlacePos != gridPos;
        if (isPosValid && haveLmbInput)
        {
            _previousPlacePos = gridPos;
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

    public void PlaceTile(int idx, Vector3 position, float yRotation, bool manually)
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
            return;
        
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
