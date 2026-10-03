using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "8.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
private Type vrRigType;

private void Start()
{
    Logger.LogInfo("Adolf NameTag 8.0.0 démarre.");
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
        yield return new WaitForSeconds(0.5f);
    }
}

private void UpdateTags()
{
    UnityEngine.Object[] objects =
        Resources.FindObjectsOfTypeAll(vrRigType);

    foreach (UnityEngine.Object obj in objects)
    {
        Component rig = obj as Component;

        if (rig == null)
            continue;

        Transform tagParent = FindHead(rig.transform);

        if (tagParent == null)
            tagParent = rig.transform;

        GameObject tag = tagParent.Find("AdolfNameTag")?.gameObject;

        if (tag == null)
        {
            tag = new GameObject("AdolfNameTag");

            tag.transform.SetParent(tagParent, false);
            tag.transform.localPosition =
                new Vector3(0f, 0.35f, 0f);

            tag.transform.localRotation =
                Quaternion.identity;

            tag.transform.localScale =
                new Vector3(0.01f, 0.01f, 0.01f);

            TextMesh text =
                tag.AddComponent<TextMesh>();

            text.fontSize = 50;
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
        }

        TextMesh mesh =
            tag.GetComponent<TextMesh>();

        if (mesh == null)
            continue;

        int hz =
            Screen.currentResolution.refreshRate;

        if (hz <= 0)
            hz = 90;

        string playerName =
            GetPlayerName(rig);

        string platform =
            GetPlatform(rig);

        mesh.text =
            hz + " Hz\n" +
            platform + " - " +
            playerName;

        mesh.anchor =
            TextAnchor.MiddleCenter;

        mesh.alignment =
            TextAlignment.Center;

        Camera camera =
            Camera.main;

        if (camera != null)
        {
            Vector3 direction =
                tag.transform.position -
                camera.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                tag.transform.rotation =
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
    Transform[] children =
        root.GetComponentsInChildren<Transform>(true);

    foreach (Transform child in children)
    {
        string name =
            child.name.ToLower();

        if (name == "head" ||
            name == "headtransform" ||
            name == "headanchor" ||
            name == "head_anchor" ||
            name.Contains("head"))
        {
            return child;
        }
    }

    return null;
}

private string GetPlayerName(Component rig)
{
    string[] fields =
    {
        "playerName",
        "PlayerName",
        "nickName",
        "nickname",
        "username",
        "UserName",
        "defaultName",
        "creatorUsername"
    };

    foreach (string fieldName in fields)
    {
        object value =
            GetMember(rig, fieldName);

        if (value == null)
            continue;

        string result =
            value.ToString();

        if (!string.IsNullOrWhiteSpace(result))
            return result;
    }

    return "Unknown";
}

private string GetPlatform(Component rig)
{
    string[] fields =
    {
        "platform",
        "Platform",
        "platformType",
        "platformTag",
        "Player_Platform"
    };

    foreach (string fieldName in fields)
    {
        object value =
            GetMember(rig, fieldName);

        if (value == null)
            continue;

        string result =
            value.ToString().ToUpper();

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

private object GetMember(
    Component rig,
    string name)
{
    Type type =
        rig.GetType();

    FieldInfo field =
        type.GetField(
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

    PropertyInfo property =
        type.GetProperty(
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
            Type type =
                assembly.GetType(name);

            if (type != null)
                return type;
        }
        catch
        {
        }
    }

    foreach (Assembly assembly in
             AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            foreach (Type type in
                     assembly.GetTypes())
            {
                if (type.Name == name)
                    return type;
            }
        }
        catch
        {
        }
    }

    return null;
  }
}
