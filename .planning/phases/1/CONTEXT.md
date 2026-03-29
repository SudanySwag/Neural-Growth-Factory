# Phase 1: Intro Cutscene

## Goal
Add an intro cutscene sequence that plays when the game starts, before gameplay begins.

## Requirements

### Sequence 1: Void & Cursor Reveal
1. Show an uninteractable background with nothing spawned — just the empty environment for a few seconds
2. Fade in a custom mouse cursor
3. Hide the default system cursor during the cutscene
4. When the player moves the custom cursor around and passes over the center of the screen, show the caption "Look into the void" underneath the cursor
5. On click (while caption is visible), transition to the next cutscene/gameplay

### Constraints
- No cells should be spawned during the intro
- All normal gameplay UI (sidebar, counters, upgrade trees) must be hidden during cutscene
- The custom cursor should follow the mouse position smoothly
- The "Look into the void" caption should appear only when the cursor is near the center of the screen
- Must integrate cleanly with the existing scene (Neurosphere.unity) and MonoBehaviour architecture
- Should use UI Toolkit for any overlay UI (consistent with project conventions)

## Success Criteria
- [ ] Game starts with empty background, no cells, no UI
- [ ] Custom cursor fades in after initial delay
- [ ] System cursor is hidden during cutscene
- [ ] "Look into the void" appears when cursor passes over screen center
- [ ] Clicking while caption is shown advances past the intro
- [ ] Normal gameplay begins after cutscene completes
- [ ] Cutscene can be skipped or only plays once per session
