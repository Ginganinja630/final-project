using System;

[Serializable]
public class VideoData
{
    public string videoId;
    public string title;
    public string channelName;
    public string channelId;
    public long viewCount;
    public long likeCount;
    public long commentCount;
    public DateTime publishedDate;
}