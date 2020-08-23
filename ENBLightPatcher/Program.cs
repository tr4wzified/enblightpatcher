using System;
using System.Collections.Generic;
using System.Linq;
using Mutagen.Bethesda;
using Mutagen.Bethesda.Synthesis;
using Mutagen.Bethesda.Skyrim;
using Newtonsoft.Json.Serialization;
using Wabbajack.Common;

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
                new UserPreferences()
                {
                    ActionsForEmptyArgs = new RunDefaultPatcher()
                    {
                        IdentifyingModKey = "ENBLightPatcher.esp",
                        TargetRelease = GameRelease.SkyrimSE
                    }
                }
            );
        }

        public static void RunPatch(SynthesisState<ISkyrimMod, ISkyrimModGetter> state)
        {
            FormKey[] cells = { };

            foreach (var mod in state.LoadOrder.PriorityOrder)
            {
                if (mod.Mod == null) continue;

                foreach (var worldspaceGetter in mod.Mod.Worldspaces)
                {
                    foreach (var blockGetter in worldspaceGetter.SubCells)
                    {
                        foreach (var subBlockGetter in blockGetter.Items)
                        {
                            foreach (var cell in subBlockGetter.Items)
                            {
                                foreach (var refr in cell.Persistent)
                                {
                                    switch (refr)
                                    {
                                        case IPlacedObject placed:
                                            if (placed.Base.TryResolve.try
                                            {

                                            }
                                            break;

                                        default: continue;
                                    }
                                }
                            }
                        }
                    }
                }

                //    foreach (var refr in cell.Persistent)
                //    {
                //        switch (refr)
                //        {
                //            case IPlacedObjectGetter placed:
                //                placed.Base.
                //                break;

                //            default:
                //                continue;
                //        }

                //        if (refr. == null) continue;

                //        if (!refr.Name.ContainsInsensitive("LIGH:")) continue;

                //        if (!refr.Name.ContainsInsensitive("Torch") && !refr.Name.ContainsInsensitive("Camp") && !refr.Name.ContainsInsensitive("Candle")) continue;

                //    }
            }

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
