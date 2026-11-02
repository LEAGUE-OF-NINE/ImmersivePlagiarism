using Bolus;
using System;
using UnityEngine;

public class CorrectingCustomShader : MonoBehaviour
{
    public static Shader main_custom;
    public static Shader main_custom_pub;
    public static Shader compact_shader;
    public static Shader colormask;
    public static Shader noise_random_compact;
    public static Shader colormask_n_compact;
    public static Shader colormask_noise_random;
    public static Shader main_custom_distortion;
    public static Shader pub_distortion_master;
    public static Shader FX_Mat_0_Master_Dissolve_Distortion;

    public void Awake()
    {
        Main.log.LogInfo($"[CorrectingCustomShader] Awake on '{base.gameObject.name}'");
        foreach (var r in base.gameObject.GetComponentsInChildren<Renderer>(true))
        {
            var mats = r.materials;
            if (mats == null) continue;
            for (int i = 0; i < mats.Length; i++)
                ProcessMaterial(mats[i]);
        }
    }

    private void ProcessMaterial(Material mat)
    {
        // yes this code sucks but im lazy
        Main.log.LogInfo($"[CorrectingCustomShader] mat '{mat.name}' shader='{mat.shader?.name ?? "<null>"}' on '{base.gameObject.name}'");
        if (mat == null || mat.shader == null) return;

        string currentShaderName = mat.shader.name;

        if (currentShaderName == "Hidden/InternalErrorShader") return;

        if (currentShaderName == "Fx_Team/Fx_Grp_Compact_Shader")
        {
            ApplyShaderToMat(mat, compact_shader, "compact_shader");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Shader")
        {
            ApplyShaderToMat(mat, main_custom, "main_custom");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Pub_Shader")
        {
            ApplyShaderToMat(mat, main_custom_pub, "main_custom_pub");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader")
        {
            ApplyShaderToMat(mat, colormask, "colormask");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_N_Compact")
        {
            ApplyShaderToMat(mat, colormask_n_compact, "colormask_n_compact");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_Outline_ColorMask_Shader_Noise_Random_Compact")
        {
            ApplyShaderToMat(mat, colormask_noise_random, "colormask_noise_random");
        }
        else if (currentShaderName == "Fx_Team/Fx_Grp_MainCustom_Distortion_Shader")
        {
            ApplyShaderToMat(mat, main_custom_distortion, "main_custom_distortion");
        }
        else if (currentShaderName == "Shader Graphs/FX_S_Public_DistortionMaster1_N")
        {
            ApplyShaderToMat(mat, pub_distortion_master, "pub_distortion_master");
        }
        else if (currentShaderName == "Shader Graphs/FX_Mat_0_Master_Dissolve_Distortion")
        {
            ApplyShaderToMat(mat, FX_Mat_0_Master_Dissolve_Distortion, "master_dissolve_distortion");
        }
        else if (ShaderRegistry.TryGet(currentShaderName, out var registered))
        {
            mat.shader = registered;
        }
    }

    private void ApplyShaderToMat(Material mat, Shader targetShader, string shaderVarName)
    {
        if (targetShader != null)
        {
            mat.shader = targetShader;
        }
        else
        {
            Main.log.LogError($"[CorrectingCustomShader] [SWAP FAILED] Cannot swap on '{gameObject.name}': Static variable '{shaderVarName}' is NULL!");
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
