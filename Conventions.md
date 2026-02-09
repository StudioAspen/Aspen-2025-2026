# Conventions & Styling in Charon's Corner 
### Made by Samson Wu

This guide ensures consistency and readability across all Unity C# scripts and files.
This is also a living document and changes may be made as time goes on.

------------------------------------------------------------------------

## File & Script Names 

### File Names

-   **PascalCase**
-   File name = Class name

``` csharp
// Good
PlayerController.cs   // contains class PlayerController
PlayerConfigSO.cs   // contains ScriptableObject derived class PlayerConfigSO

// Bad
playercontroller.cs
player_controller.cs
```
### ScriptableObjects

-   **ScriptableObject** class files must end with **SO**
-   All ScriptableObjects must have the **CreateAssetMenu** line above their class declaration
    - The **fileName** and **menuName** should be named appropriately as you see fit

``` csharp
[CreateAssetMenu(fileName = "WorldConfig", menuName = "CharonsCorner/WorldConfig")]
public class WorldConfigSO : ScriptableObject
{
    // scriptable object
}
```

### Folder Names

-   **PascalCase**
-   Please put assets or scripts inside folders that make sense. Feel free to ask if you're unsure.
    - Most of the time, your scripts will be inside the `charonsCorner/Assets/Scripts/Runtime` folder

### Asset Names

-   **PascalCase**
    - Includes prefabs and scriptable objects

------------------------------------------------------------------------

## Braces

-   **Clang-Format** braces
    -   Braces start on a new line
-   One line statements don't need braces

``` csharp
private void Move(Vector3 moveDirection)
{
    if(moveDirection == Vector3.zero)
        return; // One line statement

    for(int i = 0; i < 10; i++)
    {
        Debug.Log($"Iteration {i}");
        Debug.Log("Multiline example");
    }

    _controller.Move(moveDirection * Time.deltaTime); 
}
```

------------------------------------------------------------------------ 

## Namespaces

-   **PascalCase**

``` csharp
namespace CharonsCorner.Bingus
{
    public class PlayerController { }
}
```

------------------------------------------------------------------------

## Classes, Structs, Interfaces, Enums

-   **PascalCase**
-   Interfaces prefixed with `I`

``` csharp
public class PlayerController { }
public struct KeyValuePair { }
public interface IDamageable { }
public enum GameState { 
    MainMenu, 
    Playing, 
    Paused 
}
```

------------------------------------------------------------------------

## Generic Types

-   **PascalCase** prefixed with `T`

``` csharp
public class KeyValuePair<TKey, TValue>
{
    // generic class
}
```

------------------------------------------------------------------------

## Methods

-   **PascalCase**
-   Should always start with a verb

``` csharp
public void TakeDamage(int amount) { }
public bool IsGrounded() 
{ 
    return true; 
}
```

------------------------------------------------------------------------

## Properties

-   **PascalCase**
-   Use `{ get; private set; }` where possible

``` csharp
public int Health { get; private set; }
public Vector3 Velocity { get; private set; }
```

------------------------------------------------------------------------

## Fields

### Serialized Fields (Exposed in Inspector)

-   **private** + `[SerializeField]`
    -   **camelCase** with leading underscore `_`
-   **public** + `[field: SerializeField]`
    -   **PascalCase**

``` csharp
[SerializeField] private float _moveSpeed;
[SerializeField] private GameObject _bulletPrefab;
[SerializeField] private protected string _name;
[field: SerializeField] public float MaxSpeed { get; private set; } // Property exposed to the Unity inspector
```

### Public Fields (Rare, Prefer Properties)

-   **PascalCase**
-   You probably shouldn't create a public field that is writable from a different class

``` csharp
public float JumpHeight; // prefer property instead with { get; private set; }
```

### Private Fields (Not Serialized)

-   **camelCase** with leading underscore `_`

``` csharp
private Rigidbody _rigidbody;
private bool _isGrounded;
private protected _stateMachine;
```

### Abstract Fields

-   **PascalCase**

``` csharp
private protected abstract string SaveKey { get; } // Forces all derived classes to assign this field
```

### Constants & Readonly

-   **PascalCase** regardless of access modifer

``` csharp
private const float MaxHealth = 100f;
private static readonly Vector3 DefaultScale = Vector3.one;
public static readonly float Duration = 1f;
```

------------------------------------------------------------------------

## Parameters & Local Variables

-   **camelCase**
-   **PascalCase** for local constants (rare)

``` csharp
void DealDamage(int damageAmount)
{
    const float DamageModifier = 2f;
    int finalDamage = damageAmount * DamageModifier;
}
```

------------------------------------------------------------------------

## Event Listener Methods

-   **Context_EventName**
-   This naming requirement is not very strict, just make it make sense
-   Remember that for every event you subscribe to, you must unsubscribe to it at least once

``` csharp
private void Start
{
    InputManager.Instance.Jump += Input_Jump;
    _playerController.OnGrounded += PlayerController_OnGrounded;
    GameManager.Instance.OnStateChanged += GameManager_OnStateChanged;
    _restartButton.onClick.AddListener(RestartButton_OnClick); // Unity event

    InputManager.Instance.Move += HandleMoveInput; // This works as well even though it breaks the style. Make it clear.
}
```
------------------------------------------------------------------------

## Coroutine Methods

-   Same as regular methods, but add **Coroutine** at the end

``` csharp
private IEnumerator FadeCoroutine(float duration)
{
    // Fade
}
```

------------------------------------------------------------------------

## Vector Multiplication

-   All constants must be multiplied together first before multiplying the vector

``` csharp
float floatOne = 4.2f;
int intOne = 3;
Vector3 direction = new Vector3(1f, 2f, 3f);

Vector3 result = (floatOne * intOne) * direction; // Constants are multiplied together before the last multiplication with the vector
```

------------------------------------------------------------------------

## Unity-Specific Methods

-   Use Unity's exact names, case-sensitive:
    -   `Start()`, `Update()`, `FixedUpdate()`, `OnTriggerEnter()`, etc.
-   Keep lifecycle methods at the top of the script for visibility.

------------------------------------------------------------------------

## Script Order

Recommended order inside any class:

1.  **Variables**
2.  **Unity Methods**
3.  **Your Methods**

------------------------------------------------------------------------

## Comments

-   Use `///` XML comments for public APIs
-   Use `//` for inline explanations
-   Keep comments meaningful and up-to-date

``` csharp
/// <summary>
/// Handles player movement and input.
/// </summary>
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f; // Player's movement speed

    /// <summary>
    /// This function does the moving. Example comment.
    /// </summary>
    public void Move()
    {

    }
}
```

------------------------------------------------------------------------

## Git Branch Naming

-   Use PascalCase for your feature name
-   For personal branches, prefix your feature name with `FirstL/`
    -   `SamsonW/NewUI`
-   For team branches, prefix with your team `TeamA/`
    -   `TeamA/Cannon`