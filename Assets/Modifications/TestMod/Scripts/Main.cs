using HarmonyLib;
using Kingmaker.Modding;
using System.Reflection;
using UnityEngine;

namespace TestMod
{
    public class Main : MonoBehaviour
    {
        public static OwlcatModification ModDetails;
        public static Harmony HarmonyHandle;

        public static void Log(string message) => ModDetails.Logger.Log(message);
        public static void Log(string fmt, params object[] args) => ModDetails.Logger.Log(fmt, args);

        [OwlcatModificationEnterPoint]
        public static void ModEntryPoint(OwlcatModification modDetails)
        {
            ModDetails = modDetails;
            HarmonyHandle = new Harmony(modDetails.Manifest.UniqueName);
            HarmonyHandle.PatchAll(Assembly.GetExecutingAssembly());
        }
    }
}