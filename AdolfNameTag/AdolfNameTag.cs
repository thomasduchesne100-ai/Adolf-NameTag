using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "6.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
private Type vrRigType;

private void Start()
{
    Logger.LogInfo("Adolf NameTag démarre.");
    StartCoroutine(Setup());
}

private IEnumerator Setup()
{
    yield return new WaitForSeconds(3f);

    vrRigType = FindType("VRRig");

    if (vrRigType == null)
    {
        Logger.LogError("VRRig introuvable.");
        yield break;
    }

    Logger.LogInfo("VRRig trouvé.");

    while (true)
    {
        UpdateTags();
        yield return new WaitForSeconds(1f);
    }
}

private void UpdateTags()
{
    foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(vrRigType))
    {
        Component rig = obj as Component;

        if (rig == null)
            continue;

        Transform head = FindHead(rig.transform);

        if (head == null)
            continue;

        GameObject tag = head.Find("AdolfNameTag")?.gameObject;

        if (tag == null)
        {
            tag = new GameObject("AdolfNameTag");
            tag.transform.SetParent(head, false);
            tag.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            tag.transform.localScale = Vector3.one * 0.01f;

            TextMesh text = tag.AddComponent<TextMesh>();
            text.fontSize = 40;
            text.characterSize = 0.1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        TextMesh mesh = tag.GetComponent<TextMesh>();

        if (mesh == null)
            continue;

        int hz = Screen.currentResolution.refreshRate;

        if (hz <= 0)
            hz = 90;

        mesh.text = hz + " Hz\n" +
                    GetPlatform(rig) + " • " +
                    GetName(rig);

        Camera cam = Camera.main;

        if (cam != null)
            tag.transform.LookAt(cam.transform);
    }
}

private Transform FindHead(Transform root)
{
    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
    {
        string name = child.name.ToLower();

        if (name == "head" ||
            name == "headtransform" ||
            name.Contains("head"))
        {
            return child;
        }
    }

    return null;
}

private string GetName(Component rig)
{
    string[] names =
    {
        "playerName",
        "nickName",
        "nickname",
        "username",
        "UserName"
    };

    foreach (string name in names)
    {
        object value = GetMember(rig, name);

        if (value != null &&
            !string.IsNullOrWhiteSpace(value.ToString()))
        {
            return value.ToString();
        }
    }

    return "Unknown";
}

private string GetPlatform(Component rig)
{
    string[] names =
    {
        "platform",
        "platformType",
        "platformTag",
        "Player_Platform"
    };

    foreach (string name in names)
    {
        object value = GetMember(rig, name);

        if (value == null)
            continue;

        string result = value.ToString().ToUpper();

        if (result.Contains("META") ||
            result.Contains("OCULUS") ||
            result.Contains("QUEST"))
        {
            return "META";
        }

        if (result.Contains("STEAM"))
            return "STEAM";
    }

    return "STEAM";
}

private object GetMember(Component obj, string name)
{
    Type type = obj.GetType();

    FieldInfo field = type.GetField(
        name,
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic);

    if (field != null)
    {
        try
        {
            return field.GetValue(obj);
        }
        catch
        {
        }
    }

    PropertyInfo property = type.GetProperty(
        name,
        BindingFlags.Instance |
        BindingFlags.Public |
        BindingFlags.NonPublic);

    if (property != null)
    {
        try
        {
            return property.GetValue(obj);
        }
        catch
        {
        }
    }

    return null;
}

private Type FindType(string name)
{
    foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
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
