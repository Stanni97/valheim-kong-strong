# Changelog

## 1.2.1

- Pfandautomat deutlich größer. Die Textur des Automaten wiederholte sich senkrecht, weil das kopierte Material seine Kachelung mitbrachte; sie wird jetzt auf 1:1 gesetzt (betrifft alle Baustücke).
- Das Leergut ist jetzt eine unbeschädigte graue Dose in voller Größe statt einer zerdrückten.
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
