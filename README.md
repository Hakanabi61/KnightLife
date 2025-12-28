# KnightLife

## Dungeon UI System

### Overview
The dungeon UI system provides a runtime-created, mobile-safe interface for dungeon encounters and player choices. The UI is created at runtime to avoid binary scene file changes and uses SendMessage for loose coupling between UI and game logic.

### Components

#### SafeArea.cs
Handles safe area insets for mobile devices with notches or rounded corners. Automatically adjusts the UI to fit within the safe area.

#### PanelController.cs
Provides fade-in/fade-out transitions for UI panels with raycast blocking during transitions.

#### EncounterPanelView.cs
Displays encounter information with title, description, and action buttons (Fight/Flee).

#### ChoicePanelView.cs
Displays a list of choices with dynamically spawned buttons from a prefab.

#### DungeonUIBootstrap.cs
Automatically creates the UI hierarchy at runtime:
- UIRoot (Canvas with CanvasScaler)
- SafeArea
- EventSystem (with InputSystemUIInputModule)
- EncounterPanel
- ChoicePanel

The bootstrap runs automatically on scene load via `[RuntimeInitializeOnLoadMethod]`.

#### DungeonUIController.cs
Integrates the UI panels with DungeonManager using SendMessage for loose coupling.

**Usage:**
1. Attach DungeonUIController to any GameObject in your scene (typically the DungeonManager)
2. The bootstrap will automatically link the encounter and choice views
3. Call public methods to show/hide panels:
   - `ShowEncounter(title, description)` - Shows encounter panel
   - `HideEncounter()` - Hides encounter panel
   - `ShowDefaultChoices()` - Shows choice panel with default options
   - `ShowChoices(header, options)` - Shows choice panel with custom options
   - `HideChoices()` - Hides choice panel

**SendMessage Integration:**
The controller uses SendMessage to call methods on DungeonManager without hard compile-time dependencies. If a method doesn't exist, no exception is thrown.

Default method mappings:
- Fight button → `StartFight()`
- Flee button → `TryFlee()`
- Shop choice → `OpenShop()`
- Chest choice → `OpenChest()`
- Boss choice → `GoToBoss()`
- Continue choice → `ContinueExploration()`

You can customize these method names in the inspector.

### Integration with DungeonManager

The DungeonManager now includes stub methods for UI integration:
- `StartFight()` - Called when player clicks Fight
- `TryFlee()` - Called when player clicks Flee
- `OpenShop()` - Called when player selects Shop
- `OpenChest()` - Called when player selects Chest
- `GoToBoss()` - Called when player selects Boss
- `ContinueExploration()` - Called when player selects Continue

These methods currently call `CompleteEncounter()` but can be extended with custom logic.

### Testing

The system includes a smoke test that shows an encounter panel on startup. This will be replaced by actual game logic once integrated.

To test:
1. Enter Play mode
2. The UI should appear with proper layout and safe area handling
3. Click buttons to verify SendMessage integration
4. Check console for debug logs showing method calls

