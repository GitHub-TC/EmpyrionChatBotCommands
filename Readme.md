# Empyrion Chat Bot Commands

## Installation
Sie können diesen Mod direkt mit dem MOD-Manager von EWA (Empyrion Web Access) laden. <br/>
Ohne den EWA funktioniert der Mod nur innerhalb des EmpyrionModHost

## Implementierte Kommandos
- CB:Reset\
  Kicked den Spieler vom Server und löscht ihn, so dass er komplett neu starten kann
- CB:SetHome:[ID]\
  Setzt die Struktur (CV,BA) mit der ID als "Heimat" zu der mit CB:GoHome zurückgekehrt werden kann
- CB:GoHome\
  Zur "Heimat" zurückkehren. Muss zuvor mait CB:SetHome:ID einmalig eingestellt werden
- CB:GetShipDown:[ID]\
  Positioniert ein Schiff über dem Spieler so das es in Reichwrite der Remoteconsole ist und nach einem Ausschalten hinunterfällt
- CB:GetShipHere:[ID]\
  Teleportiert das Schiff (CV,SV,HV) zu dem Spieler
- CB:GotoShip:[ID]\
  Teleportiert den Spieler zu dem Schiff (CV,SV,HV)

## Konfiguration
In der Konfigurationsdatei der Mod im Savegameverzeichnis können diverse Timeouts eingestellt werden und welche Spieler z.B. "Heimatstrukturen" eingetragen haben

# Empyrion Chat Bot Commands

## Installation
Your can direct load this mod with the EWA (Empyrion Web Access) MOD manager.<br/>
Without the EWA the mod works only within the EmpyrionModHost

## Implemented commands
- CB:Reset\
  Kicks the player from the server and deletes him so that he can restart completely
- CB:SetHome:[ID]\
  Sets the structure (CV,BA) with the ID as "home" to which you can return with CB:GoHome
- CB:GoHome\
  Return to the "home". Must be set once beforehand with CB:SetHome:ID
- CB:GetShipDown:[ID]\
  Positions a ship above the player so that it is within range of the remote console and falls down after switching off
- CB:GetShipHere:[ID]\
  Teleports the ship (CV,SV,HV) to the player
- CB:GotoShip:[ID]\
  Teleports the player to the ship (CV,SV,HV)

## Configuration
In the configuration file of the mod in the savegame directory, various timeouts can be set and which players have entered "home structures", for example
