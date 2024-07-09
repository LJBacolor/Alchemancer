using UnityEngine;

[CreateAssetMenu(fileName = "BuffsScriptableObject", menuName = "ScriptableObjects/Buffs")]
public class BuffsSriptableObject : ScriptableObject
{
    public CommonBuffs[] commonBuffs;
    public RareBuffs[] rareBuffs; 
    public LegendaryBuffs[] legendaryBuffs; 
    public CursedBuffs[] cursedBuffs; 
}

[System.Serializable]
public class CommonBuffs
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class RareBuffs
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class LegendaryBuffs
{
    public string Name;
    public string Description;
}

[System.Serializable]
public class CursedBuffs
{
    public string Name;
    public string Description;
}