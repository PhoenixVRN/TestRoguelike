# Система размещения UI на ромбовидной сетке

## Описание
Система для размещения UI префабов на изометрической (ромбовидной) сетке по клику мыши.

## Компоненты

### 1. GridPlacer (Базовая версия)
Простая система размещения префабов по клику ЛКМ.

**Возможности:**
- Размещение префаба по клику левой кнопкой мыши
- Автоматическая привязка к центру ромба
- Настраиваемые размеры ромбов
- Смещение сетки
- Отладочная визуализация

### 2. AdvancedGridPlacer (Расширенная версия)
Продвинутая версия с дополнительными функциями.

**Возможности:**
- Все функции базовой версии
- Проверка занятости ячеек (нельзя размещать на занятые)
- Удаление объектов по ПКМ
- Словарь занятых ячеек
- API для проверки и получения объектов
- Метод очистки всех объектов

## Настройка в Unity

### Шаг 1: Подготовка Canvas
1. Создайте Canvas (UI → Canvas)
2. Установите Canvas Scaler → UI Scale Mode → Scale With Screen Size
3. Установите Reference Resolution (например, 1920x1080)

### Шаг 2: Создание Grid Image
1. Создайте Image внутри Canvas (ПКМ на Canvas → UI → Image)
2. Назовите его "GridImage"
3. Присвойте спрайт `grid.png` в компонент Image
4. Настройте размер RectTransform под вашу сетку
5. Добавьте компонент `GridPlacer` или `AdvancedGridPlacer`

### Шаг 3: Создание префаба для размещения
1. Создайте UI Image или другой UI элемент
2. Настройте его внешний вид
3. Перетащите в папку Prefabs
4. Удалите из сцены

### Шаг 4: Настройка GridPlacer
В компоненте GridPlacer настройте:

**Настройки сетки:**
- **Diamond Width** - ширина ромба в пикселях (измерьте на спрайте)
- **Diamond Height** - высота ромба в пикселях (измерьте на спрайте)

**Префаб:**
- **Prefab To Place** - перетащите созданный префаб

**Контейнер:**
- **Container** - RectTransform куда будут помещаться объекты (можно оставить пустым = текущий объект)

**Настройки:**
- **Offset X/Y** - смещение сетки если нужно выровнять
- **Show Debug** - включить отладочные логи

### Шаг 5 (Для AdvancedGridPlacer): Дополнительные настройки
- **Check Occupancy** - проверять занятость ячеек
- **Right Click To Remove** - ПКМ удаляет объекты
- **Show Hover Highlight** - подсветка при наведении

## Как определить размеры ромба

1. Откройте `grid.png` в редакторе изображений
2. Измерьте ширину ромба от левого угла до правого
3. Измерьте высоту ромба от верхнего угла до нижнего
4. Введите эти значения в Diamond Width и Diamond Height

Пример для стандартной изометрической сетки:
- Diamond Width = 64 пикселя
- Diamond Height = 32 пикселя (половина ширины)

## Использование в коде

### Базовое использование
Просто добавьте компонент на Image со спрайтом сетки - всё работает автоматически.

### Программное размещение (AdvancedGridPlacer)

```csharp
// Получить компонент
AdvancedGridPlacer placer = GetComponent<AdvancedGridPlacer>();

// Проверить занятость ячейки
Vector2Int coords = new Vector2Int(5, 3);
if (placer.IsCellOccupied(coords))
{
    Debug.Log("Ячейка занята!");
}

// Получить объект в ячейке
GameObject obj = placer.GetObjectAtCell(coords);

// Очистить все объекты
placer.ClearAll();
```

### Получение информации о размещенном объекте

```csharp
// На размещенном объекте есть компонент GridItemInfo
GridItemInfo info = placedObject.GetComponent<GridItemInfo>();
Debug.Log($"Позиция в сетке: {info.gridPosition}");
```

## Решение проблем

### Объекты размещаются не в центре ромбов
- Проверьте правильность значений Diamond Width и Diamond Height
- Используйте параметры Offset X/Y для точной настройки
- Включите Show Debug для визуализации сетки

### Клики не работают
- Убедитесь что на Canvas есть компонент GraphicRaycaster
- Проверьте что Image имеет Image компонент (raycast target)
- Убедитесь что есть EventSystem в сцене

### Сетка смещена
- Настройте параметры Offset X и Offset Y
- Проверьте Anchor и Pivot у RectTransform сетки (рекомендуется Center-Center)

## Примеры использования

### Пример 1: Roguelike с расстановкой юнитов
```csharp
public class UnitPlacer : MonoBehaviour
{
    [SerializeField] private AdvancedGridPlacer gridPlacer;
    [SerializeField] private GameObject[] unitPrefabs;
    
    private int currentUnitIndex = 0;
    
    void Update()
    {
        // Переключение префаба на цифры 1-9
        if (Input.GetKeyDown(KeyCode.Alpha1))
            currentUnitIndex = 0;
        // и т.д.
    }
}
```

### Пример 2: Редактор уровней
```csharp
public class LevelEditor : MonoBehaviour
{
    [SerializeField] private AdvancedGridPlacer gridPlacer;
    
    public void SaveLevel()
    {
        // Сохранить все занятые ячейки
        // gridPlacer.occupiedCells содержит все данные
    }
    
    public void ClearLevel()
    {
        gridPlacer.ClearAll();
    }
}
```

## Расширение функционала

Вы можете легко расширить систему:
- Добавить разные типы объектов
- Реализовать drag&drop
- Добавить анимацию размещения/удаления
- Сохранение/загрузка расстановки
- Ограничения на количество объектов
- Стоимость размещения (ресурсы)

