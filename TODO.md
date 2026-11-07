# TODO

## XXX WebGL build (invisible levels / video stutter)
- [x] Fully close and reopen the Unity Editor so the Addressables group schemas reload from disk (they were edited externally while the Editor was open, which is why the LZ4 fix didn't take effect on the last build).
- [x] In **Window → Asset Management → Addressables → Groups**, click each group and confirm the Inspector shows **LZ4** compression (not LZMA).
- [x] **Build → Clean Build → All**, then **Build → New Build → Default Build Script** to regenerate bundles with LZ4.
- [x] Rebuild the WebGL player and confirm the home screen panorama and tutorial level are visible again.
- [x] Re-check the start menu video after the rebuild — if it still stutters/catches up once Addressables loads cleanly, that points to a separate, known WebGL `VideoPlayer` sync quirk (browser video element vs. Unity frame sampling), not the Addressables issue.

## Mobile/WebGL performance follow-ups
- [ ] Split and decimate the two oversized raw FBX scenes in `Assets/Prefab/Supermarket Models/Shelves/` — `food_cans.fbx` (68MB) and `supermarket_assets_V2.fbx` (63MB) — into individual models with LODs.
- [ ] Re-encode `Assets/Videos/Askfirst.mp4` (100MB) at a lower mobile-friendly bitrate/resolution.
- [ ] Decide whether to also physically shrink the oversized source texture files on disk (canned-goods PBR sets, Planogram boards) — the import-setting fix already caps them at build time, but the large source files still bloat the git repo/Library cache.
- [x] Clean up `NPCBehavior.cs`: cached `GameObject.FindGameObjectWithTag("Player")` once in `Start()` instead of calling it every frame; replaced redundant `GetComponent<NavMeshAgent>()` in `Leaving()` with the existing `agent` field.
- [x] Did a script-based "unused asset" audit (cross-referenced every asset GUID against every scene/prefab/material/controller that could reference it, since the Editor's finder isn't drivable from here). Removed the confirmed-dead `Assets/Resources/Old versions/` folder (superseded by `RemoteLoader.cs`'s remote dialogue loading) and 6 orphan scenes not in Build Settings or referenced anywhere (`Modular.unity`, `NPCTexturingTest.unity`, `AnimationTestScene.unity`, both Starter Assets `Playground.unity` demo scenes, QuickOutline's sample scene).
- [ ] Follow-up: the same audit flagged ~130MB of likely-orphaned textures/materials (mostly loose numbered textures and `Lata_*` PBR maps under `Assets/Prefab/Supermarket Models/Shelves/canned/`, plus orphans in `Materials/otherMat/`, `NPCs/Materials/`, `Textures/Planogram/`, and loose NPC animation FBXs). Not deleted yet — GUID cross-referencing can't catch assets loaded by string path (Resources/Addressables), so these need either a manual look or a pass through the Editor's actual unused-asset finder before removal. Overlaps with the FBX-splitting task above.

## Git housekeeping
- [ ] Review `git status` and commit (when ready) the pending changes from this session: texture `maxTextureSize` reductions (71 files), removed clutter (`Assets/_Recovery`, `Assets/TutorialInfo`, `Assets/Readme.asset`, `Data/`), `.gitignore` additions, WebGL memory settings, and Addressables LZ4 compression fix.

## Level intro feature (popup + camera pan)
`LevelIntro.cs` script and `GameManager` hookup are done. Per level scene (Level1–Level4, Level5 if it exists), still need to wire up in the Editor:
- [x] Add an `IntroCamera` (`CinemachineCamera`, Priority 0, no Follow/LookAt).
- [x] Place ordered pan waypoints (empty GameObjects) marking the camera path over the level.
- [x] Build a `LevelIntroUI` Canvas with level-name text, level-description text, and a "tap to start" button.
- [x] Add a `LevelIntro` component on an empty GameObject, fill in Level Name/Description, and wire up the canvas/text/button and camera/waypoints references.
- [-] Decide if a prebuilt `LevelIntroUI` prefab (instead of building the canvas from scratch per scene) is wanted — offered, not yet built.

## Safe distance / "S" & "K" reminder system
`SafeDistanceIndicator.cs`, `SafeDistanceWarningUI.cs`, `AskReminderPopup.cs` are written, and `NPCBehavior.cs`/`TutorialNPC.cs`/`PlayerDialogue.cs` are already wired to use them. Still need to wire up in the Editor:
- [ ] On the shared NPC prefab (`Assets/Prefab/NPC (Change This Name).prefab`): add a child `SafeDistanceCircle` GameObject (at the NPC's feet) with a `LineRenderer` (thin width, simple transparent/unlit material) + the `SafeDistanceIndicator` script, then drag it into `NPCBehavior` → **Safe Distance Indicator**. Save the prefab so every level's NPC instances pick it up.
- [ ] Repeat the same wiring for the Tutorial NPC if it uses `TutorialNPCBehavior` instead.
- [ ] Per scene (each level + Tutorial): add a small panel with `CanvasGroup` + `TextMeshProUGUI` under the `GameUI` canvas, add the `SafeDistanceWarningUI` script, assign those two fields (one per scene — it's a singleton).
- [ ] Per scene: add a new `Canvas` with a body `TextMeshProUGUI` + close `Button`, add the `AskReminderPopup` script, assign **Reminder Canvas / Body Text / Close Button** (one per scene — it's a singleton). Default text is already the suggested S/K copy.
- [ ] Decide whether to keep reusing mistake id `4` for the end-of-conversation safety ding (preserves existing End Screen scoring) or change/drop it if it should be purely educational.
