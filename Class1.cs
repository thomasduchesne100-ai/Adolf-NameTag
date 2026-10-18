using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace NameTagsMod
{
    [BepInPlugin("com.tonnom.nametags", "NameTags", "2.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Dictionary<VRRig, TextMeshPro> tags = new Dictionary<VRRig, TextMeshPro>();
        private readonly List<VRRig> toRemove = new List<VRRig>();
        private VRRig[] rigs = new VRRig[0];
        private float nextRefresh;
        private string fpsColor;
        private const float HeightOffset = 0.55f;
        private const float TagScale = 0.12f;

        private void Awake()
        {
            Logger.LogInfo("NameTags 2.0.0 charge !");
        }

        private void LateUpdate()
        {
            if (!PhotonNetwork.InRoom)
            {
                ClearAll();
                return;
            }

            Camera cam = Camera.main;
            if (cam == null) return;

            if (Time.time >= nextRefresh)
            {
                rigs = FindObjectsOfType<VRRig>();
                nextRefresh = Time.time + 1f;
            }

            HashSet<VRRig> alive = new HashSet<VRRig>();

            foreach (VRRig rig in rigs)
            {
                if (rig == null || rig.isOfflineVRRig) continue;
                alive.Add(rig);

                TextMeshPro tmp;
                if (!tags.TryGetValue(rig, out tmp) || tmp == null)
                {
                    tmp = CreateTag();
                    tags[rig] = tmp;
                }

                string name = rig.OwningNetPlayer != null ? rig.OwningNetPlayer.NickName : "???";
                int fps = GetInt(rig, "fps");
                Color platformColor;
                string platform = GetPlatform(rig, out platformColor);
                string hex = ColorUtility.ToHtmlStringRGB(platformColor);


                string fpsColor = fps < 40 ? "#FF3333" :
                                  fps < 60 ? "#FFA500" :
                                  "#00FF00";


                tmp.text = "<color=" + fpsColor + ">" + fps + " Hz</color>\n"
                         + name + " <color=#" + hex + ">• " + platform
                         + "</color>";



                Vector3 headPos = rig.headMesh != null ? rig.headMesh.transform.position : rig.transform.position;
                tmp.transform.position = headPos + Vector3.up * HeightOffset;
                tmp.transform.rotation = Quaternion.LookRotation(tmp.transform.position - cam.transform.position);
            }

            toRemove.Clear();
            foreach (KeyValuePair<VRRig, TextMeshPro> pair in tags)
            {
                if (pair.Key == null || !alive.Contains(pair.Key))
                {
                    if (pair.Value != null) Destroy(pair.Value.gameObject);
                    toRemove.Add(pair.Key);
                }
            }
            foreach (VRRig rig in toRemove) tags.Remove(rig);
        }

        private TextMeshPro CreateTag()
        {
            GameObject go = new GameObject("NameTag");
            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontSize = 10f;
            tmp.color = Color.white;
            tmp.enableWordWrapping = false;
            tmp.richText = true;
            tmp.fontStyle = FontStyles.Bold;
            go.transform.localScale = Vector3.one * TagScale;
            return tmp;
        }

        // ---- Lecture par reflexion (compile meme si le champ change) ----
        private static object GetMember(object obj, string name)
        {
            if (obj == null) return null;
            BindingFlags f = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            Type t = obj.GetType();
            FieldInfo fi = t.GetField(name, f);
            if (fi != null) return fi.GetValue(obj);
            PropertyInfo pi = t.GetProperty(name, f);
            if (pi != null) return pi.GetValue(obj, null);
            return null;
        }

        private static int GetInt(object obj, string name)
        {
            object v = GetMember(obj, name);
            return v is int ? (int)v : 0;
        }

        // Le jeu donne directement la plateforme : OwningNetPlayer.Platform
        // (copie aussi dans les CustomProperties Photon, cle "platform")
        private string GetPlatform(VRRig rig, out Color color)
        {
            string p = null;

            if (rig.OwningNetPlayer != null)
            {
                p = GetMember(rig.OwningNetPlayer, "Platform") as string;

                if (string.IsNullOrEmpty(p))
                {
                    Photon.Realtime.Player pr = GetMember(rig.OwningNetPlayer, "PlayerRef") as Photon.Realtime.Player;
                    if (pr != null && pr.CustomProperties != null && pr.CustomProperties.ContainsKey("platform"))
                        p = pr.CustomProperties["platform"] as string;
                }
            }

            if (string.IsNullOrEmpty(p))
            {
                color = Color.gray;
                return "?";
            }

            string low = p.ToLowerInvariant();
            if (low.Contains("steam")) color = new Color(0.3f, 0.6f, 1f);
            else if (low.Contains("quest")) color = new Color(1f, 0.6f, 0.2f);
            else color = new Color(0.4f, 1f, 0.4f); // Oculus PC / autre

            return p;
        }

        private void ClearAll()
        {
            if (tags.Count == 0) return;
            foreach (KeyValuePair<VRRig, TextMeshPro> pair in tags)
                if (pair.Value != null) Destroy(pair.Value.gameObject);
            tags.Clear();
        }

        private void OnDestroy()
        {
            ClearAll();
        }
    }
}
