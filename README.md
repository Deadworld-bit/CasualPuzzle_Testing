[![CasualPuzzle_Testing | © 2025 by Deadworld ]](https://github.com/RobloxGL/Phan-Thanh-Duc.git)
# Welcome to CasualPuzzle_Testing repository

## Overview
In this project, the player guides a Main Character (MC) along a winding path on an isometric map. The MC follows a red dot controlled by touch or mouse input, navigating through enemies and environmental obstacles to reach the end of each level within a time limit.

## Gameplay
### Camera
* Mode: Landscape isometric.
* View: Always frames the entire map, regardless of size.

### Controls
* Mobile: Hold and drag to move the red dot.
* PC: Click and hold the left mouse button to drag the red dot.

### Mechanics
* Health: Player starts with 3 HP.
* Damage: Colliding with enemies or off-path cells destroys the environment cell, plays an effect and -1 HP.
* Win: Reach the End cell to trigger a win effect and calculate score based on completion time.
* Game Over: HP reaches 0 or timer reach 0, show ending effect and accumulated score, then offer replay.
* Progression: Each level increases in difficulty.

## Level Design
### Grid Map Generation:
* Grid Size (W x H): Can be configure.
  
### Start & End Points
* Start (S) and End (E) cells chosen randomly on the map edges.
* Start (S) and End (E) cells always on the opposite side of the map (prevent Start and End cells too close to each other).

### Paths & Alternatives
* Main Path (P): Randomized.
* Randomize from 1x1, 2x2 and 3x3 cells.

### Environment & Enemies
* Environment cells: Non-path cells populated with decorative environment objects.
* Enemies: Spawn off-path, patrol across or alongside paths.
* Number of enemies increase base on level.

