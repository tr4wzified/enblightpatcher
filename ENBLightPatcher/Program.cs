using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Wabbajack.Common;
using Noggog;

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
        public static int Main(string[] args)
        {
            return SynthesisPipeline.Instance.Patch<ISkyrimMod, ISkyrimModGetter>(
                args: args,
                patcher: RunPatch,
                new UserPreferences
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher
                    {
                        IdentifyingModKey = "ENBLightPatcher.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                });
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            // Part 1 - Patch every placed light in worldspaces/cells
            foreach (var placedObjectGetter in state.LoadOrder.PriorityOrder.PlacedObject().WinningContextOverrides(state.LinkCache))
            {
                var placedObject = placedObjectGetter.Record;
                if (placedObject.LightData == null) continue;
                placedObject.Base.TryResolve(state.LinkCache, out var placedObjectBase);
                if (placedObjectBase == null || placedObjectBase.EditorID == null) continue;
                if (placedObjectBase.EditorID.Contains("Candle") || placedObjectBase.EditorID.Contains("Torch") || placedObjectBase.EditorID.Contains("Camp"))
                {
                    IPlacedObject modifiedObject = placedObjectGetter.GetOrAddAsOverride(state.PatchMod);
                    if (modifiedObject != null && modifiedObject.LightData != null)
                        modifiedObject.LightData.FadeOffset /= 2;
                }
                else continue;
            }
            // Part 2 - Patch every LIGH record
            foreach (var light in state.LoadOrder.PriorityOrder.WinningOverrides<ILightGetter>())
            {
                if (light.EditorID == null) continue;
                if (!light.EditorID.ContainsInsensitive("Torch") && !light.EditorID.ContainsInsensitive("Camp") && !light.EditorID.ContainsInsensitive("Candle")) continue;

                var modifiedLight = state.PatchMod.Lights.GetOrAddAsOverride(light);
                modifiedLight.FadeValue = modifiedLight.FadeValue / 2;
            }
        }
    }
}
