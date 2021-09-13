using HarmonyLib;
using MiniJSON;
using Smash;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SlapCitySlapOnly.Patches
{
    // Replaces Fish's jab with a clutchable version.
    [HarmonyPatch(typeof(SmashParse), "ParseMoveSet")]
    class SmashParse_ParseMoveSet
    {
        static void Postfix(string moveset, ref SmashCharacter.IdMove[] __result)
        {
            if (!(CreateMD5(moveset) == FISH_MOVESET_HASH)) return;

            for (var i  =  0; i <  __result.Length; i++)
            {
                if (__result[i].uid != 23) continue;

                string newMove;

                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "SlapCitySlapOnly.Resources.ClutchableSlap.json";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    newMove = reader.ReadToEnd();
                }

                __result[i] = SmashCharacter.IdMove.Parse(Json.Deserialize(newMove, true) as Dictionary<string,  object>);
            }
        }

        public static string CreateMD5(string input)
        {
            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("X2"));

                return sb.ToString();
            }
        }

        static readonly string FISH_MOVESET_HASH = "D5A75867729283BE2805B441582E37FA";
    }
}