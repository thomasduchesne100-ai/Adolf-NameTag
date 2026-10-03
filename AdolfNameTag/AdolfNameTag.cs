private void Start()
{
    Logger.LogInfo("Adolf NameTag 5.0.1 démarre.");
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
            tag.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

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
            string playerName = GetName(rig);
            string platform = GetPlatform(rig);

            int hz = Screen.currentResolution.refreshRate;

            if (hz <= 0)
                hz = 90;

            mesh.text = hz + " Hz\n" + platform + " • " + playerName;
        }

        Camera cam = Camera.main;

        if (cam != null)
        {
            Vector3 direction = tag.transform.position - cam.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                tag.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            }
        }
    }
}

private Transform FindHead(Transform root)
{
    Transform[] children = root.GetComponentsInChildren<Transform>(true);

    foreach (Transform child in children)
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

    foreach (string member in names)
    {
        object value = GetMember(rig, member);

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
