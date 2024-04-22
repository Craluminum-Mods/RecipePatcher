global using static RecipePatcher.Core;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Common;
using Vintagestory.ServerMods;

[assembly: ModInfo(name: "Recipe Patcher", modID: "recipepatcher", Side = "Server")]

namespace RecipePatcher;

public class Core : ModSystem
{
    public const string LogPrefix = "[Recipe Patcher]"; 

    private Harmony HarmonyInstance => new Harmony(Mod.Info.ModID);

    public override void Start(ICoreAPI api)
    {
        HarmonyInstance.Patch(original: LoadRecipe_Original(), prefix: LoadRecipe_Patch());

        api.Logger.Event("started '{0}' mod", Mod.Info.Name);
    }

    public override void Dispose()
    {
        HarmonyInstance.Unpatch(original: LoadRecipe_Original(), HarmonyPatchType.All, HarmonyInstance.Id);
    }

    MethodInfo LoadRecipe_Original() => typeof(GridRecipeLoader).GetMethod(nameof(GridRecipeLoader.LoadRecipe));
    MethodInfo LoadRecipe_Patch() => typeof(GridRecipeLoader_LoadRecipe_Patch).GetMethod(nameof(GridRecipeLoader_LoadRecipe_Patch.Prefix));
}