using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ThreePupperStudios.Lockable;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[ExecuteAlways]
public class HexMaker : MonoBehaviour
{
    #region Variables And Properties

    private static HexMaker _instance;
    public static HexMaker instance => _instance;

    [SerializeField]
    private bool _shouldUpdate = false;

    [SerializeField]
    private int _verticalCount = 5;

    [SerializeField]
    private int _horizontalCount = 5;

    [SerializeField]
    private bool _centerOrig = false;

    [SerializeField]
    private float outerRadius = 10;

    private float InnerRadius => outerRadius * 0.866025404f;

    [SerializeField]
    private bool _startTop = false;

    [SerializeField]
    private bool _createMesh;

    [SerializeField]
    private bool _createCollider;

    [SerializeField] //, Lockable]
    private MeshFilter _meshFilter;

    [SerializeField]
    private Collider _collider;

    //[SerializeField]
    private Vector3[] _corners = new Vector3[0];

    [SerializeField]
    private Transform _boundaryObj;
    //private MeshRenderer _boundryMesh;

    [Header("Coordinates")]
    [SerializeField]
    private Canvas _canvas;

    [SerializeField]
    private Text _textPrefab;

    [SerializeField]
    private List<Coordinates> _coords = new List<Coordinates>();
    
    [Header("Navigation")]
    [SerializeField]
    private LayerMask _obstaclesLayer = new LayerMask();
    [SerializeField]
    private LayerMask _doorsLayer = new LayerMask();

    [Header("Interaction")]
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private HexHighlighter _highlighterObj;
    
    

    private readonly List<GameObject> _deleteMe = new List<GameObject>();

    private Vector3 _lastPos;

    private float XOffset => _startTop ? InnerRadius * 2f : outerRadius * 1.5f;
    private float ZOffset => _startTop ? outerRadius * 1.5f : InnerRadius * 2f;

    public bool UsesCollider => _createCollider;

    #endregion

    private void OnValidate()
    {
        if (!_shouldUpdate)
        {
            return;
        }

        _shouldUpdate = false;

        SetHexCorners();
        ClearOldCoords();
        CreateNewCoords();

        if (!_createMesh && !_createCollider)
        {
            return;
        }

        CreateMeshAndCollider();

        if (_highlighterObj != null)
        {
            GetHighlightMesh();
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void Awake()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        if (enabled && _instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            while (_deleteMe.Count > 0)
            {
                Destroy(_deleteMe[0]);
                _deleteMe.RemoveAt(0);
            }

            if (!_createMesh && _meshFilter.sharedMesh != null)
            {
                Destroy(_meshFilter.sharedMesh);
                _meshFilter.sharedMesh = null;
                Destroy(_collider);
                _collider = null;
            }
        }
        else
        {
            while (_deleteMe.Count > 0)
            {
                DestroyImmediate(_deleteMe[0]);
                _deleteMe.RemoveAt(0);
            }

            if (!_createMesh && _meshFilter.sharedMesh != null)
            {
                DestroyImmediate(_meshFilter.sharedMesh);
                _meshFilter.sharedMesh = null;
            }
            
            if (!_createCollider)
            {
                DestroyImmediate(_collider);
                _collider = null;
            }
        }
    }

    private void SetHexCorners()
    {
        if (_startTop)
        {
            _corners = new[]
            {
                new Vector3(0f, 0f, outerRadius),
                new Vector3(InnerRadius, 0f, 0.5f * outerRadius),
                new Vector3(InnerRadius, 0f, -0.5f * outerRadius),
                new Vector3(0f, 0f, -outerRadius),
                new Vector3(-InnerRadius, 0f, -0.5f * outerRadius),
                new Vector3(-InnerRadius, 0f, 0.5f * outerRadius)
            };
        }
        else
        {
            _corners = new[]
            {
                new Vector3(outerRadius, 0f, 0),
                new Vector3(0.5f * outerRadius, 0, InnerRadius),
                new Vector3(-0.5f * outerRadius, 0, InnerRadius),
                new Vector3(-outerRadius, 0f, 0),
                new Vector3(-0.5f * outerRadius, 0, -InnerRadius),
                new Vector3(0.5f * outerRadius, 0, -InnerRadius)
            };
        }
    }

    private void ClearOldCoords()
    {
        foreach (var coord in _coords)
        {
            if (coord.textObj != null)
            {
                _deleteMe.Add(coord.textObj.gameObject);
            }
        }

        _coords.Clear();
    }

    private void CreateNewCoords()
    {
        var xOffset = XOffset;
        var zOffset = ZOffset;
        var boundaryObjExists = _boundaryObj != null;
        for (var z = 0; z < _verticalCount; z++)
        {
            var zPos = z - (_centerOrig ? Mathf.Floor(_verticalCount * .5f) : 0f);
            for (var x = 0; x < _horizontalCount; x++)
            {
                Vector3 newPos;
                var xPos = x - (_centerOrig ? Mathf.Floor(_horizontalCount * .5f) : 0f);
                if (_startTop)
                {
                    newPos = new Vector3((xPos + z * 0.5f - z / 2) * xOffset, 0, zPos * zOffset);
                }
                else
                {
                    newPos = new Vector3(xPos * xOffset, 0, (zPos + x * 0.5f - x / 2) * zOffset);
                }

                if (boundaryObjExists && !Contains(newPos))
                {
                    continue;
                }

                SetNeighbors(newPos, x, z);
            }
        }

        for (var i = 0; i < _coords.Count; i++)
        {
            var coord = _coords[i];
            if (coord.textObj != null)
            {
                continue;
            }

            coord.textObj = Instantiate(_textPrefab, _canvas.transform);
            coord.textObj.transform.position = coord.pos + (Vector3.up * .25f);
            coord.textObj.text = coord.coordString;
            _coords[i] = coord;
        }
    }

    private void CreateMeshAndCollider()
    {
        var mesh = new Mesh
        {
            vertices = GetVerts(out List<int> triangles).ToArray(),
            triangles = triangles.ToArray(),
            name = "HexMap",
        };
        if (_createMesh)
        {
            _meshFilter.sharedMesh = mesh;
            _meshFilter.sharedMesh.RecalculateNormals();
        }

        if (_createCollider)
        {
            var hasCollider = TryGetComponent(out MeshCollider meshCollider);
            if (hasCollider)
            {
                meshCollider.sharedMesh = mesh;
                _collider = meshCollider;
            }
            else
            {
                var meshCol = gameObject.AddComponent<MeshCollider>();
                meshCol.sharedMesh = mesh;
                _collider = meshCol;
            }
        }
    }

    public void TryHighlightGrid()
    {
        if (_collider == null || _camera == null)
        {
            return;
        }

        var ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());
        var hitGrid = _collider.Raycast(ray, out var hitInfo, Mathf.Infinity);
        if (hitGrid)
        {
            HighlightHexCoord(hitInfo.point);
        }
    }

    private void GetHighlightMesh()
    {
        var trans = _highlighterObj.transform;
        _highlighterObj.sharedMesh = new Mesh
        {
            vertices = GetVerts(out var triangles, trans,
                new List<Coordinates>() {new Coordinates(trans.position, 0, 0, _startTop)}).ToArray(),
            triangles = triangles.ToArray(),
            name = "Highlight Mesh"
        };
        _highlighterObj.sharedMesh.RecalculateNormals();
    }

    private void SetNeighbors(Vector3 newPos, int x, int z)
    {
        var center = transform.position + newPos;
        var existingCoord = _coords.FindIndex(i => i.pos == center);
        if (existingCoord < 0)
        {
            var i = _coords.Count;
            var newCoord = new Coordinates(center, x, z, _startTop);
            var obstacles = Physics.CheckSphere(center, InnerRadius, _obstaclesLayer);
            
            newCoord.walkable = !obstacles;
            
            _coords.Add(newCoord);
            if (_startTop)
            {
                if (x > 0)
                {
                    newCoord.SetNeighbor(StartTopHexDir.W, _coords, _coords[i - 1]);
                }

                if (z > 0)
                {
                    if ((z & 1) == 0)
                    {
                        newCoord.SetNeighbor(StartTopHexDir.SE, _coords, _coords[i - _horizontalCount]);
                        if (x > 0)
                        {
                            newCoord.SetNeighbor(StartTopHexDir.SW, _coords, _coords[i - _horizontalCount - 1]);
                        }
                    }
                    else
                    {
                        newCoord.SetNeighbor(StartTopHexDir.SW, _coords, _coords[i - _horizontalCount]);
                        if (x < _horizontalCount - 1)
                        {
                            newCoord.SetNeighbor(StartTopHexDir.SE, _coords, _coords[i - _horizontalCount + 1]);
                        }
                    }
                }
            }
            else
            {
                if (x > 0)
                {
                    var dir = (x & 1) == 0 ? HexDir.NW : HexDir.SW;
                    newCoord.SetNeighbor(dir, _coords, _coords[i - 1]);
                }

                if (z > 0)
                {
                    newCoord.SetNeighbor(HexDir.S, _coords, _coords[i - _horizontalCount]);
                    var isEven = (x & 1) == 0;
                    if (isEven)
                    {
                        if (x > 0)
                        {
                            newCoord.SetNeighbor(HexDir.SW, _coords, _coords[i - _horizontalCount - 1]);
                        }

                        if (x < _horizontalCount - 1)
                        {
                            newCoord.SetNeighbor(StartTopHexDir.SE, _coords, _coords[i - _horizontalCount + 1]);
                        }
                    }
                }
            }
        }
    }

    private List<Vector3> GetVerts(out List<int> triangles, Transform overrideTransform = null,
        List<Coordinates> overrideList = null)
    {
        var verts = new List<Vector3>();
        triangles = new List<int>();
        var coords = overrideList ?? _coords; //Shorthand for (overrideList == null?_coords:overrideList)
        var trans = overrideTransform != null ? overrideTransform : transform;

        foreach (var coord in coords)
        {
            var newPos = trans.InverseTransformPoint(coord.pos);
            verts.Add(newPos);
            foreach (var cornerPos in _corners)
            {
                var pos = newPos + cornerPos;
                if (!verts.Contains(pos))
                {
                    verts.Add(pos);
                }
            }

            if (_startTop)
            {
                for (var hexIndex = 0; hexIndex < _corners.Length; hexIndex++)
                {
                    SetValues(newPos, hexIndex, verts, triangles);
                }
            }
            else
            {
                for (var hexIndex = _corners.Length - 1; hexIndex > -1; hexIndex--)
                {
                    SetValues(newPos, hexIndex, verts, triangles, -1);
                }
            }
        }

        return verts;
    }

    private void SetValues(Vector3 newPos, int hexIndex, List<Vector3> verts, List<int> triangles, int changeValue = 1)
    {
        var pos1 = newPos;
        var pos2 = newPos + _corners[hexIndex];
        var pos3 = newPos;
        var nextIndex = hexIndex + changeValue;
        if (nextIndex < _corners.Length && nextIndex >= 0)
        {
            pos3 += _corners[nextIndex];
        }
        else if (_startTop)
        {
            pos3 += _corners[0];
        }
        else
        {
            pos3 += _corners[_corners.Length - 1];
        }

        var indexOf1 = verts.FindIndex(i => i == pos1);
        var indexOf2 = verts.FindIndex(i => i == pos2);
        var indexOf3 = verts.FindIndex(i => i == pos3);
        if (indexOf1 < 0 || indexOf2 < 0 || indexOf3 < 0)
        {
            Debug.LogError("Something fucked up");
        }

        triangles.Add(indexOf1);
        triangles.Add(indexOf2);
        triangles.Add(indexOf3);
    }

    private void HighlightHexCoord(Vector3 pos)
    {
        pos = transform.InverseTransformPoint(pos);
        if (_centerOrig)
        {
            var zOffset = (_centerOrig ? Mathf.Floor(_verticalCount * .5f) : 0);
            var xOffset = (_centerOrig ? Mathf.Floor(_horizontalCount * .5f) : 0);

            xOffset *= (_startTop ? InnerRadius : outerRadius) * 2;
            zOffset *= (_startTop ? outerRadius : InnerRadius) * 2;
            if (_startTop)
            {
                zOffset -= (_startTop ? outerRadius : InnerRadius);
            }
            else
            {
                xOffset -= (_startTop ? InnerRadius : outerRadius);
            }

            pos.x += xOffset;
            pos.z += zOffset;
        }

        var location = Coordinates.GetFromPos(pos, _startTop, InnerRadius, outerRadius);

        var index = 0;
        if (_startTop)
        {
            index = Mathf.Abs(location.x + location.z * _horizontalCount + (location.z / 2));
        }
        else
        {
            index = location.x + ((location.z + location.x / 2) * _horizontalCount);
        }

        if (index < 0 || index >= _coords.Count)
        {
            return;
        }
        var coord = _coords[index];
        _highlighterObj.UpdateDisplay(coord);
        _highlighterObj.transform.position = coord.pos;
    }

    private void OnDrawGizmos()
    {
        if (_lastPos != transform.position)
        {
            _lastPos = transform.position;
            OnValidate();
        }

        var xOffset = XOffset;
        var zOffset = ZOffset;
        var boundaryObjExists = _boundaryObj != null;
        foreach (var coord in _coords)
        {
            for (var hexIndex = 0; hexIndex < _corners.Length; hexIndex++)
            {
                var nextIndex = hexIndex == _corners.Length - 1 ? 0 : 1 + hexIndex;
                var pos1 = coord.pos + _corners[hexIndex];
                var pos2 = coord.pos + _corners[nextIndex];
                Gizmos.DrawLine(pos1, pos2);
            }
        }
    }

    private bool Contains(Vector3 position) //, Vector3[] rotatedBounds)
    {
        var center = _boundaryObj.transform.InverseTransformPoint(transform.localPosition + position);

        var t = _boundaryObj.transform.parent;
        var localPos = t.localPosition;
        var localScale = t.localScale;
        var minX = localPos.x - localScale.x * .5f;
        var minY = localPos.y - localScale.y * .5f;
        var minZ = localPos.z - localScale.z * .5f;
        if (center.x < minX || center.y < minY || center.z < minZ)
        {
            return false;
        }

        var maxX = localPos.x + localScale.x * .5f;
        var maxY = localPos.y + localScale.y * .5f;
        var maxZ = localPos.z + localScale.z * .5f;
        if (center.x > maxX || center.y > maxY || center.z > maxZ)
        {
            return false;
        }

        return true;
    }

    [Serializable]
    public struct Coordinates
    {
        [Lockable]
        public Vector3 pos;

        [Lockable]
        public HexCell coords;

        public bool walkable;
        
        public string coordString => $"{coords.x},{coords.y},{coords.z}";

        [Lockable]
        public Text textObj;

        [SerializeField]
        private Neighbor[] _neighborIndexes;

        public Coordinates(Vector3 p, int x, int z, bool startTop)
        {
            var tempX = x;
            var tempZ = z;

            if (startTop)
            {
                tempX = x - z / 2;
            }
            else
            {
                tempZ = z - x / 2;
            }

            pos = p;
            coords = new HexCell() {x = tempX, z = tempZ};
            textObj = null;
            _neighborIndexes = new Neighbor[6];
            walkable = true;
        }

        public void SetNeighbor(StartTopHexDir direction, List<Coordinates> coords, Coordinates cell)
        {
            _neighborIndexes[(int) direction] = new Neighbor(direction.ToString(), coords.IndexOf(cell));
            var opposite = direction.Opposite();
            cell._neighborIndexes[(int) opposite] = new Neighbor(opposite.ToString(), coords.IndexOf(this));
        }

        public void SetNeighbor(HexDir direction, List<Coordinates> coords, Coordinates cell)
        {
            _neighborIndexes[(int) direction] = new Neighbor(direction.ToString(), coords.IndexOf(cell));
            var opposite = direction.Opposite();
            cell._neighborIndexes[(int) opposite] = new Neighbor(opposite.ToString(), coords.IndexOf(this));
        }

        public int GetNeighbor(StartTopHexDir direction)
        {
            return _neighborIndexes[(int) direction].index;
        }

        public static HexCell GetFromPos(Vector3 pos, bool startTop, float innerRadius, float outerRadius)
        {
            var farSide = (outerRadius * 3f);
            var closeSide = (innerRadius * 2f);
            var x = 0f;
            var y = 0f;
            var xCell = 0;
            var zCell = 0;
            var yCell = 0;
            if (startTop)
            {
                x = pos.x / closeSide;
                y = -x;
                var offset = pos.z / farSide;
                x -= offset;
                y -= offset;
                zCell = Mathf.RoundToInt(-x - y);
                xCell = Mathf.RoundToInt(x);
                yCell = Mathf.RoundToInt(y);

                if (xCell + yCell + zCell != 0)
                {
                    var dX = Mathf.Abs(x - xCell);
                    var dY = Mathf.Abs(y - yCell);
                    var dZ = Mathf.Abs(-x - y - zCell);

                    if (dX > dY && dX > dZ)
                    {
                        xCell = -yCell - zCell;
                    }
                    else if (dZ > dY)
                    {
                        zCell = -xCell - yCell;
                    }
                }
            }
            else
            {
                var z = pos.z / closeSide;
                y = -z;
                var offset = pos.x / farSide;
                z -= offset;
                y -= offset;
                zCell = Mathf.RoundToInt(z);
                xCell = Mathf.RoundToInt(-z - y);
                yCell = Mathf.RoundToInt(y);

                if (zCell + xCell + yCell != 0)
                {
                    var dZ = Mathf.Abs(z - zCell);
                    var dY = Mathf.Abs(y - yCell);
                    var dX = Mathf.Abs(-z - y - xCell);

                    if (dZ > dY && dZ > dX)
                    {
                        zCell = -yCell - xCell;
                    }
                    else if (dX > dY)
                    {
                        xCell = -zCell - yCell;
                    }
                }
            }


            var cell = new HexCell() {x = xCell, z = zCell};
            return cell;
        }

        [Serializable]
        public struct HexCell
        {
            [Lockable(true, false)]
            public int x;

            [SerializeField, Lockable(true, false)]
            private int _y;

            public int y => _y = -x - z;


            [Lockable(true, false)]
            public int z;

            public override bool Equals(object obj)
            {
                if (!(obj is HexCell)) //shortcut for obj != null && obj is HexCell
                {
                    return false;
                }

                var cell = (HexCell) obj;
                return x == cell.x && z == cell.z;
            }

            public bool Equals(HexCell other)
            {
                return x == other.x && z == other.z;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (x * 397) ^ z;
                }
            }
        }

        [Serializable]
        public struct Neighbor
        {
            [Lockable(true, false)]
            public string direction;

            [Lockable(true, false)]
            public int index;

            public Neighbor(string dir, int i)
            {
                direction = dir;
                index = i;
            }
        }
    }
}

public enum StartTopHexDir //This only works when starting top
{
    NE,
    E,
    SE,
    SW,
    W,
    NW
}

public enum HexDir //This only works when starting top
{
    N,
    NE,
    SE,
    S,
    SW,
    NW
}

public static class HexDirectionExtensions
{
    public static StartTopHexDir Opposite(this StartTopHexDir direction)
    {
        return (int) direction < 3 ? (direction + 3) : (direction - 3);
    }

    public static HexDir Opposite(this HexDir direction)
    {
        return (int) direction < 3 ? (direction + 3) : (direction - 3);
    }
}