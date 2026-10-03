using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "7.0.0")]
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

        GameObject tagObject = null;
        Transform existing = head.Find("AdolfNameTag");

        if (existing != null)
        {
            tagObject = existing.gameObject;
        }
        else
        {
            tagObject = new GameObject("AdolfNameTag");
            tagObject.transform.SetParent(head, false);
            tagObject.transform.localPosition = new Vector3(0f, 0.35f, 0f);
            tagObject.transform.localScale = Vector3.one * 0.01f;

            TextMesh text = tagObject.AddComponent<TextMesh>();
            text.fontSize = 40;
            text.characterSize = 0.1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;
        }

        TextMesh mesh = tagObject.GetComponent<TextMesh>();

        if (mesh == null)
            continue;

        int hz = Screen.currentResolution.refreshRate;

        if (hz <= 0)
            hz = 90;

        string platform = GetPlatform(rig);
        string playerName = GetName(rig);

        mesh.text =
            hz + " Hz\n" +
            platform + " - " + playerName;

        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.alignment = TextAlignment.Center;

        Camera camera = Camera.main;

        if (camera != null)
        {
            Vector3 direction =
                tagObject.transform.position -
                camera.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                tagObject.transform.rotation =
                    Quaternion.LookRotation(
                        direction,
                        Vector3.up
                    );
            }
        }
    }
}

private Transform FindHead(Transform root)
{
    foreach (Transform child in
             root.GetComponentsInChildren<Transform>(true))
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

        if (value != null)
        {
            string result = value.ToString();

            if (!string.IsNullOrWhiteSpace(result))
                return result;
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

private object GetMember(Component rig, string name)
{
    Type type = rig.GetType();

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
            return field.GetValue(rig);
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
            return property.GetValue(rig);
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
}
