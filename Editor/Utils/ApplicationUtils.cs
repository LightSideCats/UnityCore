using UnityEngine;

public static class ApplicationUtils
{
    public static string ProjectPath => Application.dataPath.Replace("Assets", string.Empty);
}
