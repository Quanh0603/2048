using UnityEngine;

[CreateAssetMenu(fileName = "TileSetting", menuName = "2048/Tile Setting", order = 0)]

public class TileSetting : ScriptableObject
{
    public float animationTime = 0.3f;

    public AnimationCurve animationCurve;
}