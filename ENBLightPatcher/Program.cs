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

            // Part 1 - patch every light record in CELL records
            /*
            foreach(var cellRecord in state.LoadOrder.PriorityOrder.WinningOverrides<ICellGetter>())
            {
            }
            */
            // Part 2 - patch every light record in WRLD records
            foreach (var worldspaceGetter in state.LoadOrder.PriorityOrder.WinningOverrides<IWorldspaceGetter>())
            {
                var wCopy = worldspaceGetter.DeepCopy();
                bool worldspaceUpdated = false;
                foreach (var blockGetter in worldspaceGetter.SubCells)
                {
                    Console.WriteLine("Going over Worldspace Subcell");
                    foreach (var subBlockGetter in blockGetter.Items)
                    {
                        Console.WriteLine("Going over Worldspace Subcell Cell");
                        foreach (var cell in subBlockGetter.Items)
                        {
                            Console.WriteLine("Going over Worldspace Subcell Cell Item");
                            foreach (var refr in cell.Temporary)
                            {
                                Console.WriteLine("Going over Worldspace Subcell Cell Item Reference");
                                Console.WriteLine("Checking: " + refr.EditorID);
                                if (!(refr is IPlacedObject)) continue;
                                IPlacedObject placedObject = (IPlacedObject)refr;
                                if (placedObject.LightData == null) continue;
                                placedObject.Base.TryResolve(state.LinkCache, out var placedObjectBase);
                                if (placedObjectBase == null || placedObjectBase.EditorID == null) continue;
                                if (placedObjectBase.EditorID.Contains("Candle") || placedObjectBase.EditorID.Contains("Torch") || placedObjectBase.EditorID.Contains("Camp"))
                                {
                                    placedObject.LightData.FadeOffset /= 2;
                                    Console.WriteLine("Setting fade offset");
                                    worldspaceUpdated = true;
                                }
                                else continue;
                            }
                        }
                    }
                }
                Console.WriteLine("World space updated? " + worldspaceUpdated);
                if (worldspaceUpdated) state.PatchMod.Worldspaces.Set(wCopy);
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
