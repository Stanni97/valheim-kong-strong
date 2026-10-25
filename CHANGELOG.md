# Changelog

[Deutsch](CHANGELOG.de.md)

## 1.3.0

- **Server config:** On a server with the mod, its values (drinks, crash, tolerance, Skyr cup, build pieces) apply to every player. The server sends them on join; on leaving, your own values count again. Without a server your own config counts as before. Personal settings stay your own: `[Sound]`, `CustomModels`, `PowerAnimation`.
- **Shout from the server:** If the server has a `shout.wav` (`[Sound] CustomShoutFile`), every player hears that one while connected. Without one, your own file plays. Whether the shout is on and how loud stays your choice.
- Changed values work right away, without a restart: recipes, buffs, crash and heart racing, Skyr cup, build pieces and the durations in the descriptions.
- `Enabled = false` now removes the recipe (or the piece from the hammer) instead of taking the item out of the game.
- Old default values in the config values in the config are now replaced only once after an update (new key `[General] ConfigVersion`). Before, the check ran at every start, so turning the shout off (`Shout = false`) was switched back on at the next start. Your own values are kept.

## 1.2.2

- A dedicated server no longer loads the shout, and without a `shout.wav` the "No shout file" line is logged once instead of once per drink.
- README: updates go through the mod manager (r2modman, Gale). Bifrost Updater is only mentioned as an optional tool for manual installs from GitHub.

## 1.2.1

- The textures of the build pieces repeated several times because Valheim's piece shader tiles them. Build pieces now use a plain Standard material, and the deposit machine texture is drawn to the machine's proportions (size unchanged).
- Empties are now an intact grey can in full size instead of a crushed one, so the deposit machine can scan them.
- The "D K!" shout is on by default and louder (volume 1, raised to full scale plus `ShoutBoost`). The mod no longer ships any sound: the shout only plays with your own `shout.wav`. Values still at the 1.2.0 default switch at startup.
- The deposit machine has English lettering (DEPOSIT / EMPTIES) for every game language except German (PFAND / LEERGUT).
- README is now English first, with the German version in `README.de.md`.

## 1.2.0

- New drinks: Frost, Toxic and Eitr Edition (resistances and eitr), Kong Mead Bomb (fermenter) and the Skyr cup as food.
- Own models and textures for all drinks (`CustomModels`), icons in Valheim's style (first version in `Assets/backup-v1`).
- Power animation with Eikthyr's glow when drinking, without using up the real power.
- Caffeine tolerance with heart racing, sleep block in beds, empties with a deposit machine.
- Build pieces: Kong fridge, can pyramid, deposit machine and Kong tap (shared servings in multiplayer).
- Optional "D K!" shout (off), own WAV via `CustomShoutFile`.
- Config migration: recipes and crafting station at an old default switch to the new one at startup, so updates arrive without manual work.
- Still only needs BepInEx and Jotunn (Harmony is part of BepInEx).

## 1.1.0

- All drinks are now made at the cauldron instead of the workbench.
  Existing configs keep their value, set `CraftingStation = Cauldron` there.
- New recipes: Sugar Free from blueberries, thistle and yellow mushroom; Urban Classic with bloodbag and
  yellow mushroom; Skyr Energy with egg; XXL from Ashlands ingredients (vineberries, smoke puffs) and yellow mushrooms.
  Existing configs keep their old `Ingredients`.
- Warning in the log when an ingredient in the config does not exist.

## 1.0.0

- First version with four drinks: Sugar Free, Urban Classic (with sugar crash), Skyr Energy and the XXL canister.
