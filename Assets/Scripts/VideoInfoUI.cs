using UnityEngine;
using TMPro;

public class VideoInfoUI : MonoBehaviour
{
    public TMP_Text infoText;   // TextMeshPro text element

    private void Awake()
    {
        // If you forgot to drag the text in the Inspector,
        // grab the TMP_Text on the same GameObject.
        if (infoText == null)
        {
            infoText = GetComponent<TMP_Text>();
            if (infoText == null)
            {
                Debug.LogWarning("VideoInfoUI: No TMP_Text found!");
            }
        }
    }

   public void ShowVideo(VideoData v)
    {
        if (v == null)
        {
            Debug.LogWarning("VideoInfoUI.ShowVideo called with null VideoData");
            return;
        }

        if (infoText == null)
        {
            Debug.LogWarning("VideoInfoUI: infoText is still null, cannot display video info.");
            return;
        }

        // Shorten long titles so they don't take over the screen
        string title = v.title;
        int maxTitleChars = 60;
        if (!string.IsNullOrEmpty(title) && title.Length > maxTitleChars)
        {
            title = title.Substring(0, maxTitleChars) + "...";
        }

        // Format big numbers with commas
        string viewsStr    = v.viewCount.ToString("N0");   // 4,253,474,448
        string likesStr    = v.likeCount.ToString("N0");
        string commentsStr = v.commentCount.ToString("N0");

        infoText.text =
            $"Title: {title}\n" +
            $"Channel: {v.channelName}\n" +
            $"Views: {viewsStr}\n" +
            $"Likes: {likesStr}\n" +
            $"Comments: {commentsStr}";
    }
}