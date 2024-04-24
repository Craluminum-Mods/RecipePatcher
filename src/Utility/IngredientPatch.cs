using Newtonsoft.Json;
using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;

namespace RecipePatcher;

public class IngredientPatch
{
    public string IngredientCode = null;
    public string Code = null;
    public string Name = null;
    public string[] AllowedVariants = null;
    public string[] SkipVariants = null;

    [JsonProperty]
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject Attributes = null;

    [JsonProperty]
    [JsonConverter(typeof(JsonAttributesConverter))]
    public JsonObject AttributesNew = null;

    public AssetLocation GetIngredientCode() => new AssetLocation(IngredientCode);
    public AssetLocation GetCode() => string.IsNullOrEmpty(Code) ? GetIngredientCode() : new AssetLocation(Code);

    public bool MatchesIngredient(CraftingRecipeIngredient ingredient)
    {
        if (Attributes != null && (Attributes.ToString() != ingredient.Attributes?.ToString()))
        {
            return false;
        }

        return WildcardUtil.Match(GetIngredientCode(), ingredient.Code);
    }
}