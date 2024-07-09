using UnityEngine;

[CreateAssetMenu(fileName = "EnemyScriptableObject", menuName = "ScriptableObjects/Enemy")]
public class EnemyScriptableObject : ScriptableObject
{
    public Enemies[] enemies; 
}

[System.Serializable]
public class Enemies
{
    public string Name;
    public float maxHealth;
    public float defense;
    public float[] damage;
}