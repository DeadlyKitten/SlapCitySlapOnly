using HarmonyLib;
using Smash;
using System.Collections.Generic;

namespace SlapCitySlapOnly.Patches
{
    // Any attacking move that isn't jab gets ignored
    [HarmonyPatch(typeof(SmashCharacter), "CancelToMove")]
    class SmashCharacter_CancelToMove
    {
        static void Prefix(SmashCharacter __instance, ref string moveId)
        {
            // Only Fishbunjin
            if (__instance.gameObject.name.Split('(')[0].ToLower() != "fishbunjin") return;
            if (bannedMoves.Contains(moveId)) moveId = "jab";
        }

        static readonly List<string> bannedMoves = new List<string>()
        {
            "ftilt",
            "utilt",
            "dtilt",
            "fstr",
            "dstr",
            "dstr1rec",
            "ustr",
            "nair",
            "dair",
            "uair",
            "bair",
            "fair",
            "strair",
            "spn",
            "spf",
            "spfrun",
            "spfatk",
            "spu",
            "spd",
            "getupatk",
            "grab",
        };
    }
}
