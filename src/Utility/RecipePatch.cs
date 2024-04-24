using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.ServerMods.NoObf;

namespace RecipePatcher;

public class RecipePatch
{
    public bool Enabled = true;

    public EnumRecipePatchType Type;
    public PatchCondition[] Conditions = null;
    public PatchModDependence[] DependsOn = null;

    public bool MatchOutputQuantity = false;
    public bool ChangeQuantity = false;
    public int Quantity = 0;
    public int QuantityNew = 0;

    public bool MatchAttributes = false;
    public bool ChangeAttributes = false;

    [JsonProperty]
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject Attributes = null;

    [JsonProperty]
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject AttributesNew = null;

    public string OutputCode = null;
    public string OutputCodeNew = null;
    public IngredientPatch[] Ingredients = null;

    public int? RecipeGroup;

    public AssetLocation GetOutputCode() => new AssetLocation(OutputCode);
    public AssetLocation GetOutputCodeNew() => new AssetLocation(OutputCodeNew);

    public bool MatchesOutput(GridRecipe recipe)
    {
        if (Attributes != null && (Attributes.ToString() != recipe.Output.Attributes?.ToString()))
        {
            return false;
        }

        if (MatchOutputQuantity && recipe.Output.Quantity != Quantity)
        {
            return false;
        }

        return WildcardUtil.Match(GetOutputCode(), recipe.Output.Code);
    }

    public bool CanApply(ICoreAPI api, int patchIndex, AssetLocation location, ITreeAttribute worldConfig, HashSet<string> loadedModIds)
    {
        if (!Enabled)
        {
            return false;
        }

        if (Conditions != null)
        {
            foreach (PatchCondition condition in Conditions)
            {
                IAttribute attr = worldConfig[condition.When];
                if (attr == null)
                {
                    api.Logger.VerboseDebug($"{LogPrefix} Recipe patch file {{0}}, patch {{1}}: Unmet IsValue condition ({{2}}!={{3}})", location, patchIndex, condition.IsValue, null);
                    return false;
                }
                else if (!condition.IsValue.Equals(attr.GetValue()?.ToString() ?? "", StringComparison.InvariantCultureIgnoreCase))
                {
                    api.Logger.VerboseDebug($"{LogPrefix} Recipe patch file {{0}}, patch {{1}}: Unmet IsValue condition ({{2}}!={{3}})", location, patchIndex, condition.IsValue, attr.GetValue()?.ToString() ?? "");
                    return false;
                }
            }
        }

        if (DependsOn != null)
        {
            bool enabled = true;
            foreach (PatchModDependence dependence in DependsOn)
            {
                bool loaded = loadedModIds.Contains(dependence.modid);
                enabled = enabled && (loaded ^ dependence.invert);
            }
            if (!enabled)
            {
                string conditions = string.Join(",", DependsOn.Select((PatchModDependence pd) => (pd.invert ? "!" : "") + pd.modid));
                api.Logger.VerboseDebug($"{LogPrefix} Recipe patch file {{0}}, patch {{1}}: Unmet DependsOn condition ({{2}})", location, patchIndex, conditions);
                return false;
            }
        }

        return true;
    }
}