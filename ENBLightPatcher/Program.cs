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

        public static void RunPatch(IPatcherState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Part 1 - Patch every placed light in worldspaces/cells
            foreach (var placedObjectGetter in state.LoadOrder.PriorityOrder.PlacedObject().WinningContextOverrides(state.LinkCache))
            {
                var placedObject = placedObjectGetter.Record;

                if (placedObject.LightData == null) continue;
                placedObject.Base.TryResolve<ILightGetter>(state.LinkCache, out var placedObjectBase);
                if (placedObjectBase == null || placedObjectBase.EditorID == null) continue;
                if (placedObjectBase.EditorID.ContainsInsensitive("Candle") || placedObjectBase.EditorID.ContainsInsensitive("Torch") || placedObjectBase.EditorID.ContainsInsensitive("Camp"))
                {
                    if (placedObject != null && placedObject.LightData != null && placedObject.LightData.FadeOffset > 0 && placedObjectGetter.ModKey != "ENB Light.esp")
                    {
                        IPlacedObject modifiedObject = placedObjectGetter.GetOrAddAsOverride(state.PatchMod);
                        modifiedObject.LightData!.FadeOffset /= 2;
                    }
                }
                else continue;
            }
            // Part 2 - Patch every LIGH record
            foreach (var light in state.LoadOrder.PriorityOrder.Light().WinningOverrides())
            {
                if (light.EditorID == null) continue;
                if (light.EditorID.ContainsInsensitive("Torch") || light.EditorID.ContainsInsensitive("Camp") || light.EditorID.ContainsInsensitive("Candle"))
                {
                    var modifiedLight = state.PatchMod.Lights.GetOrAddAsOverride(light);
                    modifiedLight.FadeValue /= 2;
                }
            }
        }
    }
}
