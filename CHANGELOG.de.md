# Changelog (Deutsch)

[English](CHANGELOG.md)

## 1.3.0

- **Server-Config:** Auf einem Server mit dem Mod gelten seine Werte (Drinks, Crash, Toleranz, Skyr-Becher, Baustücke) für alle Spieler. Der Server schickt sie beim Betreten; beim Verlassen gelten wieder die eigenen. Ohne Server zählt wie bisher die eigene Config. Persönliche Einstellungen bleiben die eigenen: `[Sound]`, `CustomModels`, `PowerAnimation`.
- **Ruf vom Server:** Hat der Server eine `shout.wav` (`[Sound] CustomShoutFile`), hört jeder Spieler diese, solange er verbunden ist. Ohne eine spielt die eigene Datei. Ob der Ruf an ist und wie laut, entscheidet jeder selbst.
- Geänderte Werte wirken sofort, ohne Neustart: Rezepte, Buffs, Crash und Herzrasen, Skyr-Becher, Baustücke und die Dauer in den Beschreibungen.
- `Enabled = false` entfernt jetzt das Rezept (oder das Baustück aus dem Hammer), statt den Gegenstand ganz aus dem Spiel zu nehmen.
- Alte Standardwerte in der Config werden nach einem Update nur noch einmal ersetzt (neuer Eintrag `[General] ConfigVersion`). Vorher lief die Prüfung bei jedem Start, sodass ein ausgeschalteter Ruf (`Shout = false`) beim nächsten Start wieder an war. Eigene Werte bleiben erhalten.

## 1.2.2

- Ein dedizierter Server lädt den Ruf nicht mehr, und ohne `shout.wav` steht „No shout file“ nur noch einmal im Log statt einmal pro Getränk.
- README: Updates laufen über den Mod-Manager (r2modman, Gale). Bifrost Updater wird nur noch als optionales Werkzeug für manuelle Installationen von GitHub erwähnt.

## 1.2.1

- Die Textur der Baustücke wiederholte sich mehrfach (Pfandautomat in unveränderter Größe), weil Valheims Piece-Shader sie kachelt; die Baustücke nutzen jetzt ein einfaches Standard-Material (Kühlschrank, Pyramide, Automat, Zapfstelle).
- Das Leergut ist jetzt eine unbeschädigte graue Dose in voller Größe statt einer zerdrückten.
- Der Pfandautomat hat englische Beschriftung (DEPOSIT / EMPTIES) für alle Spielsprachen außer Deutsch (PFAND / LEERGUT).
- Der „D K!“-Ruf ist jetzt standardmäßig an und lauter (Lautstärke 1, Anhebung auf volle Aussteuerung plus `ShoutBoost`). Die Mod liefert keinen Ton mehr mit, der Ruf spielt nur mit einer eigenen `shout.wav`. Werte auf dem 1.2.0-Standard wechseln beim Start automatisch.

## 1.2.0

- Neue Drinks: Frost, Toxic und Eitr Edition (Resistenzen bzw. Eitr), Kong-Met-Bombe (Gärbottich) und der Skyr-Becher als Essen.
- Eigene Modelle und Texturen für alle Drinks (`CustomModels`), Icons im Valheim-Stil (erste Version in `Assets/backup-v1`).
- Kraft-Animation mit Eikthyr-Leuchten beim Trinken, ohne die echte Kraft zu verbrauchen.
- Koffein-Toleranz mit Herzrasen, Schlafsperre im Bett, Leergut mit Pfandautomat.
- Baustücke: Kong-Kühlschrank, Dosenpyramide, Pfandautomat und Kong-Zapfstelle (gemeinsame Portionen im Multiplayer).
- Optionaler „D K!“-Ruf (aus), eigene WAV über `CustomShoutFile`.
- Config-Migration: Rezepte und Herstellungsort auf altem Standard wechseln beim Start automatisch auf den neuen, damit Updates ohne Handarbeit ankommen.
- Benötigt weiterhin nur BepInEx und Jotunn (Harmony gehört zu BepInEx).

## 1.1.0

- Alle Drinks werden jetzt am Kessel hergestellt statt an der Werkbank.
  Bestehende Configs behalten ihren Wert, dort `CraftingStation = Cauldron` setzen.
- Neue Rezepte: Sugar Free aus Blaubeeren, Distel und Gelbem Pilz; Urban Classic mit Blutbeutel und
  Gelbem Pilz; Skyr Energy mit Ei; XXL aus Ashlands-Zutaten (Vineberries, Smoke Puffs) und Gelben Pilzen.
  Bestehende Configs behalten ihre alten `Ingredients`.
- Warnung im Log, wenn eine Zutat in der Config nicht existiert.
- Kong Strong XXL macht jetzt feuerfest (halber Feuerschaden), einstellbar über `FireResistance`.

## 1.0.0

- Erste Version mit vier Drinks: Sugar Free, Urban Classic (mit Zuckercrash), Skyr Energy und XXL-Kanister.
