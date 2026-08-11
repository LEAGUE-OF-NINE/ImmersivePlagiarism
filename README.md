# ImmersivePlagiarism (now open edition wow)

Allows the usage of vanilla Shaders in your custom motions projects (or bundles if you're feelin' spicy.)

# Steps for usage:

- Get the __data bundle of your target appearance (check #dev-resources for a list of prefabs)
- Put it in assetripper and export as project
- Import new files (into your unity project)
- Open the appearance prefab in the resources_moved folder
- Find the vfx you wish to use
- Turn the GameObject on and attach a blank custom script (called CorrectingCustomShader) to the particles that have materials. (Do this on every single one.)
- Make sure all Materials (under renderer) / Shaders are either None or attached to your target bundle that you will be building with.
- Use it as you would use it with any other VFX. Adjust positioning as needed.
