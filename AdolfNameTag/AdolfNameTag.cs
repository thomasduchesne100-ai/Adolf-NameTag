using BepInEx;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "1.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private void Start()
    {
        Logger.LogInfo("Adolf NameTag chargé !");
    }
}
