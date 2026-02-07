# Traduio

Traduio is a flexible and robust localization library for Unity, designed to handle complex translation scenarios with ease. It supports multiple translation sources, asynchronous loading, dynamic parameter substitution, and automatic UI updates.

## Features

*   **Multiple Loaders**: Load translations from various sources (Resources, Dictionaries, etc.) with priority support.
*   **Asynchronous Loading**: Built-in support for async loading with timeouts and retries.
*   **Smart Translations**: Dynamic text replacement with parameter support (e.g., "Hello, {username}!").
*   **Automatic UI Updates**: `TranslationTarget` components automatically refresh when translations change.
*   **Fallback & Error Handling**: Configurable behavior for missing translations and loader failures.
*   **Extensible**: Easy to add custom loaders, processors, and translation targets.

## Installation

This package is available via UPM.

## Basic Usage

### Initialization

Initialize the `TranslationManager` and add loaders.

```csharp
using Damdor.Traduio;
using Cysharp.Threading.Tasks;

public class LocalizationService
{
    public TranslationManager Manager { get; private set; }

    public async UniTask Initialize()
    {
        Manager = new TranslationManager();

        // Add a loader for a JSON file in Resources
        Manager.AddLoader(new ResourceTranslationLoader("Localization/en", async: true));

        // Start loading
        Manager.Load();
        await Manager.WaitForLoad();
        
        // Set the global pointer so UI components can access it
        TraduioMainManager.Pointer.Setup(Manager);
        
        // Refresh currently created UI components
        Manager.RefreshTargets();
    }
}
```

### Retrieving Translations

```csharp
// Get a simple translation
string greeting = Manager.GetTranslation("ui.greeting");

// Check if a translation exists
bool hasKey = Manager.HasTranslation("ui.greeting");
```

### UI Components

Traduio provides base classes for creating UI components that automatically update when the language changes.

#### SmartTranslation (Text with Parameters)

`BaseSmartTranslation` allows you to bind a translation key to a UI element and inject dynamic parameters.

```csharp
// Example usage in a MonoBehaviour
public class MyUI : MonoBehaviour
{
    [SerializeField] private SmartTranslationTMP titleText; // Inherits from BaseSmartTranslation

    void Start()
    {
        // Set the key
        titleText.Key = SmartTranslationKey.Translation("ui.welcome_message");
        
        // Set a parameter (e.g., "Welcome, {user}!")
        titleText.SetParameter("user", SmartTranslationKey.Raw("Player1"));
    }
}
```

#### Creating Custom Targets

To support a specific UI component (like `TextMeshPro` or `UnityEngine.UI.Text`), inherit from `BaseSmartTranslation` or `TranslationTarget`.

```csharp
using TMPro;
using Damdor.Traduio;

public class SmartTranslationTMP : BaseSmartTranslation
{
    private TMP_Text target;
    
    protected override void Apply(string text)
    {
        if(target == null) target = GetComponent<TMP_Text>();
        if (target != null) target.text = text;
    }
}
```

## Advanced Features

### Translation Processors

You can intercept and modify translations globally using a processor.

```csharp
Manager.SetTranslationProcessor((key, translation, loader) => 
{
    // Example: Convert all text to uppercase
    return translation.ToUpper();
});
```

### Error Handling

Configure how the system reacts to missing keys or loader failures.

```csharp
Manager.SetMissingTranslationErrorHandler(key => 
{
    Debug.LogWarning($"Missing translation: {key}");
});

Manager.SetLoaderErrorHandler((loader, exception) => 
{
    Debug.LogError($"Loader {loader.Id} failed: {exception}");
});
```

### Batch Updates

For performance, you can batch updates to a translation target.

```csharp
target.StartChanges();
target.SetParameter("param1", value1);
target.SetParameter("param2", value2);
target.CommitChanges(); // Refreshes UI once
```

## Supported Loaders

*   **ResourceTranslationLoader**: Loads JSON translation files from Unity `Resources`.
*   **DictionaryTranslationLoader**: Loads translations from an in-memory `Dictionary<string, string>`.
