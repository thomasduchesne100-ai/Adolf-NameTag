using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "4.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private Type vrRigType;

    private readonly Dictionary<object, GameObject> nameTags =
        new Dictionary<object, GameObject>();

    private void Start()
    {
        Logger.LogInfo("Adolf NameTag 4.0.0 démarre.");
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        yield return new WaitForSeconds(3f);

        vrRigType = FindType("VRRig");

        if (vrRigType == null)
        {
            Logger.LogError("Adolf NameTag : VRRig introuvable.");
            yield break;
        }

        Logger.LogInfo("Adolf NameTag : VRRig trouvé.");

        while (true)
        {
            UpdateNameTags();
            yield return new WaitForSeconds(0.5f);
        }
    }

    private void UpdateNameTags()
    {
        UnityEngine.Object[] rigs =
            Resources.FindObjectsOfTypeAll(vrRigType);

        foreach (UnityEngine.Object rigObject in rigs)
        {
            if (rigObject == null)
                continue;

            object rig = rigObject;

            GameObject rigGameObject =
                rigObject as GameObject;

            Component component =
                rigObject as Component;

            if (rigGameObject == null && component != null)
                rigGameObject = component.gameObject;

            if (rigGameObject == null)
                continue;

            Transform head =
                FindHead(rigGameObject.transform);

            if (head == null)
                head = rigGameObject.transform;

            GameObject tag;

            if (!nameTags.TryGetValue(rig, out tag) || tag == null)
            {
                tag = CreateNameTag(head);
                nameTags[rig] = tag;
            }

            if (tag == null)
                continue;

            tag.transform.SetParent(head, false);
            tag.transform.localPosition =
                new Vector3(0f, 0.35f, 0f);

            UpdateTagText(rig, tag);
        }
    }

    private GameObject CreateNameTag(Transform parent)
    {
        GameObject tag =
            new GameObject("AdolfNameTag");

        tag.transform.SetParent(parent, false);
        tag.transform.localPosition =
            new Vector3(0f, 0.35f, 0f);

        tag.transform.localScale =
            new Vector3(0.01f, 0.01f, 0.01f);

        TextMesh text =
            tag.AddComponent<TextMesh>();

        text.text = "90 Hz\nSTEAM • Player";
        text.fontSize = 32;
        text.characterSize = 0.1f;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;

        return tag;
    }

    private void UpdateTagText(object rig, GameObject tag)
    {
        TextMesh text =
            tag.GetComponent<TextMesh>();

        if (text == null)
            return;

        string playerName =
            GetPlayerName(rig);

        string platform =
            GetPlatform(rig);

        int hz = 90;

        try
        {
            hz = Screen.currentResolution.refreshRate;
        }
        catch
        {
        }

        if (hz <= 0)
            hz = 90;

        text.text =
            hz + " Hz\n" +
            platform + " • " +
            playerName;

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

    private string GetPlayerName(object rig)
    {
        string[] names =
        {
            "playerName",
            "nickName",
            "nickname",
            "defaultName",
            "creatorUsername",
            "PlayerName",
            "username",
            "UserName"
        };

        foreach (string name in names)
        {
            object value =
                GetMemberValue(rig, name);

            if (value != null)
            {
                string result =
                    value.ToString();

                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
        }

        return "Unknown";
    }

    private string GetPlatform(object rig)
    {
        string[] names =
        {
            "platformTag",
            "platformType",
            "Player_Platform",
            "platform",
            "Platform"
        };

        foreach (string name in names)
        {
            object value =
                GetMemberValue(rig, name);

            if (value == null)
                continue;

            string platform =
                value.ToString().ToUpper();

            if (platform.Contains("STEAM"))
                return "STEAM";

            if (platform.Contains("META") ||
                platform.Contains("OCULUS") ||
                platform.Contains("QUEST"))
                return "META";
        }

        return "STEAM";
    }

    private object GetMemberValue(
        object instance,
        string memberName)
    {
        if (instance == null)
            return null;

        Type type =
            instance.GetType();

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null)
        {
            try
            {
                return property.GetValue(instance);
            }
            catch
            {
            }
        }

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            try
            {
                return field.GetValue(instance);
            }
            catch
            {
            }
        }

        return null;
    }

    private Transform FindHead(Transform root)
    {
        string[] names =
        {
            "head",
            "Head",
            "headTransform",
            "HeadTransform"
        };

        foreach (string name in names)
        {
            Transform result =
                FindChildRecursive(root, name);

            if (result != null)
                return result;
        }

        return null;
    }

    private Transform FindChildRecursive(
        Transform parent,
        string targetName)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(
                targetName,
                StringComparison.OrdinalIgnoreCase))
            {
                return child;
            }

            Transform result =
                FindChildRecursive(
                    child,
                    targetName
                );

            if (result != null)
                return result;
        }

        return null;
    }

    private Type FindType(string typeName)
    {
        foreach (Assembly assembly in
                 AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type type =
                    assembly.GetType(typeName);

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
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName)
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
    private void UpdateNameTags()
    {
        if (vrRigType == null)
            return;

        UnityEngine.Object[] rigs =
            Resources.FindObjectsOfTypeAll(vrRigType);

        foreach (UnityEngine.Object rigObject in rigs)
        {
            if (rigObject == null)
                continue;

            object rig = rigObject;

            string playerName = GetPlayerName(rig);

            if (string.IsNullOrEmpty(playerName))
                playerName = "Unknown";

            string platform = GetPlatform(rig);

            int hz = GetRefreshRate();

            string finalText =
                hz + " Hz\n" +
                platform + " • " +
                playerName;

            SetNameText(rig, finalText);
        }
    }

    private string GetPlayerName(object rig)
    {
        string[] names =
        {
            "playerName",
            "nickName",
            "nickname",
            "defaultName",
            "creatorUsername",
            "PlayerName"
        };

        foreach (string name in names)
        {
            object value = GetMemberValue(rig, name);

            if (value != null)
            {
                string result = value.ToString();

                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }
        }

        return "";
    }

    private string GetPlatform(object rig)
    {
        string[] names =
        {
            "platformTag",
            "platformType",
            "Player_Platform",
            "platform",
            "Platform"
        };

        foreach (string name in names)
        {
            object value = GetMemberValue(rig, name);

            if (value != null)
            {
                string platform = value.ToString().ToUpper();

                if (platform.Contains("STEAM"))
                    return "STEAM";

                if (platform.Contains("META"))
                    return "META";

                if (platform.Contains("OCULUS"))
                    return "META";

                if (platform.Contains("QUEST"))
                    return "META";
            }
        }

        return "STEAM";
    }

    private int GetRefreshRate()
    {
        try
        {
            int hz = Screen.currentResolution.refreshRate;

            if (hz <= 0)
                return 90;

            return hz;
        }
        catch
        {
            return 90;
        }
    }

    private void SetNameText(object rig, string text)
    {
        string[] textMembers =
        {
            "playerText",
            "playerNameText",
            "nameText",
            "NameText"
        };

        foreach (string memberName in textMembers)
        {
            object textObject = GetMemberValue(rig, memberName);

            if (textObject == null)
                continue;

            if (SetTextValue(textObject, text))
            {
                CenterText(textObject);
                return;
            }
        }
    }

    private bool SetTextValue(object textObject, string text)
    {
        Type type = textObject.GetType();

        PropertyInfo property =
            type.GetProperty(
                "text",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null && property.CanWrite)
        {
            try
            {
                property.SetValue(textObject, text);
                return true;
            }
            catch
            {
            }
        }

        FieldInfo field =
            type.GetField(
                "text",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            try
            {
                field.SetValue(textObject, text);
                return true;
            }
            catch
            {
            }
        }

        return false;
    }

    private void CenterText(object textObject)
    {
        Type type = textObject.GetType();

        PropertyInfo property =
            type.GetProperty(
                "alignment",
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null && property.CanWrite)
        {
            try
            {
                if (property.PropertyType.IsEnum)
                {
                    object value =
                        Enum.Parse(
                            property.PropertyType,
                            "Center",
                            true
                        );

                    property.SetValue(textObject, value);
                }
            }
            catch
            {
            }
        }
    }

    private object GetMemberValue(object instance, string memberName)
    {
        if (instance == null)
            return null;

        Type type = instance.GetType();

        PropertyInfo property =
            type.GetProperty(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (property != null)
        {
            try
            {
                return property.GetValue(instance);
            }
            catch
            {
            }
        }

        FieldInfo field =
            type.GetField(
                memberName,
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic
            );

        if (field != null)
        {
            try
            {
                return field.GetValue(instance);
            }
            catch
            {
            }
        }

        return null;
    }

    private Type FindType(string typeName)
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                Type type = assembly.GetType(typeName);

                if (type != null)
                    return type;
            }
            catch
            {
            }
        }

        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            try
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.Name == typeName)
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
