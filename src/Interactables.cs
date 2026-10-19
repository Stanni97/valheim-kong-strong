using UnityEngine;

namespace KongStrong
{
    /// <summary>Takes all Kong empties from the inventory and pays coins for them.</summary>
    internal class KongDepositMachine : MonoBehaviour, Hoverable, Interactable
    {
        internal const string EmptyToken = "$item_kong_empty";
        internal static int CoinsPerEmpty = 1;

        public string GetHoverName() => Localization.instance.Localize("$piece_kong_automat");

        public float GetHoverOffset() => 0f;

        public string GetHoverText()
        {
            int empties = Player.m_localPlayer == null ? 0 : Player.m_localPlayer.GetInventory().CountItems(EmptyToken, -1, true);
            return Localization.instance.Localize(
                $"$piece_kong_automat\n[<color=yellow><b>$KEY_Use</b></color>] $piece_kong_automat_use ({empties} × {CoinsPerEmpty} $item_coins)");
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || !(user is Player player) || player != Player.m_localPlayer)
            {
                return false;
            }

            Inventory inventory = player.GetInventory();
            int empties = inventory.CountItems(EmptyToken, -1, true);
            if (empties <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "$msg_kong_noempties");
                return false;
            }

            GameObject coins = ObjectDB.instance.GetItemPrefab("Coins");
            int total = empties * Mathf.Max(0, CoinsPerEmpty);
            if (coins == null || (total > 0 && !inventory.CanAddItem(coins, total)))
            {
                player.Message(MessageHud.MessageType.Center, "$inventory_full");
                return false;
            }

            inventory.RemoveItem(EmptyToken, empties, -1, true);
            if (total > 0)
            {
                inventory.AddItem(coins, total);
            }

            player.Message(MessageHud.MessageType.Center,
                string.Format(Localization.instance.Localize("$msg_kong_deposit"), empties, total));
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item) => false;
    }

    /// <summary>An XXL canister everyone can tap. Servings live in the piece's ZDO, so all players share them.</summary>
    internal class KongTap : MonoBehaviour, Hoverable, Interactable
    {
        internal const string XxlToken = "$item_kong_xxl";
        internal static int Full = 38;              // 18.9 litres in 0.5 litre cans
        internal static StatusEffect Effect;
        private const string ServingsKey = "kong_servings";

        private ZNetView _nview;

        private void Awake()
        {
            _nview = GetComponent<ZNetView>();
            if (_nview == null || _nview.GetZDO() == null)
            {
                return;                             // build preview
            }

            if (_nview.IsOwner() && _nview.GetZDO().GetInt(ServingsKey, -1) < 0)
            {
                _nview.GetZDO().Set(ServingsKey, Full);
            }
        }

        private int Servings => _nview != null && _nview.IsValid() ? Mathf.Max(0, _nview.GetZDO().GetInt(ServingsKey, 0)) : 0;

        public string GetHoverName() => Localization.instance.Localize("$piece_kong_tap");

        public float GetHoverOffset() => 0f;

        public string GetHoverText()
        {
            string text = $"$piece_kong_tap ({Servings}/{Full})\n" +
                          (Servings > 0 ? "[<color=yellow><b>$KEY_Use</b></color>] $piece_kong_tap_use\n" : "") +
                          "[<color=yellow><b>1-8</b></color>] $piece_kong_tap_refill";
            return Localization.instance.Localize(text);
        }

        public bool Interact(Humanoid user, bool hold, bool alt)
        {
            if (hold || !(user is Player player) || player != Player.m_localPlayer || _nview == null || !_nview.IsValid())
            {
                return false;
            }

            int left = Servings;
            if (left <= 0)
            {
                player.Message(MessageHud.MessageType.Center, "$msg_kong_tapempty");
                return false;
            }

            if (Effect == null)
            {
                return false;
            }

            if (player.GetSEMan().HaveStatusEffectCategory(Effect.m_category))
            {
                player.Message(MessageHud.MessageType.Center, "$msg_cantconsume");
                return false;
            }

            _nview.ClaimOwnership();
            _nview.GetZDO().Set(ServingsKey, left - 1);
            player.GetSEMan().AddStatusEffect(Effect.NameHash(), true);
            return true;
        }

        public bool UseItem(Humanoid user, ItemDrop.ItemData item)
        {
            if (item == null || item.m_shared.m_name != XxlToken || !(user is Player player) || _nview == null || !_nview.IsValid())
            {
                return false;
            }

            player.GetInventory().RemoveOneItem(item);
            _nview.ClaimOwnership();
            _nview.GetZDO().Set(ServingsKey, Full);
            player.Message(MessageHud.MessageType.Center, "$msg_kong_refilled");
            return true;
        }
    }
}
