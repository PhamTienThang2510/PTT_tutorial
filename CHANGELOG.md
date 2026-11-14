# Changelog

## [1.0.0] - 2026-11-14
### Changed
- Packaged as a UPM package (`com.ptt.tutorial`) with Runtime and Editor assemblies.
- Button and sequence IDs are now strings picked from a `TutorialIDDatabase` asset (dropdown in the Inspector) instead of per-project generated enums.
- Removed the NaughtyAttributes dependency (debug actions are now in the component's context menu).
- Editor-only code (`UnityEditor`) moved out of runtime code, so player builds compile.

### Fixed
- Tooltip pointed to the wrong place when the button was inside a LayoutGroup or a panel that was just activated.
- The last focused button stayed lifted above the overlay after the tutorial ended.
- A button that already had its own Canvas lost it after being focused.
- Tooltip position assumed the button's pivot was centred.

### Added
- `OnSequenceCompleted` event, `IsTutorialActive`, `HasSequence`, `RefreshButtons`, `UITutorialManager.Instance`.
- "Generate Constants" on the database: writes `TutorialButtonIDs` / `TutorialSequenceIDs` classes.
