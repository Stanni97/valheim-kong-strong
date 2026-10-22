using System;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace KongStrong
{
    /// <summary>English and German texts.</summary>
    internal static class Texts
    {
        public static void Register(Func<string, Drink> drink, SkyrCupSettings cup)
        {
            string M(string key) => Mathf.Max(1, Mathf.RoundToInt(drink(key).Duration / 60f)).ToString();
            bool fireproof = Protects(drink("XXL").FireResistance);
            CustomLocalization loc = LocalizationManager.Instance.GetLocalization();

            loc.AddTranslation("English", new Dictionary<string, string>
            {
                { "item_kong_sugarfree", "Kong Strong Sugar Free" },
                { "item_kong_sugarfree_desc", $"Ice-cold, blue and sugar free. More stamina and speed for {M("SugarFree")} minutes." },
                { "se_kong_sugarfree", "Kong Strong" },
                { "se_kong_sugarfree_tooltip", "Caffeine is rushing through your veins." },
                { "se_kong_sugarfree_start", "KONG STRONG!" },

                { "item_kong_classic", "Kong Strong Urban Classic" },
                { "item_kong_classic_desc", $"The red can, full of sugar. A big kick for {M("Classic")} minutes, followed by a sugar crash." },
                { "se_kong_classic", "Sugar Rush" },
                { "se_kong_classic_tooltip", "Sugar and caffeine. This won't end well." },
                { "se_kong_classic_start", "KONG STRONG!" },

                { "item_kong_skyr", "Skyr Energy" },
                { "item_kong_skyr_desc", $"Protein drink with caffeine. Carry more and regenerate faster for {M("Skyr")} minutes." },
                { "se_kong_skyr", "Skyr Power" },
                { "se_kong_skyr_tooltip", "27 g of protein. Gains incoming." },
                { "se_kong_skyr_start", "PROTEIN!" },

                { "item_kong_xxl", "Kong Strong XXL (18.9 L)" },
                { "item_kong_xxl_desc", $"18.9 litres of Sugar Free in a canister. Lasts {M("XXL")} minutes{(fireproof ? " and protects you from fire" : "")}. Good luck carrying it." },
                { "se_kong_xxl", "Kong Strong XXL" },
                { "se_kong_xxl_tooltip", "You drank 18.9 litres. Your heart is doing 200 bpm." },
                { "se_kong_xxl_start", "KONG STRONG XXL!" },

                { "item_kong_frost", "Kong Strong Frost Edition" },
                { "item_kong_frost_desc", $"Ice-blue and ice-cold. Protects you from frost and freezing for {M("Frost")} minutes." },
                { "se_kong_frost", "Frost Edition" },
                { "se_kong_frost_tooltip", "Brain freeze included." },
                { "se_kong_frost_start", "KONG STRONG FROST!" },

                { "item_kong_toxic", "Kong Strong Toxic Edition" },
                { "item_kong_toxic_desc", $"Neon green. Poison bothers you a lot less for {M("Toxic")} minutes." },
                { "se_kong_toxic", "Toxic Edition" },
                { "se_kong_toxic_tooltip", "Tastes like the swamp. In a good way." },
                { "se_kong_toxic_start", "KONG STRONG TOXIC!" },

                { "item_kong_eitr", "Kong Strong Eitr Edition" },
                { "item_kong_eitr_desc", $"Purple and magical. Faster eitr regeneration for {M("Eitr")} minutes." },
                { "se_kong_eitr", "Eitr Edition" },
                { "se_kong_eitr_tooltip", "Sparkles on the tongue." },
                { "se_kong_eitr_start", "KONG STRONG EITR!" },

                { "item_kong_metbombe", "Kong Mead Bomb" },
                { "item_kong_metbombe_desc", $"Mead and Kong in one bottle. Heals you and gives the Kong kick for {M("MetBombe")} minutes." },
                { "item_kong_metbombe_base", "Kong mead bomb base" },
                { "item_kong_metbombe_base_desc", "Ferment it to get Kong mead bombs." },
                { "se_kong_metbombe", "Mead Bomb" },
                { "se_kong_metbombe_tooltip", "Honey, caffeine and a bad idea." },
                { "se_kong_metbombe_start", "SKÅL & KONG!" },

                { "item_kong_skyrcup", "Skyr cup" },
                { "item_kong_skyrcup_desc", "Thick, creamy and full of protein. Proper food, not a drink." },

                { "item_kong_empty", "Kong empties" },
                { "item_kong_empty_desc", "Empty can or bottle, undamaged so the barcode still scans. Build something with it or bring it to a deposit machine." },

                { "se_kong_stop", "The energy rush wears off." },
                { "se_kong_crash", "Sugar Crash" },
                { "se_kong_crash_tooltip", "The sugar is gone and you are tired." },
                { "se_kong_crash_start", "Sugar crash..." },
                { "se_kong_heartracing", "Heart Racing" },
                { "se_kong_heartracing_tooltip", "Too much caffeine. Take a break." },
                { "se_kong_heartracing_start", "HEART RACING!" },

                { "msg_kong_tolerance", "Caffeine tolerance: the drink works less." },
                { "msg_kong_tooawake", "You are way too awake to sleep." },

                { "piece_kong_fridge", "Kong fridge" },
                { "piece_kong_fridge_desc", "Keeps your Kong ice-cold. 16 slots." },
                { "piece_kong_pyramid", "Can pyramid" },
                { "piece_kong_pyramid_desc", "Ten empties, artfully stacked. A little comfort for true connoisseurs." },
                { "piece_kong_automat", "Deposit machine" },
                { "piece_kong_automat_desc", "Return your Kong empties for coins." },
                { "piece_kong_automat_use", "Return empties" },
                { "piece_kong_tap", "Kong tap" },
                { "piece_kong_tap_desc", "An XXL canister to share: everyone can tap a Sugar Free here. Refill it with another canister." },
                { "piece_kong_tap_use", "Tap a Kong" },
                { "piece_kong_tap_refill", "Refill with an XXL canister" },
                { "msg_kong_noempties", "You have no empties on you." },
                { "msg_kong_deposit", "Deposit: {0} empties, {1} coins" },
                { "msg_kong_tapempty", "The tap is empty." },
                { "msg_kong_refilled", "Tap refilled." },
            });

            loc.AddTranslation("German", new Dictionary<string, string>
            {
                { "item_kong_sugarfree", "Kong Strong Sugar Free" },
                { "item_kong_sugarfree_desc", $"Eiskalt, blau und zuckerfrei. Mehr Ausdauer und Tempo für {M("SugarFree")} Minuten." },
                { "se_kong_sugarfree", "Kong Strong" },
                { "se_kong_sugarfree_tooltip", "Koffein rauscht durch deine Adern." },
                { "se_kong_sugarfree_start", "KONG STRONG!" },

                { "item_kong_classic", "Kong Strong Urban Classic" },
                { "item_kong_classic_desc", $"Die rote Dose mit ordentlich Zucker. Ein starker Kick für {M("Classic")} Minuten, danach kommt der Zuckercrash." },
                { "se_kong_classic", "Zuckerschock" },
                { "se_kong_classic_tooltip", "Zucker und Koffein. Das geht nicht lange gut." },
                { "se_kong_classic_start", "KONG STRONG!" },

                { "item_kong_skyr", "Skyr Energy" },
                { "item_kong_skyr_desc", $"Protein-Drink mit Koffein. Mehr Tragkraft und schnellere Regeneration für {M("Skyr")} Minuten." },
                { "se_kong_skyr", "Skyr Power" },
                { "se_kong_skyr_tooltip", "27 g Protein. Die Gains kommen." },
                { "se_kong_skyr_start", "PROTEIN!" },

                { "item_kong_xxl", "Kong Strong XXL (18,9 L)" },
                { "item_kong_xxl_desc", $"18,9 Liter Sugar Free im Kanister. Hält {M("XXL")} Minuten{(fireproof ? " und schützt vor Feuer" : "")}. Viel Spaß beim Tragen." },
                { "se_kong_xxl", "Kong Strong XXL" },
                { "se_kong_xxl_tooltip", "Du hast 18,9 Liter getrunken. Dein Herz schlägt mit 200 bpm." },
                { "se_kong_xxl_start", "KONG STRONG XXL!" },

                { "item_kong_frost", "Kong Strong Frost Edition" },
                { "item_kong_frost_desc", $"Eisblau und eiskalt. Schützt {M("Frost")} Minuten lang vor Frost und Erfrieren." },
                { "se_kong_frost", "Frost Edition" },
                { "se_kong_frost_tooltip", "Hirnfrost inklusive." },
                { "se_kong_frost_start", "KONG STRONG FROST!" },

                { "item_kong_toxic", "Kong Strong Gift Edition" },
                { "item_kong_toxic_desc", $"Neongrün. Gift setzt dir {M("Toxic")} Minuten lang deutlich weniger zu." },
                { "se_kong_toxic", "Gift Edition" },
                { "se_kong_toxic_tooltip", "Schmeckt nach Sumpf. Im guten Sinne." },
                { "se_kong_toxic_start", "KONG STRONG GIFT!" },

                { "item_kong_eitr", "Kong Strong Eitr Edition" },
                { "item_kong_eitr_desc", $"Lila und magisch. Schnellere Eitr-Regeneration für {M("Eitr")} Minuten." },
                { "se_kong_eitr", "Eitr Edition" },
                { "se_kong_eitr_tooltip", "Prickelt magisch auf der Zunge." },
                { "se_kong_eitr_start", "KONG STRONG EITR!" },

                { "item_kong_metbombe", "Kong-Met-Bombe" },
                { "item_kong_metbombe_desc", $"Met und Kong in einer Flasche. Heilt dich und gibt {M("MetBombe")} Minuten lang den Kong-Kick." },
                { "item_kong_metbombe_base", "Kong-Met-Bombe (Ansatz)" },
                { "item_kong_metbombe_base_desc", "Im Gärbottich wird daraus die Kong-Met-Bombe." },
                { "se_kong_metbombe", "Met-Bombe" },
                { "se_kong_metbombe_tooltip", "Honig, Koffein und eine schlechte Idee." },
                { "se_kong_metbombe_start", "SKÅL & KONG!" },

                { "item_kong_skyrcup", "Skyr-Becher" },
                { "item_kong_skyrcup_desc", "Dick, cremig und voller Protein. Richtiges Essen, kein Drink." },

                { "item_kong_empty", "Kong-Leergut" },
                { "item_kong_empty_desc", "Leere Dose oder Flasche, unbeschädigt, damit der Pfand noch gescannt wird. Daraus lässt sich etwas bauen, oder du bringst sie zum Pfandautomaten." },

                { "se_kong_stop", "Der Energiekick lässt nach." },
                { "se_kong_crash", "Zuckercrash" },
                { "se_kong_crash_tooltip", "Der Zucker ist alle und du bist müde." },
                { "se_kong_crash_start", "Zuckercrash..." },
                { "se_kong_heartracing", "Herzrasen" },
                { "se_kong_heartracing_tooltip", "Zu viel Koffein. Mach mal Pause." },
                { "se_kong_heartracing_start", "HERZRASEN!" },

                { "msg_kong_tolerance", "Koffein-Toleranz: Der Drink wirkt schwächer." },
                { "msg_kong_tooawake", "Du bist viel zu wach zum Schlafen." },

                { "piece_kong_fridge", "Kong-Kühlschrank" },
                { "piece_kong_fridge_desc", "Hält dein Kong eiskalt. 16 Plätze." },
                { "piece_kong_pyramid", "Dosenpyramide" },
                { "piece_kong_pyramid_desc", "Zehn leere Dosen, kunstvoll gestapelt. Etwas Komfort für echte Kenner." },
                { "piece_kong_automat", "Pfandautomat" },
                { "piece_kong_automat_desc", "Gib dein Kong-Leergut gegen Münzen zurück." },
                { "piece_kong_automat_use", "Leergut zurückgeben" },
                { "piece_kong_tap", "Kong-Zapfstelle" },
                { "piece_kong_tap_desc", "Ein XXL-Kanister zum Teilen: Hier kann sich jeder ein Sugar Free zapfen. Mit einem weiteren Kanister auffüllbar." },
                { "piece_kong_tap_use", "Kong zapfen" },
                { "piece_kong_tap_refill", "Mit XXL-Kanister auffüllen" },
                { "msg_kong_noempties", "Du hast kein Leergut dabei." },
                { "msg_kong_deposit", "Pfand: {0} Leergut, {1} Münzen" },
                { "msg_kong_tapempty", "Die Zapfstelle ist leer." },
                { "msg_kong_refilled", "Zapfstelle aufgefüllt." },
            });
        }

        public static bool Protects(HitData.DamageModifier modifier)
        {
            return modifier is HitData.DamageModifier.SlightlyResistant or HitData.DamageModifier.Resistant
                or HitData.DamageModifier.VeryResistant or HitData.DamageModifier.Immune;
        }
    }
}
