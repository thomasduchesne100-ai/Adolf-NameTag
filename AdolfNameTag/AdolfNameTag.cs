using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "5.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
private Type vrRigType;

private void Start()
{
    Logger.LogInfo("Adolf NameTag 5.0.0 démarre.");
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
        CreateTags();
        yield return new WaitForSeconds(1f);
    }
}

private void CreateTags()
{
    foreach (UnityEngine.Object obj in Resources.FindObjectsOfTypeAll(vrRigType))
    {
        Component rig = obj as Component;

        if (rig == null)
            continue;

        Transform head = FindHead(rig.transform);

        if (head == null)
            continue;

        Transform oldTag = head.Find("AdolfNameTag");

        GameObject tag;

        if (oldTag == null)
        {
            tag = new GameObject("AdolfNameTag");
            tag.transform.SetParent(head, false);
            tag.transform.localPosition = new Vector3(0f, 0.35f, 0f);

            TextMesh text = tag.AddComponent<TextMesh>();
            text.fontSize = 40;
            text.characterSize = 0.08f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }
        else
        {
            tag = oldTag.gameObject;
        }

        TextMesh mesh = tag.GetComponent<TextMesh>();

        if (mesh != null)
        {
            string name = GetName(rig);
            string platform = GetPlatform(rig);

            int hz = Screen.currentResolution.refreshRate;

            if (hz <= 0)
                hz = 90;

            mesh.text = hz + " Hz\n" + platform + " • " + name;
        }

        Camera cam = Camera.main;

        if (cam != null)
        {
            tag.transform.LookAt(cam.transform);
        }
    }
}

private Transform FindHead(Transform root)
{
    foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
    {
        string n = child.name.ToLower();

        if (n == "head" ||
            n == "headtransform" ||
            n.Contains("head"))
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

    foreach (string member in names)
    {
        object value = GetMember(rig, member);

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

    foreach (string member in names)
    {
        object value = GetMember(rig, member);

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
        BindingFlags.NonPublic
    );

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
        BindingFlags.NonPublic
    );

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
    foreach (Assembly assembly in
        AppDomain.CurrentDomain.GetAssemblies())
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
