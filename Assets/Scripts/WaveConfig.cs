using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Конфигурация волны врагов
/// Определяет каких врагов и где размещать
/// </summary>
[CreateAssetMenu(fileName = "NewWave", menuName = "Game/Wave Config", order = 2)]
public class WaveConfig : ScriptableObject
{
    [Header("Информация о волне")]
    [Tooltip("Название волны")]
    public string waveName = "Wave 1";
    
    [Tooltip("Номер волны")]
    public int waveNumber = 1;
    
    [Header("Враги в волне")]
    [Tooltip("Список врагов для спавна")]
    public List<EnemySpawnData> enemies = new List<EnemySpawnData>();
    
    [System.Serializable]
    public class EnemySpawnData
    {
        [Tooltip("Префаб врага")]
        public GameObject enemyPrefab;
        
        [Tooltip("Позиция в сетке (X, Y)")]
        public Vector2Int gridPosition;
        
        [Tooltip("Или использовать случайную позицию")]
        public bool randomPosition = false;
    }
    
    /// <summary>
    /// Получить количество врагов в волне
    /// </summary>
    public int GetEnemyCount()
    {
        return enemies.Count;
    }
}

