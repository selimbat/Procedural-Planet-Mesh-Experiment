using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Clan { RedClan, BlueClan }
public enum Territory { Neutral, RedClan, BlueClan, Cliff, Ocean }

public class ClansInfos : MonoBehaviour
{
    public static readonly Dictionary<Clan, Color32> ClanColor = new Dictionary<Clan, Color32>();
    public static readonly Dictionary<Clan, Territory> ClanTerritory = new Dictionary<Clan, Territory>(); 


    private void Awake()
    {
        ClanColor.Add(Clan.BlueClan, new Color32(0,0,255,0));
        ClanColor.Add(Clan.RedClan, new Color32(255, 0, 0, 0));

        ClanTerritory.Add(Clan.BlueClan, Territory.BlueClan);
        ClanTerritory.Add(Clan.RedClan, Territory.RedClan);
    }
}
