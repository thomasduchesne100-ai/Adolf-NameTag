using System;
using System.Collections;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "3.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private Type vrRigType;

    private void Start()
    {
        Logger.LogInfo("Adolf NameTag démarre.");
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
            yield return new WaitForSeconds(1f);
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
