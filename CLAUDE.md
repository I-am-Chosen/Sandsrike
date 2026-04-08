# CLAUDE.md — Unity Game Development Assistant

Ты — AI-ассистент для разработки игр в Unity. У тебя есть прямой доступ к Unity Editor через MCP инструменты.

## Версия Unity
**Unity 6000.4.1f1**

---

## Доступные MCP инструменты

### AnkleBreaker Unity MCP (основной)
Используй для управления сценой, объектами и проектом:

| Инструмент | Назначение |
|---|---|
| `unity_list_advanced_tools` | показать все доступные инструменты |
| `unity_create_gameobject` | создать GameObject |
| `unity_modify_component` | изменить компонент |
| `unity_create_script` | создать C# скрипт |
| `unity_run_tests` | запустить тесты |
| `unity_build_project` | собрать проект |
| `unity_manage_scene` | управление сценой |
| `unity_shader_graph` | редактировать Shader Graph |
| `unity_navmesh_bake` | запечь NavMesh |
| `unity_profile_performance` | профилировать производительность |

### Unity-MCP (IvanMurzak) — дополнительно

| Инструмент | Назначение |
|---|---|
| `assets-get-data` | получить данные ассета |
| `assets-material-create` | создать материал |
| `editor-application-get-state` | состояние редактора |
| `editor-application-set-state` | управление Play Mode |
| `editor-selection-get` | текущий выбор в Editor |
| `batch_execute` | пакетное выполнение операций |

---

## Архитектура и паттерны

### Структура проекта

```
Assets/
├── _Project/
│   ├── Scripts/
│   │   ├── Core/           — базовые системы
│   │   ├── Gameplay/       — игровая логика
│   │   ├── UI/             — интерфейс
│   │   └── Utils/          — утилиты
│   ├── ScriptableObjects/  — данные и конфиги
│   ├── Prefabs/
│   ├── Scenes/
│   └── Art/
```

### Обязательные паттерны

**ScriptableObject Architecture** — для данных и событий:
```csharp
// Данные — через ScriptableObject, не через синглтоны
[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    public float moveSpeed = 5f;
    public int maxHealth = 100;
}
```

**Object Pooling** — для всего, что часто создаётся/уничтожается:
```csharp
// Всегда использовать пул вместо Instantiate/Destroy в рантайме
ObjectPool<GameObject> pool = new ObjectPool<GameObject>(
    createFunc: () => Instantiate(prefab),
    actionOnGet: obj => obj.SetActive(true),
    actionOnRelease: obj => obj.SetActive(false)
);
```

**Event System** — через ScriptableObject события, не прямые ссылки:
```csharp
[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEventSO : ScriptableObject
{
    private List<GameEventListener> listeners = new();
    public void Raise() => listeners.ForEach(l => l.OnEventRaised());
}
```

---

## Оптимизация — обязательно

```csharp
// Кешируй компоненты в Awake
private Rigidbody _rb;
private void Awake() => _rb = GetComponent<Rigidbody>();

// Избегай GetComponent в Update
// void Update() { GetComponent<Renderer>().material.color = ... }  <- НИКОГДА

// Используй Job System для тяжёлых вычислений
[BurstCompile]
public struct MoveJob : IJobParallelFor { ... }

// Кешируй строки для анимаций
private static readonly int SpeedHash = Animator.StringToHash("Speed");
animator.SetFloat(SpeedHash, speed);

// Используй StringBuilder для строк в Update
private StringBuilder _sb = new StringBuilder();
```

---

## Workflow с MCP

### Создание нового объекта
1. Спроси о контексте сцены: `editor-application-get-state`
2. Создай объект: `unity_create_gameobject`
3. Добавь компоненты: `unity_modify_component`
4. Создай скрипт если нужно: `unity_create_script`
5. Проверь результат: `editor-selection-get`

### Дебаггинг
1. Получи состояние редактора
2. Запусти тесты: `unity_run_tests`
3. Профилируй: `unity_profile_performance`
4. Исправь и повтори

### Пакетные операции
Для создания множества объектов — всегда используй `batch_execute`:
```
// Вместо 10 отдельных вызовов — один batch_execute
// Это в 10-100x быстрее
```

---

## C# в Unity — правила

```csharp
// Пространства имён — всегда
namespace MyGame.Gameplay { }

// Сериализация — явная
[SerializeField] private float _speed;
[field: SerializeField] public int Health { get; private set; }

// Null проверки через C# 8+
var component = GetComponent<Renderer>();
if (component is null) return;

// Корутины — только для логики с задержками, не для Update замены
private IEnumerator SpawnEnemies()
{
    while (true)
    {
        SpawnEnemy();
        yield return new WaitForSeconds(spawnInterval);
    }
}

// Async/Await для загрузки ресурсов
private async UniTask LoadSceneAsync(string sceneName)
{
    await SceneManager.LoadSceneAsync(sceneName);
}
```

---

## Никогда не делай

- `FindObjectOfType<>()` в Update или часто вызываемых методах
- `Camera.main` в Update (кешируй в Awake)
- Синглтоны на MonoBehaviour — используй ScriptableObject
- `Instantiate`/`Destroy` в рантайме — используй Object Pool
- Хардкод строк для тегов и слоёв — используй константы
- Логику в `OnGUI` — только для дебаггинга
