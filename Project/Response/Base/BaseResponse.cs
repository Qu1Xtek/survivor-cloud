using CloudLogic.Response;
using System;
using System.Collections.Generic;


public class BaseResponse
{
    public bool Success;

    public Dictionary<string, long> AchievementProgressInfo = new Dictionary<string, long>();

    public Dictionary<string, long> QuestsProgressInfo = new Dictionary<string, long>();

    public PlayerDataDTO? PlayerData;

    public string AdditionalInfo;

    public static BaseResponse Fail()
    {
        return new BaseResponse { Success = false };
    }
}
