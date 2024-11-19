using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LevelTypeScriptable", menuName = "BlockPuzzleGameToolkit/Levels/LevelTypeScriptable",
    order = 1)]
public class LevelTypeScriptable : ScriptableObject
{
    [FormerlySerializedAs("levelType")] public ELevelType elevelType;

    public TargetScriptable[] targets;

    public bool selectable = true;

    public bool singleColorMode = true;
}