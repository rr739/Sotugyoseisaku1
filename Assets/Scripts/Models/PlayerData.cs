using System;
[Serializable]
public class InitResponse
{
    public string type;
    public string name_id;  
    public string room_id;     // Š‘®‚·‚éƒ‹[ƒ€ID
    public int  index;
    public bool IsStarted;
}

[Serializable]
public class PlayerData: InitResponse
{
    
    public float position_x;   // XÀ•W
    public float position_y;   // YÀ•W
  
}


