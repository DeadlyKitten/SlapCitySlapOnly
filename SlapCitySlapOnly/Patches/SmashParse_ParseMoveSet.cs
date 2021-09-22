using HarmonyLib;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
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
        static void Prefix(ref string moveset, ref SmashCharacter.IdMove[] __result)
        {
            if (!(CreateMD5(moveset) == FISH_MOVESET_HASH)) return;

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "SlapCitySlapOnly.Resources.ClutchableSlapPatch.json";

            JsonPatchDocument patch;

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                var json = reader.ReadToEnd();
                patch = new JsonPatchDocument(JsonConvert.DeserializeObject<List<Operation>>(json), new DefaultContractResolver());
            }

            Plugin.LogDebug(patch.ToString());

            object obj = JsonConvert.DeserializeObject(moveset);
            patch.ApplyTo(obj);
            moveset = JsonConvert.SerializeObject(obj);
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