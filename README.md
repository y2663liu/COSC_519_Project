 Courtyard Reunion VR Experience

## Overview
The Courtyard Setup scene (located under `Assets/UBCO Courtyard Assets`) delivers an interactive VR mystery: you are walking your dog through the courtyard when the leash slips, forcing you to follow environmental clues to reunite. The gameplay loop progresses through narrative stages that blend hand-controlled locomotion, interactive hints, and atmospheric cues such as audio barks and glowing paw-print props.

This README documents the current runtime systems, how they work together, and how to reuse every script under `Assets/Scripts` when extending the experience.

## Core Game Flow
### GameStateManager (`Assets/Scripts/GameStateManager.cs`)
* Maintains a singleton (`Instance`) that persists between scene loads (`DontDestroyOnLoad`).
* Defines the `GameStage` enum: `Intro → WalkWithDog → DogRanAway → Search → Reunited`.
* Raises `OnStageChanged` whenever the stage transitions and `OnPlayerMovementStateChanged` when player locomotion should be locked/unlocked.
* Exposes `SetStage` and `SetPlayerMovementEnabled` to other systems; redundant requests are ignored to avoid duplicate events.
* All stage-aware scripts subscribe to the events in `Awake` and enable/disable themselves based on the active stage.

> **Usage:** Add this component to an empty GameObject in the scene (preferably via prefab). Configure the desired `startingStage` and whether the player can move at boot. All other scripts find it via `GameStateManager.Instance`.

## UI & Guidance Systems
### HintPopup (`Assets/Scripts/HintPopup.cs`)
* Singleton utility that shows a world-space canvas anchored relative to the headset (`Camera.main`).
* `ShowHint(title, body, footer, source)` sets TMP labels and keeps the popup facing the camera at a fixed distance with a vertical offset. The `source` transform is stored to prevent other objects from hiding the popup prematurely.
* `HideHint(source)` only closes the popup if the requesting transform matches the currently displayed hint; `HideImmediate` turns off the canvas right away.

> **Usage:** Place the prefab containing `HintPopup` in the scene once. Other scripts call `HintPopup.Instance?.ShowHint(...)` and `HideHint(...)` to deliver tutorial prompts or clue text.

## Player Experience Controllers
### IntroStageController (`Assets/Scripts/Player/IntroStageController.cs`)
* Guides players through snap-turning, smooth locomotion, and teleportation via a sequence of `TutorialPage`s.
* Uses `InputActionProperty` references for “next” and “previous” page buttons; listeners are attached in `OnEnable`/`OnDisable`.
* Tracks player orientation/position deltas each frame to determine when the current tutorial task is complete (`EvaluateRotation`, `EvaluateMovement`, `EvaluateTeleport`). When the player performs the required motion, `AdvancePage()` displays the next prompt and eventually bumps the stage to `WalkWithDog`.
* Automatically forces the `GameStateManager` into the `Intro` stage at `Start` to guarantee consistent state when entering the scene.

> **Usage:** Attach to a tutorial manager object. Assign the XR rig transform to `playerTransform` (or rely on the Player-tagged object lookup). Map the `nextPageAction`/`previousPageAction` to your controller inputs and customize page text inside `BuildPages()`.

### WalkWithDogStageController (`Assets/Scripts/Player/WalkWithDogStageController.cs`)
* Shows final guidance after the tutorial: a glowing `sceneMarker` GameObject appears and a hint instructs the player to reach it.
* Listens for the stage transition to `WalkWithDog` and self-enables only while active.
* Uses the `closePageAction` input to let the player dismiss the hint prompt.
* Continuously measures 2D distance between the player and the marker; upon reaching `markerArrivalDistance`, hides the marker and advances the `GameStateManager` to `DogRanAway` to kick off the chase sequence.

> **Usage:** Reference a marker prefab (e.g., a light beam). Set `markerArrivalDistance` to the acceptable tolerance. Bind `closePageAction` to the same “A”/“X” button that dismisses popups.

## Dog Behaviour Controllers
### DogFollowAI (`Assets/Scripts/Dog/DogFollowAI.cs`)
* Keeps the dog roughly beside the player during the opening walk. Desired position is offset forward/right relative to the player and smoothed using `Vector3.MoveTowards`.
* Constrains rotation to the planar forward direction and cross-fade into the “Walk” animation layer via `Animator.SetLayerWeight`.
* Responds to stage changes: enabled only during `Intro` and `WalkWithDog`.

### DogPlayAI (`Assets/Scripts/Dog/DogPlayAI.cs`)
* Used when the dog is romping around an area. Establishes a square playground centered on its initial position (`width`) and keeps a minimum distance (`minDistance`) from the player.
* When the player approaches, the dog computes a desired point either on the circle around the player or at the farthest square corner, clamped inside the boundaries, then moves there at `speed`.

### DogRunAwayAI (`Assets/Scripts/Dog/DogRunAwayAI.cs`)
* Drives the dramatic escape. On activation, the dog grabs its current vector away from the player, accelerates (`RunAwayAcceleration`) while playing a bark audio cue, and despawns after `DespawnDelaySeconds`.
* When the despawn timer ends, it signals the `GameStateManager` to enter the `Search` stage so clues become active.

> **Usage for dog scripts:** Each behaviour script expects a reference to the Player-tagged transform (auto-detected in `Awake`). Assign an `Animator` with matching layer names (“Walk” or “Run”) and (for run-away) an `AudioSource`/`AudioClip`. Enable the appropriate script based on the scene beat; the `GameStateManager` handles the rest through stage events.

## Controller Utilities
### TeleportationActivator & RayInteractionActivator (`Assets/Scripts/Controller/*.cs`)
* Both scripts wrap XR Interaction Toolkit ray interactors so that the ray only appears while a button is held.
* Each script references an `XRRayInteractor` and an `InputActionProperty` (e.g., trigger press). In `Start`, the ray object is deactivated and a listener turns it on when the action is performed. The `Update` loop watches `WasReleasedThisFrame()` to hide the ray immediately when the button is released.

> **Usage:** Attach each script to the controller game object that also holds the corresponding `XRRayInteractor`. Drag in the interactor and the Input Action (from your `Input Action Asset`).

## Clue & Interaction System
Stage-aware interactables derive from `InteractableBase`, ensuring they only function during the `Search` stage.

### InteractableBase (`Assets/Scripts/Interactables/InteractableBase.cs`)
* Subscribes to `GameStateManager.OnStageChanged` and toggles its own `enabled` flag so derived scripts activate automatically when the search begins.

### ProximityInteractableBase (`Assets/Scripts/Interactables/ProximityInteractableBase.cs`)
* Extends `InteractableBase` with proximity detection logic: finds the Player transform and fires `OnPlayerEnteredRange` once the player is within `activationRadius`. Uses a slightly larger `deactivateRadius` to prevent rapid toggling.
* Child classes (SoundClue & VisualClue) override the entry/exit callbacks to define the actual interaction.

### HoverClue (`Assets/Scripts/Interactables/HoverClue.cs`)
* Listens to XR ray hover events via `XRBaseInteractable`. When the player points at the object, it displays a hint describing title, clue text, and additional flavor (“fun facts”).

### SoundClue (`Assets/Scripts/Interactables/SoundClue.cs`)
* Requires an `AudioSource` on the same GameObject. When the player enters range, optionally loops (`loopSoundWhileNearby`) a 3D audio clip (set to `spatialBlend = 1`); stops when the player leaves.

### VisualClue (`Assets/Scripts/Interactables/VisualClue.cs`)
* Highlights renderers by enabling material emission with the configured color/intensity when the player is close. Caches each renderer’s original emission color so it can be restored when the player leaves.
