# PTT_tutorial

Step-by-step UI tutorials for Unity uGUI. Each step dims the screen, lifts one button above the
overlay and shows a tooltip with an arrow pointing at it. The step advances when the player taps
that button, or taps anywhere for steps without a button.

Requires Unity 6 (6000.0+) with uGUI and TextMeshPro.

## Install

Pick one:

- **Git URL**: Package Manager › **+** › *Add package from git URL…*
  `https://github.com/PhamTienThang2510/PTT_tutorial.git#1.0.0`
- **Local folder**: Package Manager › **+** › *Add package from disk…* › select `package.json`.
- **Tarball**: Package Manager › **+** › *Add package from tarball…* (made with *Export* / `npm pack`).

Keep the `.meta` files when committing the package: scenes reference scripts and the prefab by GUID.

## Set up in a project

1. **ID database**: *Assets › Create › PTT › Tutorial ID Database*. Keep one per project.
   Add the button IDs (e.g. `btn_shop`) and sequence IDs (e.g. `seq_first_shop`) you need.
2. **Buttons**: add `UIButton` to each button the tutorial should point at and pick its ID from the dropdown.
   Buttons the tutorial never points at need nothing.
3. **Overlay**: add a full-screen `Image` (dark, semi-transparent, *Raycast Target* on) as the **last child** of a
   *Screen Space – Overlay* canvas, and turn it off.
4. **Manager**: add `UITutorialManager` to any GameObject. Assign the overlay and
   `Packages/com.ptt.tutorial/Runtime/Prefabs/tooltipUI.prefab` (or your own prefab with `TooltipUI`).
5. **Steps**: in the manager's *Tutorial Sequences*, add a sequence and its steps. For each step, set the
   button (None = tooltip in the centre, tap anywhere), the message, and optionally *On Step Completed*
   (e.g. open the panel the next step needs).
6. **Start it** from your code:

```csharp
tutorialManager.StartTutorial("seq_first_shop");
// or, after "Generate Constants" on the database:
tutorialManager.StartTutorial(TutorialSequenceIDs.seq_first_shop);
```

Steps are stored in the scene, on the `UITutorialManager` component. To share them between scenes, make the
manager a prefab.

## API

| Member | Use |
|---|---|
| `StartTutorial(string sequenceID)` | Run a sequence from step 0. |
| `EndTutorial()` | Stop and hide everything. |
| `IsTutorialActive`, `HasSequence(id)` | Query state. |
| `OnSequenceCompleted` (UnityEvent&lt;string&gt;) | Raised when a sequence finishes; save progress here. |
| `FocusOnButton(id, text, onClicked)` / `StopFocus()` | Point at one button outside a sequence. |
| `RefreshButtons()` | Call after spawning UI that contains new `UIButton`s. |
| `UITutorialManager.OnButtonFocus` (static event) | Raised with the focused ID (null = none), e.g. to add your own highlight effect. |

Debug: right-click the `UITutorialManager` header › *Debug / Start Test Sequence* (in Play mode).

## Notes and limits

- How a button is lifted: a `Canvas` with *Override Sorting* (order 10, set on `UIButton`) is added while it is focused, then
  removed. If the button already has its own Canvas, its settings are restored instead.
- A button inside a `ScrollRect` with a `Mask` is drawn outside the mask while focused, and the manager does not
  scroll to it: scroll it into view in the previous step's *On Step Completed*.
- Each ID should be used by one `UIButton` per scene. Duplicates log a warning.
- Renaming an ID in the database does not rename it where it is used. The Inspector marks those with ⚠.

## Sample

Package Manager › this package › *Samples* › **Main Menu Demo** › *Import*. Open `MainMenuTutorial.unity`, or
rebuild it with *PTT › Build Main Menu Tutorial Demo*.
