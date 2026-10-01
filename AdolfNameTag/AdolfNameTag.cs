using BepInEx;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;

[BepInPlugin("com.adolf.nametag", "Adolf NameTag", "1.0.0")]
public class AdolfNameTag : BaseUnityPlugin
{
    private readonly Dictionary<GameObject, TextMesh> nameTags = new Dictionary<GameObject, TextMesh>();

    private float nextScanTime = 0f;

    private void Start()
    {
        Logger.LogInfo("Adolf NameTag chargé !");
    }

    private void Update()
    {
        if (Time.time >= nextScanTime)
        {
            nextScanTime = Time.time + 1f;
            FindPlayers();
        }

        UpdateNameTags();
    }

    private void FindPlayers()
    {
        GameObject[] objects;

        try
        {
            objects = FindObjectsOfType<GameObject>();
        }
        catch
        {
            return;
        }

        foreach (GameObject obj in objects)
        {
            if (obj == null)
                continue;

            if (nameTags.ContainsKey(obj))
                continue;

            if (!LooksLikePlayer(obj))
                continue;

            CreateNameTag(obj);
        }
    }

    private bool LooksLikePlayer(GameObject obj)
    {
        string name = obj.name.ToLower();

        return
            name.Contains("gorillaplayer") ||
            name.Contains("player") ||
            name.Contains("vrig") ||
            name.Contains("rig");
    }

    private void CreateNameTag(GameObject player)
    {
        try
        {
            GameObject tagObject = new GameObject("AdolfNameTag");

            tagObject.transform.SetParent(player.transform, false);
            tagObject.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            tagObject.transform.localRotation = Quaternion.identity;
            tagObject.transform.localScale = Vector3.one * 0.01f;

            TextMesh text = tagObject.AddComponent<TextMesh>();

            text.text = GetPlayerName(player);
            text.fontSize = 100;
            text.characterSize = 0.1f;
            text.anchor = TextAnchor.MiddleCenter;
            text.alignment = TextAlignment.Center;

            Renderer renderer = text.GetComponent<Renderer>();

            if (renderer != null)
            {
                renderer.material.color = Color.white;
            }

            nameTags.Add(player, text);
        }
        catch (Exception ex)
        {
            Logger.LogError("Erreur NameTag : " + ex.Message);
        }
    }

    private void UpdateNameTags()
    {
        foreach (KeyValuePair<GameObject, TextMesh> pair in nameTags)
        {
            GameObject player = pair.Key;
            TextMesh text = pair.Value;

            if (player == null || text == null)
                continue;

            text.text = GetPlayerName(player);

            Camera cam = Camera.main;

            if (cam != null)
            {
                text.transform.LookAt(cam.transform);

                text.transform.Rotate(0f, 180f, 0f);
            }
        }
    }

    private string GetPlayerName(GameObject player)
    {
        try
        {
            Component photonView = FindPhotonView(player);

            if (photonView != null)
            {
                PropertyInfo ownerProperty =
                    photonView.GetType().GetProperty("Owner");

                if (ownerProperty != null)
                {
                    object owner = ownerProperty.GetValue(photonView);

                    if (owner != null)
                    {
                        PropertyInfo nickNameProperty =
                            owner.GetType().GetProperty("NickName");

                        if (nickNameProperty != null)
                        {
                            object nickname =
                                nickNameProperty.GetValue(owner);

                            if (nickname != null &&
                                !string.IsNullOrEmpty(nickname.ToString()))
                            {
                                return nickname.ToString();
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Si Photon n'est pas accessible, on utilise le nom de l'objet.
        }

        return player.name;
    }

    private Component FindPhotonView(GameObject player)
    {
        Component[] components = player.GetComponents<Component>();

        foreach (Component component in components)
        {
            if (component == null)
                continue;

            string typeName = component.GetType().Name;

            if (typeName == "PhotonView")
            {
                return component;
            }
        }

        return null;
    }

    private void OnDestroy()
    {
        foreach (TextMesh tag in nameTags.Values)
        {
            if (tag != null)
            {
                Destroy(tag.gameObject);
            }
        }

        nameTags.Clear();
    }
}
