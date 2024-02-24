# Senior Unity Programmer test task

This project contains the implementation of a a test task for a Senior Unity Developer position.

## Gameplay Footage (Solution)
![alt text](Recordings/gameplay.gif)

## Task description

We would like to ask you to implement a grid-based game. It will not have a win or lose
condition.
Please avoid using third party assets and packages, with the exception of the standard Unity
packages like TextMeshPro etc.

Features:
- A grid of 2D slots
- The width and height of the grid should be loaded from a text file in any format
(JSON, binary, xml etc)
- Each slot should have alternating colors in a chessboard pattern
- Each slot when created should have a 25% chance of being blocked - denoted
by a darker color
- The grid should have its center in the middle of the scene (coordinates 0,0,0)
- At the start of the game a 2D object called “Spawner” should be placed in the middle of
the grid
- Upon pressing and holding down a “Spawn” button on the UI the spawner should
spawn colored items in the nearby unblocked and empty slots (the placement of
the UI elements is up to you)
- The spawner should spawn one item per frame while the UI button is held down
- The spawner should search for available slots in a circle around itself using the
clockwise direction (top -> top-right -> right -> bottom-right -> bottom - >
bottom-left -> left -> top-left)
- The color of the item should be selected from a collection of 3 possible colors
(the colors are up to you)
- The user should be able to drag and drop the spawner across the grid: upon
releasing the spawner over a slot, the spawner should find the nearest
unblocked and empty slot and snap to it
- Upon spawning an item it should animate from the spawner’s position to its
designated target slot with a short, linear animation
- The user should be able to clear adjacent items from the board by clicking a UI button
“Clear”
- Items are considered adjacent to each other when they are situated in
neighbouring slots (up, right, bottom, left, no diagonal) and have the same
color
- Upon pressing the button “Clear” all chains of neighbouring items should
simultaneously disappear from the board leaving empty slots
- Camera
- Please implement the ability to pan the camera to be able to see the entire grid
by holding down left mouse button and dragging the mouse across the screen
- Please implement a UI slider that allows to zoom the camera in and out (max
zoomed out camera should display the entire grid)
- Tip: use orthographic camera for this task


Requirements for the task:
- The visual aspect of the game will not be evaluated so please use simple sprites to
denote elements of the game (example: squares for slots, white circle for spawner,
colored circles for items)
- The readability and simplicity of the code will be taken into account when evaluating the
task
- The finished game will be evaluated with small 2x2 grids as well as large 250x250+
grids. Although the large number of slots and items may impact the FPS due to
rendering costs, please make sure that the algorithm used for spawning and clearing
items has as little impact on the FPS as possible. Ideally the FPS should remain
consistent whether one is holding down the spawn button or not, regardless of grid
dimensions

Optionally:
- Provide a working Android build of the game along with the repository link
- Add an FPS counter to the build to track the performance of the algorithms used

  
## The Result
### Algorithms implementation notes
- Clearing the neighbouring cells that share the same colour:
  - DFS algorithm due to its simplicity, and its computational cost O(N).
  - Recursive version of the algorithm has been chosen due to being simpler and more intuitive, even at a cost of potential increased performance cost.
