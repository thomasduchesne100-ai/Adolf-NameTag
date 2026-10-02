using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "1.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private readonly HashSet<VRRig> processedRigs = new HashSet<VRRig>();

    private void Awake()
    {
        Logger.LogInfo("Adolf NameTag chargé.");
    }

    private void LateUpdate()
    {
        if (GorillaParent.instance == null)
            return;

        if (GorillaParent.instance.vrrigs == null)
            return;

        foreach (VRRig rig in GorillaParent.instance.vrrigs)
        {
            if (rig == null)
                continue;

            // Ne touche jamais au rig local.
            if (rig.isOfflineVRRig)
                continue;

            if (rig.isMyPlayer)
                continue;

            UpdateRigName(rig);
        }
    }

    private void UpdateRigName(VRRig rig)
    {
        try
        {
            // Récupère le champ playerText sans nécessiter
            // directement UnityEngine.UI.dll.
            FieldInfo playerTextField =
                typeof(VRRig).GetField(
                    "playerText",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance
                );

            if (playerTextField == null)
                return;

            object playerText = playerTextField.GetValue(rig);

            if (playerText == null)
                return;

            Type textType = playerText.GetType();

            // Récupère la propriété "text".
            PropertyInfo textProperty =
                textType.GetProperty("text");

            if (textProperty == null)
                return;

            string playerName = GetPlayerName(rig);

            if (string.IsNullOrEmpty(playerName))
                return;

            // Met le vrai nom du joueur.
            textProperty.SetValue(playerText, playerName);

            // Active uniquement le texte déjà présent dans le VRRig.
            Component component = playerText as Component;

            if (component != null)
            {
                if (!component.gameObject.activeSelf)
                    component.gameObject.SetActive(true);
            }

            processedRigs.Add(rig);
        }
        catch (Exception ex)
        {
            Logger.LogError(
                "Erreur NameTag : " + ex.Message
            );
        }
    }

    private string GetPlayerName(VRRig rig)
    {
        try
        {
            FieldInfo nameField =
                typeof(VRRig).GetField(
                    "playerName",
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance
                );

            if (nameField != null)
            {
                object value = nameField.GetValue(rig);

                if (value != null)
                    return value.ToString();
            }
        }
        catch
        {
        }

        return rig.name;
    }

    private void OnDestroy()
    {
        processedRigs.Clear();
    }
}
