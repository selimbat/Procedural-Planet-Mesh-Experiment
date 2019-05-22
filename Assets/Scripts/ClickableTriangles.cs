using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTriangles : MonoBehaviour
{
    private Camera _mainCamera;
    private Planet _planet;
    private Color32 _redClanColor = new Color32(255, 0, 0, 0);

    private void Awake()
    {
        _mainCamera = Camera.main;
        _planet = GetComponentInParent<Planet>();
    }

    public void OnMouseDown()
    {
        RaycastHit hit;
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out hit))
        {
            return;
        }

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null)
        {
            return;
        }

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Color32[] colors32 = mesh.colors32;

        // Change the color of the clicked triangle
        colors32[triangles[hit.triangleIndex * 3 + 0]] = _redClanColor;
        colors32[triangles[hit.triangleIndex * 3 + 1]] = _redClanColor;
        colors32[triangles[hit.triangleIndex * 3 + 2]] = _redClanColor;

        // Get the Polygon object representing the clicked triangle and check if its 
        // neighbors are now circled.
        Polygon hitPoly = PolySet.FindPolyInPolyset(vertices[hit.triangleIndex * 3 + 0],
                                                    vertices[hit.triangleIndex * 3 + 1],
                                                    vertices[hit.triangleIndex * 3 + 2],
                                                    _planet.m_landPolys,
                                                    _planet);

        // Make sure we update the color property of the hit polygon
        hitPoly.m_Color = _redClanColor;

        foreach(Polygon neighborPoly in hitPoly.m_Neighbors)
        {
            if (!neighborPoly.m_Color.Equals(_redClanColor))
            {
                KeyValuePair<bool, PolySet> zoneToBeCircled = IsZoneCircled(neighborPoly, new PolySet() { });
                if (zoneToBeCircled.Key)
                {
                    foreach (Polygon poly in zoneToBeCircled.Value)
                    {
                        colors32[triangles[poly.m_triangleIndex * 3 + 0]] = _redClanColor;
                        colors32[triangles[poly.m_triangleIndex * 3 + 1]] = _redClanColor;
                        colors32[triangles[poly.m_triangleIndex * 3 + 2]] = _redClanColor;
                        poly.m_Color = _redClanColor;
                    }
                }
            }
        }
        /*
        foreach(Polygon neighborPoly in hitPoly.m_Neighbors)
        {
            if (neighborPoly.CheckIfCircled(_redClanColor))
            {
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 0]] = _redClanColor;
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 1]] = _redClanColor;
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 2]] = _redClanColor;
                neighborPoly.m_Color = _redClanColor;
            }
        }*/
        
        mesh.colors32 = colors32;
    }

    private List<PolySet> RecursiveLookForLoops(Polygon currentPoly, Polygon rootPoly, PolySet checkedPolys, PolySet circledPolys)
    {
        if (currentPoly == rootPoly)
        {
            // add polygons of the cycle to circledPolys
            return new List<PolySet>() { checkedPolys, circledPolys };
        }
        else
        {
            checkedPolys.Add(currentPoly);
            foreach (Polygon neighborPoly in currentPoly.m_Neighbors)
            {
                if (neighborPoly.m_Color.Equals(_redClanColor) && !checkedPolys.Contains(neighborPoly))
                {
                    List<PolySet> result = RecursiveLookForLoops(neighborPoly, rootPoly, checkedPolys, circledPolys);
                    checkedPolys = result[0];
                    circledPolys = result[1];
                }
            }
            return new List<PolySet>() { checkedPolys, circledPolys };
        }
    }

    private KeyValuePair<bool,PolySet> IsZoneCircled(Polygon currentPoly, PolySet checkedPolys, int depth = 0)
    {
        depth++;
        checkedPolys.Add(currentPoly);
        if (depth > 10)
        {
            return new KeyValuePair<bool, PolySet>(false, checkedPolys);
        }
        bool isCircled = true;
        foreach (Polygon neighborPoly in currentPoly.m_Neighbors)
        {
            if (!checkedPolys.Contains(neighborPoly) && !neighborPoly.m_Color.Equals(_redClanColor))
            {
                KeyValuePair<bool, PolySet> result = IsZoneCircled(neighborPoly, checkedPolys, depth);
                checkedPolys = result.Value;
                isCircled &= result.Key;
            }
        }
        return new KeyValuePair<bool, PolySet>(isCircled, checkedPolys);
    }
}
