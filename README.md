# HerbivorousWolf-
Intro to Game Development - Assignment 3 (Game Recreation)

Estimated Total Hrs Invested: 50hrs 

Theme: Evil Bunnies and the WereWolf

Story:

There was once a wolf who had been raised as a herbivore. Every day, it ate berries until a horde of evil bunnies invaded its home. Due to his lack of bloodlust, the bunnies displayed no fear and ganged up on him. Until he has had enough of being a herbivore, he decided to embark on a new adventure as a carnivore. Consuming meat and strengthening its resolve to assassinate anyone who gets in its way. 

Description: 

This is a replica of the classic Pac-Man game. Similar to the original, the player will have to consume all the berries in the shortest time possible whilst avoiding the deadly bunnies. 

Unique Game Mechanics:

- A special bonus "Golden Orange" will periodically fly across the map. If consumed, the player is rewarded huge pts.  

- A Procedural Level Generator to generate maps from 2D arrays. However, there are some restrictions to how the 2D array is designed such as an Inner Wall is prohibited from being next to an Outer Wall.

- 4 Bunnies, each with different Movement: Chase The Wolf, Run From The Wolf, Move Along The Edge of the Map, Random. 

Game Design And Implementation:
As a challenge, Unity Pathfinding, TileMap and other External Libraries were not used.

- The Procedural Map Generator uses basic adjacent tiles checking to determine its rotation. 
Improvements: BFS could be used here to reduce the restrictions on how the 2D map is designed. 

- The Map Generator generates one quadrant and duplicates 3 more times to form an entire map. This will prevent having to store large 2D array maps. 

- Bunny 1's Chase The Wolf Algorithm uses a basic adjacent tiles checking along with squared magnitude to determine the next path to take. 
Improvements: To ensure the shortest path to the wolf, an A* Algorithm could be used to calculate on every frame. To avoid repeated calculations, memoization could be implemented using a HashMap.

- The Wolf/Player's Collision Detection with the Wall is done without a Collider, but instead achieved by checking against the 2D array. 

- Similarly for the Teleporter, it is done by checking if the player is at the edge of the map. 
