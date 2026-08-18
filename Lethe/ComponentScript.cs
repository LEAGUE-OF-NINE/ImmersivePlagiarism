using System;
using System.IO;
using HarmonyLib;
using MainUI;
using UnityEngine;

namespace Bolus
{
    public static class ShaderSaver
    {
        [HarmonyPatch(typeof(LoginSceneManager), "Start")]
        [HarmonyPrefix]
        public static void LoginSceneManager_()
        {
            Main.log.LogInfo("[ShaderSaver] LoginSceneManager.Start patch running...");

            // below are all common shaders.
            CorrectingCustomShader.main_custom = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Shader");
            CorrectingCustomShader.main_custom_pub = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Pub_Shader");
            CorrectingCustomShader.compact_shader = TryFindShader("Fx_Team/Fx_Grp_Compact_Shader");
            CorrectingCustomShader.colormask = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader");
            CorrectingCustomShader.colormask_n_compact = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader_N_Compact");
            CorrectingCustomShader.colormask_noise_random = TryFindShader("Fx_Team/Fx_Grp_Outline_ColorMask_Shader_Noise_Random");
            CorrectingCustomShader.main_custom_distortion = TryFindShader("Fx_Team/Fx_Grp_MainCustom_Distortion_Shader");
            CorrectingCustomShader.pub_distortion_master = TryFindShader("Shader Graphs/FX_S_Public_DistortionMaster1_N");

            Main.log.LogInfo("[ShaderSaver] Shader caching complete.");
        }

        /*
        [HarmonyPatch(typeof(BattleObjectManager), nameof(BattleObjectManager.OnStageStart_Model))]
        [HarmonyPostfix]
        public static void BattleObjectManager_(BattleObjectManager __instance)
        {
            Main.log.LogInfo("[ShaderSaver] BattleObjectManager.OnStageStart_Model patch running...");
            Main.log.LogInfo("[ShaderSaver] Point of " + __instance.Pointer);
            // below are all common shaders that cant be loaded during login scene

            Main.log.LogInfo("[ShaderSaver] Shader caching complete.");
        }
        */
        private static Shader TryFindShader(string shaderName)
        {
            Shader foundShader = Shader.Find(shaderName);

            if (foundShader != null)
            {
                foundShader.hideFlags = HideFlags.DontUnloadUnusedAsset;
               // Debug.Log($"[ShaderSaver] [SUCCESS] Found & Cached: '{shaderName}'");
            }
            else
            {
                Main.log.LogError($"[ShaderSaver] [FAILED] Could not find shader in game memory: '{shaderName}'!");
            }

            return foundShader;
        }
    }
}
