using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "3.1.0")]
public class AdolfNameTag : BaseUnityPlugin
{
private Type vrRigType;
private float nextScanTime;

```
private void Awake()
{
    Logger.LogInfo("Adolf NameTag 3.1.0 chargé.");
    StartCoroutine(FindVRRig());
}

private IEnumerator FindVRRig()
{
    yield return new WaitForSeconds(2f);

    vrRigType = FindType("VRRig");

    if (vrRigType == null)
    {
        Logger.LogWarning("VRRig introuvable.");
        yield break;
    }

    Logger.LogInfo("VRRig trouvé : " + vrRigType.FullName);
}

private void Update()
{
    if (vrRigType == null)
        return;

    if (Time.unscaledTime < nextScanTime)
        return;

    nextScanTime = Time.unscaledTime + 1f;

    try
    {
        UnityEngine.Object[] rigs =
            Resources.FindObjectsOfTypeAll(vrRigType);

        foreach (UnityEngine.Object obj in rigs)
        {
            Component rig = obj as Component;

            if (rig == null)
                continue;

            GameObject go = rig.gameObject;

            if (go == null)
                continue;

            if (!go.activeInHierarchy)
                continue;

            if (!go.scene.IsValid())
                continue;

            UpdateRig(rig);
        }
    }
    catch (Exception ex)
    {
        Logger.LogError("Erreur NameTag : " + ex.Message);
    }
}

private void UpdateRig(Component rig)
{
    string playerName = GetStringValue(
        rig,
        "playerName",
        "nickName",
        "nickname",
        "defaultName",
        "creatorUsername"
    );

    if (string.IsNullOrWhiteSpace(playerName))
        return;

    object playerText = GetValue(
        rig,
        "playerText",
        "playerNameText"
    );

    if (playerText == null)
        return;

    string platform = GetPlatform(rig);
    int hz = GetRefreshRate();

    // FORMAT :
    //
    //        190 Hz
    // STEAM • PlayerName

    string newText =
        hz + " Hz\n" +
        platform + " • " + playerName;

    if (TrySetText(playerText, newText))
    {
        TryCenterText(playerText);
    }
}

private string GetPlatform(Component rig)
{
    object value = GetValue(
        rig,
        "platformTag",
        "platformType",
        "Player_Platform"
    );

    if (value != null)
    {
        string text = value.ToString();

        if (text.IndexOf(
            "Steam",
            StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "STEAM";
        }

        if (text.IndexOf(
            "Quest",
            StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "META";
        }

        if (text.IndexOf(
            "Meta",
            StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return "META";
        }
    }

    return "PC";
}

private int GetRefreshRate()
{
    try
    {
        int rate = Screen.currentResolution.refreshRate;

        if (rate > 0)
            return rate;
    }
    catch
    {
    }

    return 90;
}

private static Type FindType(string typeName)
{
    Type type =
        Type.GetType(typeName + ", Assembly-CSharp");

    if (type != null)
        return type;

    foreach (Assembly assembly
             in AppDomain.CurrentDomain.GetAssemblies())
    {
        try
        {
            type = assembly.GetType(typeName);

            if (type != null)
                return type;

            type = assembly.GetType(
                "GorillaLocomotion." + typeName
            );

            if (type != null)
                return type;
        }
        catch
        {
        }
    }

    return null;
}

private static object GetValue(
    object obj,
    params string[] names)
{
    if (obj == null)
        return null;

    Type type = obj.GetType();

    const BindingFlags flags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static;

    foreach (string name in names)
    {
        Type current = type;

        while (current != null)
        {
            try
            {
                FieldInfo field =
                    current.GetField(name, flags);

                if (field != null)
                    return field.GetValue(obj);

                PropertyInfo property =
                    current.GetProperty(name, flags);

                if (property != null &&
                    property.CanRead)
                {
                    return property.GetValue(obj);
                }
            }
            catch
            {
            }

            current = current.BaseType;
        }
    }

    return null;
}

private static string GetStringValue(
    object obj,
    params string[] names)
{
    object value = GetValue(obj, names);

    if (value == null)
        return null;

    return value.ToString();
}

private static bool TrySetText(
    object textObject,
    string value)
{
    if (textObject == null)
        return false;

    try
    {
        Type type = textObject.GetType();

        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        PropertyInfo textProperty =
            type.GetProperty("text", flags);

        if (textProperty != null &&
            textProperty.CanWrite &&
            textProperty.PropertyType == typeof(string))
        {
            textProperty.SetValue(
                textObject,
                value
            );

            return true;
        }

        FieldInfo textField =
            type.GetField("text", flags);

        if (textField != null &&
            textField.FieldType == typeof(string))
        {
            textField.SetValue(
                textObject,
                value
            );

            return true;
        }

        GameObject go = textObject as GameObject;

        if (go != null)
        {
            Component[] components =
                go.GetComponentsInChildren<Component>(true);

            foreach (Component component in components)
            {
                if (component == null)
                    continue;

                if (TrySetText(component, value))
                    return true;
            }
        }
    }
    catch
    {
    }

    return false;
}

private static void TryCenterText(
    object textObject)
{
    if (textObject == null)
        return;

    try
    {
        Type type = textObject.GetType();

        const BindingFlags flags =
            BindingFlags.Public |
            BindingFlags.NonPublic |
            BindingFlags.Instance;

        PropertyInfo alignmentProperty =
            type.GetProperty(
                "alignment",
                flags
            );

        if (alignmentProperty != null &&
            alignmentProperty.CanWrite &&
            alignmentProperty.PropertyType.IsEnum)
        {
            try
            {
                object center =
                    Enum.Parse(
                        alignmentProperty.PropertyType,
                        "Center"
                    );

                alignmentProperty.SetValue(
                    textObject,
                    center
                );
            }
            catch
            {
            }
        }

        FieldInfo alignmentField =
            type.GetField(
                "alignment",
                flags
            );

        if (alignmentField != null &&
            alignmentField.FieldType.IsEnum)
        {
            try
            {
                object center =
                    Enum.Parse(
                        alignmentField.FieldType,
                        "Center"
                    );

                alignmentField.SetValue(
                    textObject,
                    center
                );
            }
            catch
            {
            }
        }
    }
    catch
    {
    }
}
```

}
