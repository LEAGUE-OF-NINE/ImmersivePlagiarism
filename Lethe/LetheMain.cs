using BepInEx;
using BepInEx.Unity.IL2CPP;
using Il2CppInterop.Runtime.Injection;
using HarmonyLib;
using Il2CppSystem.IO;

namespace Bolus;

[BepInPlugin(GUID, NAME, VERSION)]
public class Main : BasePlugin
{
    public const string GUID = $"{AUTHOR}.{NAME}";
    public const string NAME = "ImmersivePlagiarism";
    public const string VERSION = "1.0.0";
    public const string AUTHOR = "ShaneAndDiesel";
    public override void Load()
    {
        Harmony harmony = new Harmony(VERSION);
        ClassInjector.RegisterTypeInIl2Cpp<CorrectingCustomShader>();
        harmony.PatchAll(typeof(ShaderSaver));
    }
}


/*
Clear Material AssetBundle Tags
The Culprit for Purple Meshes: Imported/ripped materials often hardcoded tags in the AssetBundle slot (e.g., bb7922...), so reassign to the main bundle. (Very fucking cool, right)
If a material has an explicit bundle tag different from your main prefab export, Unity strips the material reference, resulting in Hidden/InternalErrorShader (Instance).
The Fix: Always set the AssetBundle dropdown on all custom Material assets in your project panel to None (or to the bundle you're using) so they are bundled directly into your prefab's AssetBundle. 




If you are trying to add NEW shaders:
1. Create a new Shader property in ComponentScript.cs
2. In ShaderScript.cs use the "TryFindShader" method, and input the Shader name under the object.
3. In ComponentScript.cs, add a new else if at the bottom of the chain, then input the same args, only changing the Shader values (first two params) to be the new shader property you created.
The string in the function is only for debugging purposes, same with the gameobject name. You may uncomment the logging of errors if you notice that the shader is not being applied.
*/
