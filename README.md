# SMZ3-Save-Fix
CLI program to fix the Tourian soft-lock in SMZ3 multiworld

## Purpose
In SMZ3 multiworld, it's possible for a player to lock themselves inside Tourian if they use the second save station in the area, after beating Ganon.
If the player still has checks in either game to perform, the player is unable to access these checks and would be required to forfeit.
This tool modifies the SMZ3 SRAM file to put the player back on the ship in Crateria and allow them to resume the game as normal.

## Usage
Simply drag the SRAM file on to the executable, or pass it in as an argument through a terminal.
The program will create a backup of the existing SRAM file, modify Samus' spawn position to be on the ship in Crateria, and then fix the checksum.
