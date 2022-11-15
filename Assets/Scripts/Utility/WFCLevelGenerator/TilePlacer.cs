using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TilePalette))]
public class TilePlacer : MonoBehaviour
{
    [SerializeField]
    private float _snapSize = 1.0f;
    
    private Camera _cam;
    private TilePalette _palette;
    private List<Transform> _markers;
    private int _currentMarkerIdx;

    private Transform CurrentMarker => _markers[_currentMarkerIdx];
    
    void Start()
    {
        _cam = FindObjectOfType<Camera>();
        _palette = GetComponent<TilePalette>();

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
            var renderers = marker.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.materials)
                    mat.color = Color.green;
            }
            _markers.Add(marker.transform);
            //Destroy(marker);
        }

        _currentMarkerIdx = 0;
        CurrentMarker.gameObject.SetActive(true);
    }

    void Update()
    {
        if (_snapSize <= 0)
            return;
       
        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
        float SnapValue(float value)
        {
            var abs = Mathf.Abs(value);
            var sign = Mathf.Sign(value);
            var rem = abs % _snapSize;
            var snapped = abs - rem;
            if (rem / _snapSize > 0.5f)
                snapped += _snapSize;
            return snapped * sign;
        }
       
        var t = -ray.origin.y / ray.direction.y;
        var x = SnapValue(ray.origin.x + t * ray.direction.x);
        var z = SnapValue(ray.origin.z + t * ray.direction.z);
       
        var markerPosition = new Vector3(x, 0, z);
        CurrentMarker.position = markerPosition;
        if (Input.GetMouseButtonDown(0))
        {
            var prototype = _palette.Palette[_currentMarkerIdx];
            Instantiate(prototype, markerPosition, prototype.transform.rotation);
        }
        if (Input.GetMouseButtonDown(1))
        {
            CurrentMarker.gameObject.SetActive(false);
            _currentMarkerIdx = (_currentMarkerIdx + 1) % _markers.Count;
            CurrentMarker.gameObject.SetActive(true);
        }
    }
}
