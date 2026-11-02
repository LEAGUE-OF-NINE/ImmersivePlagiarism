using System.Collections.Generic;
using UnityEngine;

namespace Bolus;
public static class ShaderRegistry
{
    private static readonly Dictionary<string, string> _aliases = new();
    private static readonly Dictionary<string, Shader> _resolved = new();
    private static readonly object _lock = new();

    public static void RegisterAlias(string materialShaderName, string targetShaderName)
    {
        lock (_lock) { _aliases[materialShaderName] = targetShaderName; }
        Main.log.LogInfo($"[ShaderRegistry] alias '{materialShaderName}' -> '{targetShaderName}'");
    }

    public static void RegisterShader(string materialShaderName, Shader shader)
    {
        if (shader == null) { Main.log.LogWarning($"[ShaderRegistry] null shader for '{materialShaderName}'"); return; }
        shader.hideFlags = HideFlags.DontUnloadUnusedAsset;
        lock (_lock) { _resolved[materialShaderName] = shader; }
        Main.log.LogInfo($"[ShaderRegistry] shader '{materialShaderName}' = '{shader.name}'");
    }

    public static void Prewarm()
    {
        string[] names;
        lock (_lock) { names = new List<string>(_aliases.Keys).ToArray(); }
        Main.log.LogInfo($"[ShaderRegistry] Prewarm: {names.Length} alias(es)");
        foreach (var n in names)
        {
            if (TryGet(n, out var s)) Main.log.LogInfo($"[ShaderRegistry] Prewarm OK {n} -> {s.name}");
            else Main.log.LogWarning($"[ShaderRegistry] Prewarm FAIL {n}");
        }
    }

    public static void RegisterSuffixed(string realShaderName, string suffix = "1") => RegisterAlias(realShaderName + suffix, realShaderName);

    public static bool TryGet(string materialShaderName, out Shader shader)
    {
        lock (_lock)
        {
            if (_resolved.TryGetValue(materialShaderName, out shader) && shader != null)
                return true;

            if (!_aliases.TryGetValue(materialShaderName, out var target))
            {
                shader = null;
                return false;
            }

            shader = Shader.Find(target);
            if (shader == null)
            {
                Main.log.LogWarning($"[ShaderRegistry] alias '{materialShaderName}' -> '{target}' not found by Shader.Find");
                return false;
            }

            shader.hideFlags = HideFlags.DontUnloadUnusedAsset;
            _resolved[materialShaderName] = shader;
            Main.log.LogInfo($"[ShaderRegistry] '{materialShaderName}' -> '{shader.name}'");
            return true;
        }
    }
}