using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "1.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private void Awake()
    {
        Logger.LogInfo("Adolf NameTag chargé.");
    }
}
