# UniGame.GameDB

A configurable game resource database system for Unity that provides centralized asset management, categorization, and loading capabilities with Addressables integration.

# Installation

## Package Manager
```
"com.unigame.gamedb": "https://github.com/UnioGame/UniGame.GameDB.git"
```

## Dependencies

```json
  "dependencies": {
    "com.unity.addressables": "2.6.0",
    "com.unigame.unicore": "https://github.com/UnioGame/unigame.core.git",
    "com.unigame.rx": "https://github.com/UnioGame/unigame.rx.git",
    "com.cysharp.unitask" : "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask",
    "com.cysharp.r3": "https://github.com/Cysharp/R3.git?path=src/R3.Unity/Assets/R3.Unity",
    "com.github-glitchenzo.nugetforunity": "https://github.com/GlitchEnzo/NuGetForUnity.git?path=/src/NuGetForUnity",
    "com.unity.nuget.newtonsoft-json": "3.2.1"
  }
```

# Overview

UniGame.GameDB is a comprehensive resource management system that allows you to:
- Organize game assets into logical categories
- Load resources asynchronously with caching
- Support multiple resource providers (Addressables, custom providers)
- Provide fallback mechanisms for resource loading

# Core Components

## DB Source

The main entry point for the game database, responsible for initializing and providing access to the database.

```csharp
[CreateAssetMenu(menuName = "UniGame/Game DB/Game DB Source", fileName = "Game DB Source")]
public class GameDataServiceSource : DataSourceAsset<IGameDataService>
```

## GameDatabase

To create all base game DB functionality call menu:

```csharp
"Assets/UniGame/Game DB/Create Game DB"
```

The main database class that manages resource categories and provides loading target asset by id

```csharp
public interface IGameDatabase : IGameResourceProvider
{
    bool IsValidResourceSource(string resource,Type resourceType);
        
    UniTask<GameResourceResult> LoadAsync<TResult>(string resource, ILifeTime lifeTime);
    
    UniTask<GameResourceResult[]> LoadAllAsync<TResult>(string resource, ILifeTime lifeTime);
}
```

IGameResourceRecord

Represents a single resource entry in the database:

```csharp
public interface IGameResourceRecord : ISearchFilterable
{
    string Name { get; }
    string Id { get; }
    bool CheckRecord(string filter);
}
```

## Game DB Category

A category that groups related resources together, allowing for organized access and 
management of game assets and implement custom logic support for category assets.

To create a new category, implement the `GameDataCategory` asset class

```csharp
public abstract class GameDataCategory : ScriptableObject, IGameDataCategory
````

## Unity Resource Category

allows you to load resource from Unity's built-in resources system and group assets by category

```csharp
ublic abstract class ResourcesAssetsCategory<TAsset> : GameDataCategory
        where TAsset : UnityEngine.Object
```

# Usage

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