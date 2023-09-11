# Cherrypick-test-task

Author: Gerard Cuello Adell

## Assumptions

- The grid has to have same number of rows and columns
- If the width and height are **even numbers**, the bottom-left cell will be taken as center, since there are technically four centers.
- The middle cell will never be blocked when generating the Grid

## Algorithms
- Clearing the neighbouring cells that share the same colour:
  - DFS algorithm due to its simplicity, and its computational cost O(N).
  - Recursive version of the algorithm has been chosen due to being simpler and more intuitive, even at a cost of potential increased performance cost.

## Architecture
- New Input System to makes the code less cumbersome while adding support for mouse and touch.