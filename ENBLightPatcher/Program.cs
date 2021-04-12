using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Noggog;
using System.Threading.Tasks;

namespace ENBLightPatcher
{
    public static class MyExtensions
    {
        public static bool ContainsInsensitive(this string str, string rhs)
        {
            return str.Contains(rhs, StringComparison.OrdinalIgnoreCase);
        }
    }

    public class Program
    {
        public static Task<int> Main(string[] args)
        {
            return SynthesisPipeline.Instance
                .AddPatch<ISkyrimMod, ISkyrimModGetter>(RunPatch)
                .SetTypicalOpen(GameRelease.SkyrimSE, "ENBLightPatcher.esp")
                .Run(args);
        }

        private static readonly string enbLightPluginNameWithExtension = "ENB Light.esp";

        private static readonly string[] lightNamesToAdjust = { "Candle", "Torch", "Camp" };

        private static readonly float fadeMultiplier = 0.5f;

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Part 1 - Patch every placed light in worldspaces/cells
            foreach (var placedObjectGetter in state.LoadOrder.PriorityOrder.PlacedObject().WinningContextOverrides(state.LinkCache))
            {
                if (placedObjectGetter.ModKey == enbLightPluginNameWithExtension) continue;
                var placedObject = placedObjectGetter.Record;
                if (placedObject.LightData == null) continue;
                placedObject.Base.TryResolve<ILightGetter>(state.LinkCache, out var placedObjectBase);
                if (placedObjectBase == null || placedObjectBase.EditorID == null) continue;
                if (lightNamesToAdjust.Any(placedObjectBase.EditorID.ContainsInsensitive))
                {
                    if (placedObject != null && placedObject.LightData != null && placedObject.LightData.FadeOffset > 0)
                    {
                        IPlacedObject modifiedObject = placedObjectGetter.GetOrAddAsOverride(state.PatchMod);
                        modifiedObject.LightData!.FadeOffset *= fadeMultiplier;
                    }
                }
                else continue;
            }

            // Part 2 - Patch every LIGH record
            foreach (var lightGetter in state.LoadOrder.PriorityOrder.Light().WinningContextOverrides())
            {
                if (lightGetter.ModKey == enbLightPluginNameWithExtension) continue;
                var light = lightGetter.Record;
                if (light.EditorID == null) continue;
                if (lightNamesToAdjust.Any(light.EditorID.ContainsInsensitive))
                {
                    Light modifiedLight = state.PatchMod.Lights.GetOrAddAsOverride(light);
                    modifiedLight.FadeValue *= fadeMultiplier;
                }
            }
        }
    }
}
