using System;
using System.Collections;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "7.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
private Type vrRigType;
private bool created;

private void Start()
{
    Logger.LogInfo("Adolf NameTag 7.0.0 démarre.");
    StartCoroutine(Setup());
}

private IEnumerator Setup()
{
    yield return new WaitForSeconds(5f);

    vrRigType = FindType("VRRig");

    if (vrRigType == null)
    {
        Logger.LogError("VRRig introuvable.");
        yield break;
    }

    Logger.LogInfo("VRRig trouvé.");

    while (!created)
    {
        CreateTestTag();
        yield return new WaitForSeconds(1f);
    }
}

private void CreateTestTag()
{
    UnityEngine.Object[] rigs =
        Resources.FindObjectsOfTypeAll(vrRigType);

    Logger.LogInfo("VRRig trouvés : " + rigs.Length);

    foreach (UnityEngine.Object obj in rigs)
    {
        Component rig = obj as Component;

        if (rig == null)
            continue;

        GameObject tag = new GameObject("AdolfNameTag");

        tag.transform.SetParent(rig.transform, false);
        tag.transform.localPosition =
            new Vector3(0f, 1.5f, 0f);

        tag.transform.localScale =
            Vector3.one * 0.02f;

        TextMesh text =
            tag.AddComponent<TextMesh>();

        text.text = "NAME TAG TEST";
        text.fontSize = 100;
        text.characterSize = 0.1f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;

        MeshRenderer renderer =
            tag.GetComponent<MeshRenderer>();

        if (renderer != null)
        {
            Shader shader =
                Shader.Find("GUI/Text Shader");

            if (shader != null)
            {
                renderer.material =
                    new Material(shader);

                renderer.material.color =
                    Color.white;
            }
        }

        Logger.LogInfo(
            "TEST NAME TAG CRÉÉ SUR : " +
            rig.name
        );

        created = true;
        break;
    }
}

private Type FindType(string name)
{
    foreach (System.Reflection.Assembly assembly
             in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            Type type = assembly.GetType(name);

            if (type != null)
                return type;
        }
        catch
        {
        }
    }

    return null;
  }
}
