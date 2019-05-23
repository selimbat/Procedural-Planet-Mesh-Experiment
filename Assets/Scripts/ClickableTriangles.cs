using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickableTriangles : MonoBehaviour
{
    private Camera _mainCamera;
    private Planet _planet;

    private void Awake()
    {
        _mainCamera = Camera.main;
        _planet = GetComponentInParent<Planet>();
    }

    public void Update()
    {
        if (Input.GetMouseButton(0))
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
            int[] triangles = mesh.triangles;
            Color32[] colors32 = mesh.colors32;

            // search for the Polygon object representing the clicked triangle among 
            // the land polygons of the planet.
            Polygon hitPoly = PolySet.FindPolyInPolyset(hit.triangleIndex, _planet);

            // if the Polygon object is not a land polygon
            if (hitPoly == null || hitPoly.m_territory == Territory.RedClan)
            {
                return;
            }

            // Change the color of the clicked triangle
            ColorTriangle(triangles, hit.triangleIndex, ref colors32, Clan.RedClan);

            // Make sure we update the color and territory properties of the hit polygon
            hitPoly.UpdateClan(Clan.RedClan);

            // See if a zone has been circled
            LookForCircledZonesInNeighbors(hitPoly, triangles, ref colors32);

            mesh.colors32 = colors32;
        }
    }

    private void ColorTriangle(int[] triangles, int triangleIndex, ref Color32[] colors32, Clan clan)
    {
        colors32[triangles[triangleIndex * 3 + 0]] = ClansInfos.ClanColor[clan];
        colors32[triangles[triangleIndex * 3 + 1]] = ClansInfos.ClanColor[clan];
        colors32[triangles[triangleIndex * 3 + 2]] = ClansInfos.ClanColor[clan];
    }

    private void LookForCircledZonesInNeighbors(Polygon hitPoly, int[] triangles, ref Color32[] colors32)
    {
        foreach (Polygon neighborPoly in hitPoly.m_Neighbors)
        {
            if (neighborPoly.m_territory == Territory.Neutral || neighborPoly.m_territory == Territory.BlueClan)
            {
                KeyValuePair<bool, PolySet> zoneToBeCircled = IsZoneCircled(neighborPoly, new PolySet() { });
                if (zoneToBeCircled.Key)
                {
                    Debug.Log("Territorial combo : " + zoneToBeCircled.Value.Count + " zones!!!");
                    foreach (Polygon poly in zoneToBeCircled.Value)
                    {
                        ColorTriangle(triangles, poly.m_triangleIndex, ref colors32, Clan.RedClan);
                        poly.UpdateClan(Clan.RedClan);
                    }
                }
            }
        }

    }
    private KeyValuePair<bool,PolySet> IsZoneCircled(Polygon currentPoly, PolySet checkedPolys, int depth = 0)
    {
        depth++;
        checkedPolys.Add(currentPoly);
        /*
        if (depth > 10 || checkedPolys.Count > 10)
        {
            return new KeyValuePair<bool, PolySet>(false, checkedPolys);
        }*/
        bool isCircled = true;
        foreach (Polygon neighborPoly in currentPoly.m_Neighbors)
        {
            if (neighborPoly.m_territory == Territory.Cliff || neighborPoly.m_territory == Territory.Ocean)
            {
                return new KeyValuePair<bool, PolySet>(false, checkedPolys);
            }
            else if (!checkedPolys.Contains(neighborPoly) &&
                        (neighborPoly.m_territory == Territory.Neutral ||
                         neighborPoly.m_territory == Territory.BlueClan))
            {
                KeyValuePair<bool, PolySet> result = IsZoneCircled(neighborPoly, checkedPolys, depth);
                checkedPolys = result.Value;
                isCircled &= result.Key;
            }
        }
        return new KeyValuePair<bool, PolySet>(isCircled, checkedPolys);
    }
}
