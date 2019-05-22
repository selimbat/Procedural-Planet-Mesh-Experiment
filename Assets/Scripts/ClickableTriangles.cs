using System.Collections;
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
                                                    _planet.m_Polygons,
                                                    _planet);

        // Make sure we update the color property of the hit polygon
        hitPoly.m_Color = _redClanColor;

        foreach(Polygon neighborPoly in hitPoly.m_Neighbors)
        {
            if (neighborPoly.CheckIfCircled(_redClanColor))
            {
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 0]] = _redClanColor;
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 1]] = _redClanColor;
                colors32[triangles[neighborPoly.m_triangleIndex * 3 + 2]] = _redClanColor;
            }
        }
        
        mesh.colors32 = colors32;
    }
}
