# UniGame.GameDB

A configurable game resource database system for Unity that provides centralized asset management, categorization, and loading capabilities with Addressables integration.

## Installation

### Package Manager
```
"com.unigame.gamedb": "https://github.com/UnioGame/UniGame.GameDB.git"
```

### Dependencies
- Unity Addressables 2.6.0+
- UniGame.Core
- UniGame.Localization
- UniGame.Rx
- UniTask
- R3
- Newtonsoft.Json

## Overview

UniGame.GameDB is a comprehensive resource management system that allows you to:
- Organize game assets into logical categories
- Load resources asynchronously with caching
- Support multiple resource providers (Addressables, custom providers)
- Provide fallback loading mechanisms
- Integrate with Unity's Addressable Asset System
- Use strongly-typed resource IDs for type safety

## Core Components

### GameDatabase

The main database class that manages resource categories and provides loading functionality.

```csharp
public class GameDatabase : IGameDatabase
{
    // Initialize the database with categories
    public async UniTask<IGameDatabase> Initialize(ILifeTime lifeTime);
    
    // Load a single resource
    public async UniTask<GameResourceResult> LoadAsync<TAsset>(string resourceId, ILifeTime lifeTime);
    
    // Load all resources matching a filter
    public async UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);
    
    // Find a resource record
    public GameDbResource Find(string filter);
    
    // Get a specific category
    public IGameDataCategory GetCategory(string category);
}
```

### Basic Usage

```csharp
// Initialize database
var database = new GameDatabase();
await database.Initialize(lifeTime);

// Load a texture
var result = await database.LoadAsync<Texture2D>("player_icon", lifeTime);
if (result.Complete)
{
    var texture = (Texture2D)result.Result;
    // Use the texture
}

// Load multiple resources
var results = await database.LoadAllAsync<AudioClip>("background_music", lifeTime);
foreach (var audioResult in results)
{
    if (audioResult.Complete)
    {
        var clip = (AudioClip)audioResult.Result;
        // Use the audio clip
    }
}
```

## Resource Categories

### IGameDataCategory

Categories organize resources into logical groups and provide search and filtering capabilities.

```csharp
public interface IGameDataCategory
{
    string Category { get; }
    IGameResourceProvider ResourceProvider { get; }
    IReadOnlyList<IGameResourceRecord> Records { get; }
    Dictionary<string, IGameResourceRecord> Map { get; }
    
    UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime);
    bool Has(string id);
    IGameResourceRecord Find(string filter);
    IReadOnlyList<IGameResourceRecord> FindResources(string filter);
}
```

### AddressableGameDataCategory

A category implementation that integrates with Unity's Addressable Asset System.

```csharp
[CreateAssetMenu(menuName = "Game/GameDatabase/Addressable Category")]
public class AddressableGameDataCategory : GameDataCategory
{
    public List<AddressablesObjectRecord> records;
    public AddressableFilterData filterData;
    
    // Automatically populate from Addressables
    public override IReadOnlyList<IGameResourceRecord> FillCategory();
}
```

#### Creating an Addressable Category

1. Right-click in Project window
2. Create → Game → GameDatabase → Addressable Category
3. Configure category name and filters
4. Use "Fill Category" to auto-populate from Addressables

```csharp
// Example category setup
var category = ScriptableObject.CreateInstance<AddressableGameDataCategory>();
category.category = "UI_Icons";
category.filterData.labels = new[] { "ui", "icons" };
category.filterData.regex = new[] { @".*UI.*\.png$" };
```

### Custom Categories

Create custom categories by inheriting from `GameDataCategory`:

```csharp
[CreateAssetMenu(menuName = "Game/GameDatabase/Custom Category")]
public class CustomGameDataCategory : GameDataCategory
{
    [SerializeField] private List<CustomResourceRecord> customRecords;

    public override Dictionary<string, IGameResourceRecord> Map => _map;
    public override IReadOnlyList<IGameResourceRecord> Records => customRecords;

    public override UniTask<CategoryInitializeResult> InitializeAsync(ILifeTime lifeTime)
    {
        // Initialize your custom category
        _map.Clear();
        foreach (var record in customRecords)
        {
            _map[record.Id] = record;
        }
        
        return UniTask.FromResult(new CategoryInitializeResult
        {
            category = this,
            complete = true,
            categoryName = Category
        });
    }

    public override IGameResourceRecord Find(string filter)
    {
        return customRecords.FirstOrDefault(r => r.CheckRecord(filter)) ?? EmptyRecord.Value;
    }

    public override IReadOnlyList<IGameResourceRecord> FindResources(string filter)
    {
        return customRecords.Where(r => r.CheckRecord(filter)).ToList();
    }
}
```

## Resource Providers

### IGameResourceProvider

Resource providers handle the actual loading of assets from different sources.

```csharp
public interface IGameResourceProvider
{
    bool IsValidResourceSource(string resource, Type resourceType);
    UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime);
}
```

### AddressableResourceProvider

Default provider for loading Addressable assets:

```csharp
public class AddressableResourceProvider : IGameResourceProvider
{
    public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime)
    {
        var addressableResult = await resource.LoadAssetTaskAsync<TResult>(lifeTime);
        
        return new GameResourceResult
        {
            Complete = addressableResult != null,
            Error = addressableResult == null ? $"Asset {resource} not found" : string.Empty,
            Result = addressableResult
        };
    }
}
```

### Custom Resource Providers

```csharp
public class WebResourceProvider : IGameResourceProvider
{
    public bool IsValidResourceSource(string resource, Type resourceType)
    {
        return resource.StartsWith("http://") || resource.StartsWith("https://");
    }

    public async UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime)
    {
        // Custom web loading logic
        try
        {
            var webRequest = UnityWebRequest.Get(resource);
            await webRequest.SendWebRequest().WithCancellation(lifeTime.Token);
            
            // Process result based on TResult type
            object result = ProcessWebResult<TResult>(webRequest);
            
            return new GameResourceResult
            {
                Complete = true,
                Result = result
            };
        }
        catch (Exception ex)
        {
            return new GameResourceResult
            {
                Complete = false,
                Error = ex.Message,
                Exception = ex
            };
        }
    }
}
```

## Resource Records

### IGameResourceRecord

Represents a single resource entry in the database:

```csharp
public interface IGameResourceRecord : ISearchFilterable
{
    string Name { get; }
    string Id { get; }
    bool CheckRecord(string filter);
}
```

### AddressablesObjectRecord

Record for Addressable assets:

```csharp
[Serializable]
public class AddressablesObjectRecord : IGameResourceRecord
{
    public string name;
    public AssetReference assetReference;
    public string[] labels;

    public string Id => assetReference.AssetGUID;
    public string Name => name;

    public bool CheckRecord(string filter)
    {
        if (string.IsNullOrEmpty(filter)) return false;
        if (!assetReference.RuntimeKeyIsValid()) return false;
        
        // Check GUID
        if (assetReference.AssetGUID.Equals(filter, StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Check labels
        return labels.Any(label => label.Equals(filter, StringComparison.OrdinalIgnoreCase));
    }
}
```

### Custom Resource Records

```csharp
[Serializable]
public class CustomResourceRecord : IGameResourceRecord
{
    public string resourceName;
    public string resourcePath;
    public string[] tags;

    public string Name => resourceName;
    public string Id => resourcePath;

    public bool CheckRecord(string filter)
    {
        return resourceName.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
               tags.Any(tag => tag.Contains(filter, StringComparison.OrdinalIgnoreCase));
    }

    public bool IsMatch(string searchString)
    {
        if (string.IsNullOrEmpty(searchString)) return true;
        return Name.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0 ||
               Id.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
```

## Type-Safe Resource IDs

### GameResourceRecordId

Strongly-typed resource identifier with editor dropdown support:

```csharp
[Serializable]
public struct GameResourceRecordId
{
    [SerializeField] private string value;

    public static implicit operator string(GameResourceRecordId v) => v.value;
    public static explicit operator GameResourceRecordId(string v) => new() { value = v };
}
```

### GameResourceCategoryId

Category identifier with type safety:

```csharp
[Serializable]
public struct GameResourceCategoryId
{
    [SerializeField] private string value;

    public static implicit operator string(GameResourceCategoryId v) => v.value;
    public static explicit operator GameResourceCategoryId(string v) => new() { value = v };
}
```

### GameResourceId

Combined category and record ID:

```csharp
[Serializable]
public struct GameResourceId
{
    public GameResourceCategoryId categoryId;
    public GameResourceRecordId id;

    public static implicit operator string(GameResourceId v) => v.id;
    public static explicit operator GameResourceId(string v) => new() { id = (GameResourceRecordId)v };
}
```

### Usage with Type Safety

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameResourceId playerIcon;
    [SerializeField] private GameResourceId playerSound;
    
    private async void Start()
    {
        // Load with type safety
        var iconResult = await database.LoadAsync<Sprite>(playerIcon, lifeTime);
        var soundResult = await database.LoadAsync<AudioClip>(playerSound, lifeTime);
        
        if (iconResult.Complete)
            playerImage.sprite = (Sprite)iconResult.Result;
            
        if (soundResult.Complete)
            audioSource.clip = (AudioClip)soundResult.Result;
    }
}
```

## Database Asset Configuration

### GameDataBaseAsset

Main configuration asset for the database:

```csharp
[CreateAssetMenu(menuName = "Game/GameDatabase/GameDataBaseAsset")]
public class GameDataBaseAsset : ScriptableObject
{
    public GameDatabase gameDatabase;
    
    // Editor utilities
    public static IEnumerable<ValueDropdownItem<GameResourceRecordId>> GetGameRecordIds();
    public static IEnumerable<GameResourceCategoryId> GetGameRecordCategories();
    
    [Button]
    public void UpdateData(); // Refresh all categories
}
```

### Setting Up Database Asset

1. Create the main database asset:
```csharp
// Right-click → Create → Game → GameDatabase → GameDataBaseAsset
```

2. Configure categories and providers:
```csharp
var databaseAsset = ScriptableObject.CreateInstance<GameDataBaseAsset>();
databaseAsset.gameDatabase.categories.Add(myAddressableCategory);
databaseAsset.gameDatabase.fallBack.Add(new AddressableResourceProvider());
```

3. Update category data:
```csharp
databaseAsset.UpdateData(); // Refreshes all categories from their sources
```

## Service Integration

### GameDataService

Service implementation for dependency injection:

```csharp
public interface IGameDataService : IGameService
{
    // Service interface
}

[Serializable]
public class GameDataService : GameService, IGameDataService
{
    // Service implementation
}
```

### GameDataServiceSource

Service source for creating and configuring the database service:

```csharp
[CreateAssetMenu(menuName = "Game/Services/Game Data/Game Data Source")]
public class GameDataServiceSource : DataSourceAsset<IGameDataService>
{
    public AssetReferenceT<GameDataBaseAsset> _dataBaseAsset;

    protected override async UniTask<IGameDataService> CreateInternalAsync(IContext context)
    {
        var lifeTime = context.LifeTime;
        
        // Load database asset
        var databaseAsset = await _dataBaseAsset
            .LoadAssetTaskAsync(lifeTime)
            .ToSharedInstanceAsync();

        // Initialize database
        var database = await databaseAsset
            .gameDatabase
            .Initialize(lifeTime);
        
        // Publish to context
        context.Publish<IGameDatabase>(database);

        return new GameDataService();
    }
}
```

### Using with Dependency Injection

```csharp
public class GameManager : MonoBehaviour
{
    [Inject] private IGameDatabase _database;
    
    private async void Start()
    {
        // Database is automatically injected and initialized
        var result = await _database.LoadAsync<GameObject>("player_prefab", lifeTime);
        
        if (result.Complete)
        {
            var prefab = (GameObject)result.Result;
            Instantiate(prefab);
        }
    }
}
```

## Advanced Features

### Filtering and Search

```csharp
// Filter configuration for Addressable categories
[Serializable]
public class AddressableFilterData
{
    public string[] folders;    // Filter by folder paths
    public string[] labels;     // Filter by Addressable labels
    public string[] regex;      // Filter by regex patterns
}

// Usage
var filterData = new AddressableFilterData
{
    labels = new[] { "ui", "hud" },
    regex = new[] { @".*Icon.*\.png$" },
    folders = new[] { "Assets/UI/Icons" }
};
```

### Caching and Performance

```csharp
public class GameDatabase : IGameDatabase
{
    // Built-in caching for performance
    private Dictionary<string, GameDbResource> _dbResourceCache = new(256);
    private Dictionary<string, GameDbResource[]> _dbResourcesCache = new(256);
    
    // Cached find operations
    public GameDbResource Find(string filter)
    {
        if (_dbResourceCache.TryGetValue(filter, out var cached))
            return cached;
            
        // Perform search and cache result
        var result = SearchCategories(filter);
        _dbResourceCache[filter] = result;
        return result;
    }
}
```

### Fallback Loading

```csharp
// Configure fallback providers
var database = new GameDatabase();
database.fallBack.Add(new AddressableResourceProvider());
database.fallBack.Add(new ResourcesProvider());
database.fallBack.Add(new WebResourceProvider());

// Database will try providers in order until one succeeds
var result = await database.LoadAsync<Texture2D>("missing_texture", lifeTime);
```

### Editor Integration

```csharp
#if UNITY_EDITOR
public class DatabaseEditorTools
{
    [MenuItem("Tools/GameDB/Refresh All Categories")]
    public static void RefreshAllCategories()
    {
        var databases = AssetDatabase.FindAssets($"t:{nameof(GameDataBaseAsset)}");
        
        foreach (var guid in databases)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var database = AssetDatabase.LoadAssetAtPath<GameDataBaseAsset>(path);
            database.UpdateData();
        }
    }
    
    [MenuItem("Tools/GameDB/Validate Database")]
    public static void ValidateDatabase()
    {
        // Validation logic for database integrity
    }
}
#endif
```

## Best Practices

### 1. Category Organization
```csharp
// Organize by logical groups
- UI_Elements (buttons, icons, panels)
- Audio_Effects (sounds, music)
- Character_Assets (models, textures, animations)
- Environment_Assets (props, materials, prefabs)
```

### 2. Resource Naming
```csharp
// Use consistent naming conventions
"player_icon_64x64"
"background_music_menu"
"effect_explosion_large"
"ui_button_primary"
```

### 3. Performance Optimization
```csharp
// Preload frequently used resources
var criticalResources = new[] { "player_prefab", "ui_canvas", "game_manager" };
var preloadTasks = criticalResources.Select(id => database.LoadAsync<GameObject>(id, lifeTime));
await UniTask.WhenAll(preloadTasks);

// Use lifetime management for automatic cleanup
using var resourceLifetime = new LifeTime();
var result = await database.LoadAsync<Texture2D>("temporary_texture", resourceLifetime);
// Automatically cleaned up when resourceLifetime is disposed
```

### 4. Error Handling
```csharp
public async UniTask<T> LoadResourceSafely<T>(string resourceId, ILifeTime lifeTime) where T : Object
{
    try
    {
        var result = await database.LoadAsync<T>(resourceId, lifeTime);
        
        if (!result.Complete)
        {
            Debug.LogError($"Failed to load resource '{resourceId}': {result.Error}");
            return null;
        }
        
        return (T)result.Result;
    }
    catch (Exception ex)
    {
        Debug.LogException(ex);
        return null;
    }
}
```

### 5. Type Safety
```csharp
// Use strongly-typed resource IDs
public class GameAssets : ScriptableObject
{
    [Header("UI Assets")]
    public GameResourceId mainMenuBackground;
    public GameResourceId buttonClickSound;
    
    [Header("Player Assets")]
    public GameResourceId playerPrefab;
    public GameResourceId playerIcon;
    
    // Validation
    private void OnValidate()
    {
        ValidateResourceId(mainMenuBackground, "Main Menu Background");
        ValidateResourceId(buttonClickSound, "Button Click Sound");
    }
    
    private void ValidateResourceId(GameResourceId resourceId, string description)
    {
        if (string.IsNullOrEmpty(resourceId))
            Debug.LogWarning($"{description} resource ID is not set");
    }
}
```

## Troubleshooting

### Common Issues

1. **Resource Not Found**
```csharp
// Check if resource exists in database
if (!database.Find("my_resource").success)
{
    Debug.LogError("Resource 'my_resource' not found in database");
}
```

2. **Category Not Initialized**
```csharp
// Ensure database is initialized before use
if (database == null)
{
    Debug.LogError("Database not initialized");
    return;
}
```

3. **Addressable Asset Not Loaded**
```csharp
// Verify Addressable setup
var record = category.Find("asset_name");
if (record is AddressablesObjectRecord addressableRecord)
{
    if (!addressableRecord.assetReference.RuntimeKeyIsValid())
    {
        Debug.LogError($"Invalid Addressable reference for {record.Name}");
    }
}
```

### Debug Utilities

```csharp
public static class GameDBDebug
{
    public static void LogDatabaseState(GameDatabase database)
    {
        Debug.Log($"Database has {database.categories.Count} categories");
        
        foreach (var category in database.categories)
        {
            var asset = category.editorAsset;
            if (asset != null)
            {
                Debug.Log($"Category '{asset.Category}' has {asset.Records.Count} records");
            }
        }
    }
    
    public static void ValidateResourceIds(GameResourceId[] resourceIds)
    {
        foreach (var id in resourceIds)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"Empty resource ID found");
            }
        }
    }
}
```

## Features Summary

- **Centralized Asset Management**: Single point of access for all game resources
- **Type Safety**: Strongly-typed resource IDs with editor validation
- **Flexible Loading**: Support for multiple resource providers and fallback mechanisms
- **Performance Optimized**: Built-in caching and async loading
- **Editor Integration**: Visual tools for database management and validation
- **Addressables Support**: First-class integration with Unity's Addressable Asset System
- **Extensible Architecture**: Easy to add custom categories and resource providers
- **Search and Filtering**: Powerful filtering capabilities for large asset collections
- **Lifetime Management**: Automatic resource cleanup with UniGame.Core integration
- **Service Integration**: Works with dependency injection systems