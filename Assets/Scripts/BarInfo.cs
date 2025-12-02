using UnityEngine;

[System.Serializable]
public class ChannelData
{
    public string channelName;
    public int videoCount;
    public double totalViews;
    public double totalLikes;
    public double totalComments;
}

public class BarInfo : MonoBehaviour
{
    public ChannelData data;
}