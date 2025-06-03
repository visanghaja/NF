using UnityEngine;

public static class SortingLayerManager
{
    // 정적 상수로 레이어 이름 정의
    public const string BACKGROUND_LAYER = "Background";
    public const string WALL_LAYER = "Wall";
    public const string ENEMY_LAYER = "Enemy";
    public const string PLAYER_LAYER = "Player";
    public const string UI_LAYER = "UI";

    // 각 레이어의 기본 Order in Layer 값
    public const int BACKGROUND_ORDER = 0;
    public const int WALL_ORDER = 100;
    public const int ENEMY_ORDER = 200;
    public const int PLAYER_ORDER = 300;
    public const int UI_ORDER = 1000;
} 