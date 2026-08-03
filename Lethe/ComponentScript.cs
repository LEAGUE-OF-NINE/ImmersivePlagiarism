using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CorrectingCustomShader : MonoBehaviour
{
    public static Shader main_custom;
    public static Shader main_custom_pub;
    public static Shader compact_shader;
    public static Shader colormask;
    public static Shader colormask_n_compact;
    public static Shader colormask_noise_random;
    public static Shader main_custom_distortion;
    public static Shader pub_distortion_master;
    const string easteregg = "Lookin' for something?";
    const string shaneandbolus = "We found it :D";

    public static Dictionary<string, Shader> allShaders = new();

    public void Awake()
    {
        ParticleSystemRenderer renderer = base.gameObject.GetComponent<ParticleSystemRenderer>();

        if (renderer == null)
        {
            return;
        }

        Material[] materials = renderer.materials;

        if (materials == null || materials.Length == 0)
        {
            Debug.LogWarning($"[CorrectingCustomShader] No materials found in array on '{base.gameObject.name}'.");
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null || mat.shader == null) continue;

            Shader currentShader = mat.shader;
            string currentShaderName = currentShader.name;

           // Debug.Log($"[CorrectingCustomShader] Inspecting '{base.gameObject.name}' [Mat Index {i}] | Material: '{mat.name}' | Current Shader: '{currentShaderName}'");

            if (currentShaderName == "Hidden/InternalErrorShader")
            {
                Debug.LogError($"[CorrectingCustomShader] '{base.gameObject.name}' loaded with 'Hidden/InternalErrorShader'. The AssetBundle failed to pack the dummy shader!");
                continue;
            }

            // Swap Logic applied directly to the material instance in the array
            if (currentShaderName == "Fx_Team/Fx_Grp_Compact_Shader")
            {
                ApplyShaderToMat(mat, compact_shader, "compact_shader", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Shader")
            {
                ApplyShaderToMat(mat, main_custom, "main_custom", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Pub_Shader")
            {
                ApplyShaderToMat(mat, main_custom_pub, "main_custom_pub", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader")
            {
                ApplyShaderToMat(mat, colormask, "colormask", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_N_Compact")
            {
                ApplyShaderToMat(mat, colormask_n_compact, "colormask_n_compact", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_Noise_Random_Compact")
            {
                ApplyShaderToMat(mat, colormask_noise_random, "colormask_noise_random", base.gameObject.name);
            }
            else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Distortion_Shader")
            {
                ApplyShaderToMat(mat, main_custom_distortion, "main_custom_distortion", base.gameObject.name);
            }
            else if (currentShaderName == "Shader Graphs/FX_S_Public_DistortionMaster1_N")
            {
                ApplyShaderToMat(mat, pub_distortion_master, "pub_distortion_master", base.gameObject.name);
            }
            else
            {
                Debug.LogWarning($"[CorrectingCustomShader] Unrecognized dummy shader '{currentShaderName}' on '{base.gameObject.name}'.");
            }
        }
    }

    private void ApplyShaderToMat(Material mat, Shader targetShader, string shaderVarName, string objName)
    {
        if (targetShader != null)
        {
            mat.shader = targetShader;
           // Debug.Log($"[CorrectingCustomShader] [SWAP SUCCESS] Applied '{targetShader.name}' ({shaderVarName}) to material '{mat.name}' on '{objName}'!");
        }
        else
        {
            Debug.LogError($"[CorrectingCustomShader] [SWAP FAILED] Cannot swap on '{objName}': Static variable '{shaderVarName}' is NULL!");
        }
    }
}

/*
Clear Material AssetBundle Tags
The Culprit for Purple Meshes: Imported/ripped materials often hardcoded tags in the AssetBundle slot (e.g., bb7922...), so reassign to the main bundle. (Very fucking cool, right)
If a material has an explicit bundle tag different from your main prefab export, Unity strips the material reference, resulting in Hidden/InternalErrorShader (Instance).
The Fix: Always set the AssetBundle dropdown on all custom Material assets in your project panel to None (or to the bundle you're using) so they are bundled directly into your prefab's AssetBundle. 


Support sharedMaterials Arrays for Mesh Particles
Particle Systems operating in Mesh Mode handle materials differently in IL2CPP builds than Billboard Mode particles.
Billboard Mode: Accessing renderer.material works directly.
Mesh Mode: Accessing renderer.material can return null. You must iterate through renderer.materials (plural/array) to safely access and swap every material instance.

Working Implementation:



using HarmonyLib;
using MainUI;
using UnityEngine;

namespace Bolus
{
    public static class ShaderSaver
    {
        [HarmonyPatch(typeof(LoginSceneManager), "Start")]
        [HarmonyPrefix]
        public static void LoginSceneManager_Prefix()
        {
            CorrectingCustomShader.main_custom = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Shader");
            CorrectingCustomShader.main_custom_pub = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Pub_Shader");
            CorrectingCustomShader.compact_shader = TryFindShader("Fx_Team/Fx_Grp_Compact_Shader");
            CorrectingCustomShader.colormask = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader");
            CorrectingCustomShader.colormask_n_compact = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader_N_Compact");
            CorrectingCustomShader.colormask_noise_random = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader_Noise_Random");
            CorrectingCustomShader.main_custom_distortion = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Distortion_Shader");
        }

        private static Shader TryFindShader(string shaderName)
        {
            Shader foundShader = Shader.Find(shaderName);
            if (foundShader != null)
            {
                foundShader.hideFlags = HideFlags.DontUnloadUnusedAsset;
            }
            return foundShader;
        }
    }
}


Working Component:
using UnityEngine;

public class CorrectingCustomShader : MonoBehaviour
{
    public static Shader main_custom;
    public static Shader main_custom_pub;
    public static Shader compact_shader;
    public static Shader colormask;
    public static Shader colormask_n_compact;
    public static Shader colormask_noise_random;
    public static Shader main_custom_distortion;

    public void Awake()
    {
        ParticleSystemRenderer renderer = base.gameObject.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) return;

        // Fetching renderer.materials handles both Mesh Mode and Billboard Mode
        Material[] materials = renderer.materials;
        if (materials == null || materials.Length == 0) return;

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null || mat.shader == null) continue;

            string shaderName = mat.shader.name;

            if (shaderName == "Fx_Team/Fx_Grp_Compact_Shader1")
                ApplyShader(mat, compact_shader);
            else if (shaderName == "Fx_Team/Fx_Grp_MainCustom_Shader1")
                ApplyShader(mat, main_custom);
            else if (shaderName == "Fx_Team/Fx_Grp_MainCustom_Pub_Shader1")
                ApplyShader(mat, main_custom_pub);
            else if (shaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader1")
                ApplyShader(mat, colormask);
            else if (shaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_N_Compact1")
                ApplyShader(mat, colormask_n_compact);
            else if (shaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_Noise_Random_Compact1")
                ApplyShader(mat, colormask_noise_random);
            else if (shaderName == "Fx_Team/Fx_Grp_MainCustom_Distortion_Shader1")
                ApplyShader(mat, main_custom_distortion);
        }
    }

    private void ApplyShader(Material mat, Shader targetShader)
    {
        if (targetShader != null)
        {
            mat.shader = targetShader;
        }
    }
}

*/