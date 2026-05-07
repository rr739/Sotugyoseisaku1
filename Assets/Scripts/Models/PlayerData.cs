using System;

[Serializable]
public class PlayerData
{
    public string id;          // 一意のプレイヤーID
    public string name;        // プレイヤーの名前
    public string room_id;     // 所属するルームID
    public float position_x;   // X座標
    public float position_y;   // Y座標
}

[Serializable]
public class InitResponse
{
    public string type;
    public string id;
    public int index;
}
