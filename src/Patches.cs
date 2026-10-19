using HarmonyLib;
using UnityEngine;

namespace KongStrong
{
    internal static class Patches
    {
        internal static bool TooAwakeToSleep = true;

        /// <summary>The bed refuses to let you sleep while a Kong buff is active. Claiming a bed still works.</summary>
        [HarmonyPatch(typeof(Bed), nameof(Bed.Interact)), HarmonyPrefix]
        private static bool BedInteract(Bed __instance, Humanoid human, bool repeat, ref bool __result)
        {
            if (!TooAwakeToSleep || repeat || human == null || human != Player.m_localPlayer)
            {
                return true;
            }

            if (!__instance.IsCurrent() || !EnvMan.CanSleep() || !SE_Kong.HasActiveDrink(human))
            {
                return true;
            }

            human.Message(MessageHud.MessageType.Center, "$msg_kong_tooawake");
            __result = false;
            return false;
        }

        /// <summary>Drinking a can or bottle leaves its empty (Leergut) behind.</summary>
        [HarmonyPatch(typeof(Player), nameof(Player.ConsumeItem)), HarmonyPostfix]
        private static void ConsumeItem(Player __instance, ItemDrop.ItemData item, bool __result)
        {
            if (!__result || item == null || __instance != Player.m_localPlayer)
            {
                return;
            }

            string empty = KongStrongPlugin.EmptyFor(item.m_shared.m_name);
            GameObject prefab = empty == null || ObjectDB.instance == null ? null : ObjectDB.instance.GetItemPrefab(empty);
            if (prefab == null)
            {
                return;
            }

            Inventory inventory = __instance.GetInventory();
            if (inventory.CanAddItem(prefab, 1))
            {
                inventory.AddItem(prefab, 1);
            }
            else
            {
                Transform t = __instance.transform;
                Object.Instantiate(prefab, t.position + t.forward * 0.5f + Vector3.up, Quaternion.identity);
            }
        }
    }
}
