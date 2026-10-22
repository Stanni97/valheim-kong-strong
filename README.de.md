# Kong Strong Energy für Valheim

[English](README.md) · **Deutsch**

![Sugar Free](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/sugarfree.png) ![Urban Classic](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/classic.png) ![Skyr Energy](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/skyr.png) ![XXL-Kanister](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/xxl.png)

![Frost](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/frost.png) ![Toxic](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/toxic.png) ![Eitr](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/eitr.png) ![Met-Bombe](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/metbombe.png) ![Skyr-Becher](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/skyrcup.png) ![Kühlschrank](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/fridge.png) ![Dosenpyramide](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/pyramid.png) ![Pfandautomat](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/automat.png) ![Zapfstelle](https://raw.githubusercontent.com/Stanni97/valheim-kong-strong/main/Assets/tap.png)

Ein Valheim-Mod mit Energydrinks im Stil von Kong Strong: mehr Ausdauer, mehr Tempo, Schutz vor Feuer, Frost und Gift, dazu eigene Modelle, Leergut mit Pfandautomat und Baustücke wie Kühlschrank und Zapfstelle.

## Die Drinks

| Drink | Dauer | Wirkung |
| --- | --- | --- |
| **Kong Strong Sugar Free** (blau) | 5 Min. | +40 Sofort-Ausdauer, +50 % Ausdauer-Regeneration, +10 % Tempo, −25 % Ausdauerverbrauch beim Rennen und Springen |
| **Kong Strong Urban Classic** (rot, mit Zucker) | 3 Min. | +80 Sofort-Ausdauer, +75 % Ausdauer-Regeneration, +15 % Tempo, −25 % beim Rennen und Springen. **Danach 1 Min. Zuckercrash:** −25 % Ausdauer-Regeneration, −5 % Tempo |
| **Skyr Energy** (Protein-Drink) | 10 Min. | +20 Sofort-Ausdauer, +50 Tragkraft, +25 % Lebens- und Ausdauer-Regeneration |
| **Kong Strong XXL** (18,9-L-Kanister) | 20 Min. | Wirkung wie Sugar Free, dazu **feuerfest** (halber Feuerschaden). Wiegt 19 kg und stapelt nicht |

| **Frost Edition** (eisblau) | 10 Min. | +30 Sofort-Ausdauer, +25 % Ausdauer-Regeneration, +5 % Tempo, −15 % beim Rennen und Springen, **halber Frostschaden** (wie der Frostresistenz-Met) |
| **Toxic Edition** (grün, „Gift Edition“) | 10 Min. | wie Frost, aber **halber Giftschaden** |
| **Eitr Edition** (lila) | 10 Min. | +20 Sofort-Ausdauer, +25 Sofort-Eitr, +25 % Ausdauer- und +50 % Eitr-Regeneration |
| **Kong-Met-Bombe** | 5 Min. | +50 Leben über 10 Sekunden, +50 % Ausdauer-Regeneration, +10 % Tempo, −25 % beim Rennen und Springen |
| **Skyr-Becher** (Essen) | 30 Min. | normales Essen für einen der drei Essensplätze: +50 Leben, +20 Ausdauer, Lebensregeneration |

Es wirkt immer nur ein Kong-Drink auf einmal, und während des Zuckercrashs kannst du keinen neuen trinken. Mit Met lassen sich alle Drinks kombinieren.

### Drumherum

- **Kraft-Animation:** Beim Trinken hebst du die Arme wie bei einer Kraft der Verlassenen, samt Eikthyr-Leuchten ohne dessen Ton. Deine echte Kraft und ihre Abklingzeit bleiben unberührt.
- **Koffein-Toleranz:** Mehrere Drinks innerhalb von 15 Minuten wirken schwächer (der zweite zu 50 %, jeder weitere zu 25 %). Ab dem dritten gibt es 2 Minuten **Herzrasen** mit langsamerer Regeneration.
- **Zu wach zum Schlafen:** Mit aktivem Kong-Buff verweigert dein Bett den Schlaf.
- **Leergut:** Dosen und Flaschen hinterlassen unbeschädigtes Leergut (nicht zerdrückt, sonst scannt der Pfandautomat es nicht). Daraus baust du die Dosenpyramide, oder du tauschst es am Pfandautomaten gegen Münzen.
- **Eigene Modelle:** Dosen, Flaschen, Kanister, Becher und Fass sind eigene Modelle, in der Config mit `CustomModels = false` auf die Met-Flaschen des Spiels umschaltbar.
- **„D K!“-Ruf:** Nach den Kong-Strong-Dosen kann ein eigener Ruf kommen (`[Sound] Shout`, standardmäßig an). Die Mod liefert **keinen** Ton mit: Lege eine eigene WAV-Datei (16 Bit PCM) unter `BepInEx/config/KongStrong/shout.wav` ab, sie bleibt auf deinem Rechner. Ohne Datei bleibt es still. `ShoutBoost` macht den Ton lauter.

## Herstellung

Alle Drinks werden am **Kessel** (Cauldron) gebraut. Den Kessel kannst du bauen, sobald du Zinn aus dem Schwarzwald hast, und er braucht ein Feuer darunter.

| Drink | Zutaten | Ergibt | Ab Biom |
| --- | --- | --- | --- |
| Sugar Free | 3 Blaubeeren, 1 Distel, 1 Gelber Pilz | 2 | Schwarzwald |
| Urban Classic | 2 Honig, 3 Himbeeren, 1 Blutbeutel, 1 Gelber Pilz | 2 | Sumpf |
| Skyr Energy | 1 Ei, 1 Honig, 2 Blaubeeren | 2 | Ebenen |
| XXL | 6 Vineberries, 3 Smoke Puffs, 3 Gelbe Pilze | 1 | Ashlands |

| Frost Edition | 3 Blaubeeren, 1 Gefrierdrüse, 1 Gelber Pilz | 2 | Deep North |
| Toxic Edition | 2 Blaubeeren, 1 Schleim, 1 Distel, 1 Gelber Pilz | 2 | Sumpf |
| Eitr Edition | 1 Saft, 1 Magierhut-Pilz, 1 Gelber Pilz, 2 Blaubeeren | 2 | Mistlands |
| Skyr-Becher | 2 Eier, 2 Wolkenbeeren, 1 Honig | 2 | Ebenen |
| Met-Bombe | Ansatz am Kessel: 5 Honig, 2 Sugar Free, 5 Himbeeren, dann im Gärbottich | 6 | Ashlands/Sumpf |

Sugar Free kommt ohne Honig aus, schließlich ist es zuckerfrei.

## Baustücke (Hammer)

| Stück | Station | Zutaten | Besonderheit |
| --- | --- | --- | --- |
| Kong-Kühlschrank | Werkbank | 6 Edelholz, 2 Bronze, 1 Gefrierdrüse | 16 Plätze, Tür öffnet sich |
| Dosenpyramide | – | 10 Kong-Leergut | etwas Komfort |
| Pfandautomat | Werkbank | 8 Edelholz, 3 Bronze | 1 Münze pro Leergut |
| Kong-Zapfstelle | Werkbank | 1 XXL-Kanister, 6 Holz | 38 Drinks für alle Mitspieler, Nachfüllen durch Rechtsklick mit einem weiteren XXL |

Alle Werte, Rezepte und Stationen lassen sich in der Config ändern (siehe [Konfiguration](#konfiguration)).

## Voraussetzungen

- [BepInExPack_Valheim](https://thunderstore.io/c/valheim/p/denikson/BepInExPack_Valheim/) 5.4.2333 oder neuer
- [Jotunn](https://thunderstore.io/c/valheim/p/ValheimModding/Jotunn/) 2.30.2 oder neuer

Im **Multiplayer** müssen alle Spieler und der Server den Mod in derselben Version haben, sonst verweigert Jotunn die Verbindung.

## Installation

Die fertige Mod gibt es auf der [Release-Seite](https://github.com/Stanni97/valheim-kong-strong/releases/latest) als `KongStrongEnergy-<version>.zip`. Darin liegen `plugins/KongStrong.dll` sowie `manifest.json` und `icon.png` für Mod-Manager.

### Mit Mod-Manager (Windows und Linux)

In [r2modman](https://thunderstore.io/c/valheim/p/ebkr/r2modman/) oder [Gale](https://thunderstore.io/c/valheim/p/Kesomannen/GaleModManager/):

1. Ein Valheim-Profil anlegen und darin **BepInExPack_Valheim** und **Jotunn** installieren.
2. Die Zip-Datei mit der Import-Funktion für lokale Mods einbinden (in r2modman: **Settings → Import local mod**).
3. Valheim über den Mod-Manager starten (in r2modman: **Start modded**).

### Manuell unter Windows

1. Den Valheim-Ordner öffnen: in Steam Rechtsklick auf Valheim → **Verwalten** → **Lokale Dateien durchsuchen**.
2. **BepInExPack_Valheim** herunterladen und den *Inhalt* des Ordners `BepInExPack_Valheim` aus der Zip direkt in den Valheim-Ordner kopieren. Danach liegen `BepInEx\`, `winhttp.dll` und `doorstop_config.ini` neben `valheim.exe`.
3. **Jotunn** herunterladen und `plugins\Jotunn.dll` nach `BepInEx\plugins\` kopieren.
4. Den Ordner `BepInEx\plugins\KongStrong\` anlegen und `KongStrong.dll` hineinkopieren.
5. Valheim ganz normal über Steam starten. Unter Windows sind keine Startoptionen nötig.

### Manuell unter Linux (native Steam-Version)

1. Den Valheim-Ordner öffnen, meist `~/.local/share/Steam/steamapps/common/Valheim`.
2. Den Inhalt von `BepInExPack_Valheim` aus der Zip in diesen Ordner kopieren und das Startskript ausführbar machen:
   ```bash
   chmod +x start_game_bepinex.sh
   ```
3. Jotunn und `KongStrong.dll` wie unter Windows ablegen: `BepInEx/plugins/Jotunn.dll` und `BepInEx/plugins/KongStrong/KongStrong.dll`.
4. In Steam Rechtsklick auf Valheim → **Eigenschaften** → **Startoptionen**:
   ```
   ./start_game_bepinex.sh %command%
   ```
5. Valheim über Steam starten.

**Linux mit Proton:** Wer Valheim per Proton startet (Kompatibilität erzwungen), installiert die Dateien wie unter Windows und setzt als Startoption stattdessen `WINEDLLOVERRIDES="winhttp=n,b" %command%`.

### Dedicated Server

`BepInExPack_Valheim`, `Jotunn.dll` und `KongStrong/KongStrong.dll` auf dem Server genauso ablegen. Unter Linux startest du den Server mit `start_server_bepinex.sh`, unter Windows ganz normal.

### Funktioniert es?

- In `BepInEx/LogOutput.log` stehen Zeilen wie `Added KongStrongSugarFree`.
- Am Kessel tauchen die Drinks in der Rezeptliste auf.
- Zum Testen geht auch die Konsole (F5): erst `devcommands`, dann zum Beispiel `spawn KongStrongSugarFree 5`.
  Die Prefab-Namen sind `KongStrongSugarFree`, `KongStrongClassic`, `KongSkyrEnergy` und `KongStrongXXL`.

## Konfiguration

Nach dem ersten Start mit dem Mod liegt die Datei `BepInEx/config/felix.kongstrong.cfg` im Valheim-Ordner. Änderungen greifen nach einem Neustart des Spiels.

Die Drink-Abschnitte `[SugarFree]`, `[Classic]`, `[Skyr]`, `[XXL]`, `[Frost]`, `[Toxic]`, `[Eitr]` und `[MetBombe]` haben jeweils dieselben Schlüssel:

| Schlüssel | Bedeutung |
| --- | --- |
| `Enabled` | Drink an- oder abschalten |
| `DurationSeconds` | Dauer des Buffs in Sekunden |
| `InstantStamina` | Ausdauer, die sofort aufgefüllt wird |
| `StaminaRegenBonusPercent` | Schnellere Ausdauer-Regeneration in % |
| `HealthRegenBonusPercent` | Schnellere Lebens-Regeneration in % |
| `MoveSpeedBonusPercent` | Schnellere Bewegung in % |
| `RunStaminaReductionPercent` | Weniger Ausdauerverbrauch beim Rennen in % |
| `JumpStaminaReductionPercent` | Weniger Ausdauerverbrauch beim Springen in % |
| `CarryWeightBonus` | Zusätzliche Tragkraft |
| `InstantEitr`, `EitrRegenBonusPercent`, `HealOverTime` | Sofort-Eitr, schnellere Eitr-Regeneration, Heilung über 10 Sekunden |
| `FireResistance`, `FrostResistance`, `PoisonResistance` | Schaden während des Buffs: `Normal`, `SlightlyResistant`, `Resistant` (halber Schaden), `VeryResistant` oder `Immune` |
| `ReturnsEmpty` | Trinken hinterlässt Leergut |
| `CraftingStation` | Herstellungsort, Standard `Cauldron`, z. B. auch `Workbench` oder `Forge` |
| `Ingredients` | Zutaten als `Prefab:Anzahl`, durch Kommas getrennt, z. B. `Blueberries:3,Thistle:1`. Unbekannte Namen landen als Warnung im Log |
| `Amount` | Drinks pro Herstellung |

Weitere Abschnitte: `[SugarCrash]`, `[CaffeineTolerance]`, `[HeartRacing]`, `[SkyrCup]`, `[General]` (Schlafsperre, Leergut, Modelle, Animation), `[Sound]` und je Baustück `[Fridge]`, `[CanPyramid]`, `[DepositMachine]`, `[Tap]`.

**Updates:** Rezepte (`Ingredients`) und Herstellungsort (`CraftingStation`), die noch auf dem Standard einer älteren Version stehen, werden beim Start automatisch auf den neuen Standard gesetzt. Selbst geänderte Werte bleiben unberührt, die Config muss also nicht gelöscht werden.

Die Prefab-Namen der Zutaten stehen in der [Jotunn-Itemliste](https://valheim-modding.github.io/Jotunn/data/objects/item-list.html).

## Selbst bauen

Du brauchst:

- das [.NET SDK](https://dotnet.microsoft.com/download) 8 oder neuer
  - Windows: Installer von der Microsoft-Seite oder `winget install Microsoft.DotNet.SDK.10`
  - Arch/CachyOS: `sudo pacman -S dotnet-sdk`
- Valheim mit installiertem BepInExPack und Jotunn. Der Build verwendet die DLLs direkt aus dem Spielordner. NuGet-Pakete werden dabei nicht heruntergeladen.

```bash
git clone https://github.com/Stanni97/valheim-kong-strong.git
cd valheim-kong-strong
dotnet build -c Release
```

Der Build kopiert `KongStrong.dll` automatisch nach `BepInEx/plugins/KongStrong/` und legt das Release-Zip unter `dist/` ab.

Liegt Valheim nicht im Standard-Steam-Ordner, gibst du den Pfad mit:

```bash
dotnet build -c Release -p:ValheimDir="D:\SteamLibrary\steamapps\common\Valheim"
```

Das automatische Kopieren ins Spiel schaltest du mit `-p:InstallToGame=false` ab.

Bei einer neuen Version die Versionsnummer an zwei Stellen erhöhen: `<Version>` in `KongStrong.csproj` und `PluginVersion` in `src/KongStrongPlugin.cs`.

Icons, Modell-Texturen und der eingebaute Ruf entstehen mit Python 3 (Pillow, numpy): `tools/make_icons.py` (Icons, `--classic` für den ersten, klaren Stil, den es auch fertig unter `Assets/backup-v1` gibt) und `tools/make_textures.py` (Texturen der Modelle). Der Gorilla kommt aus `tools/gorilla.py`, die Meshes baut `src/Models.cs` im Code. Als Schriften dienen standardmäßig Noto Sans Black und DejaVu Serif Bold. Andere Pfade setzt du über die Umgebungsvariablen `KONG_FONT_HEAVY` und `KONG_FONT_SLAB`.

```bash
python3 tools/make_icons.py Assets 128
python3 tools/make_icons.py /tmp/icons256 256   # danach sugarfree.png als Thunderstore/icon.png kopieren
```

## Automatische Updates

Diese Mod wird vom [Bifrost Updater](https://github.com/Stanni97/bifrost-updater) mit aktualisiert, wenn er installiert ist. Er prüft beim Spielstart das neueste GitHub-Release. Die Mod nennt dafür ihr Repository in der DLL. Der Updater ist optional, die Mod läuft auch ohne.

## Hinweis

Dies ist ein inoffizieller Fan-Mod ohne Verbindung zu Lidl, Milbona oder der Marke Kong Strong. Icons, Modelle und Ton sind eigene Arbeiten im Stil der Dosen.

## Lizenz

Code und Icons stehen unter der [MIT-Lizenz](https://github.com/Stanni97/valheim-kong-strong/blob/main/LICENSE). Rechte an den Marken Kong Strong, Milbona und Lidl verleiht die Lizenz nicht, die bleiben bei ihren Inhabern.
