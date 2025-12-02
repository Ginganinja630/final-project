using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;     // new Input System

public class BarChart : MonoBehaviour
{
    [Header("Data & Prefab")]
    public DataManager dataManager;
    public GameObject barPrefab;

    [Header("Layout")]
    public int maxChannels = 10;    
    public float barWidth   = 1.0f;
    public float barSpacing = 0.5f;
    public float maxBarHeight = 20f;

    [Header("Interaction")]
    public Camera cameraToUse;        // drag Main Camera here
    public VideoInfoUI videoInfoUI;   // drag Label (VideoInfoUI) here

    private readonly List<GameObject> spawnedBars = new List<GameObject>();

    private void Start()
    {
        BuildChart();
    }

    public void BuildChart()
    {
        // Clear any old bars
        foreach (var b in spawnedBars)
            Destroy(b);
        spawnedBars.Clear();

        if (dataManager == null || dataManager.videos == null || dataManager.videos.Count == 0)
        {
            Debug.LogWarning("BarChart: No data to build chart.");
            return;
        }

        // 1) Group videos by channel and aggregate stats
        var channelStats = dataManager.videos
        .GroupBy(v => v.channelName)
        .Select(g => new
        {
            channelName   = g.Key,
            videoCount    = g.Count(),
            totalViews    = g.Sum(v => (double)v.viewCount),
            totalLikes    = g.Sum(v => (double)v.likeCount),
            totalComments = g.Sum(v => (double)v.commentCount)
        })
        .OrderByDescending(c => c.totalViews)    // biggest channels first
        .Take(maxChannels)                       
        .ToList();

        if (channelStats.Count == 0)
            return;

        double maxTotalViews = channelStats.Max(c => c.totalViews);
        if (maxTotalViews <= 0) maxTotalViews = 1.0;

        // 2) Compute horizontal offset so bars are centered around x = 0
        int n = channelStats.Count;
        float step = barWidth + barSpacing;
        float totalWidth = n * step;
        float startX = -totalWidth / 2f + step / 2f;

        // 3) Create each bar
        for (int i = 0; i < n; i++)
        {
            var c = channelStats[i];

            // Normalize height by max views
            float height01 = (float)(c.totalViews / maxTotalViews);
            height01 = Mathf.Sqrt(height01); 
            float height = height01 * maxBarHeight;

            float x = startX + i * step;
            float y = height / 2f; // so bar sits on ground

            Vector3 pos = new Vector3(x, y, 0f);
            GameObject bar = Instantiate(barPrefab, pos, Quaternion.identity, this.transform);
            bar.name = c.channelName;

            // Ensure it has a collider so it can be clicked
            if (bar.GetComponent<Collider>() == null)
            {
                var box = bar.AddComponent<BoxCollider>();
                box.size = new Vector3(1f, 1f, 1f);
            }

            // Scale bar based on height
            bar.transform.localScale = new Vector3(barWidth, height, barWidth);

            // Attach BarInfo with aggregated channel data
            ChannelData cd = new ChannelData
            {
                channelName   = c.channelName,
                videoCount    = c.videoCount,
                totalViews    = c.totalViews,
                totalLikes    = c.totalLikes,
                totalComments = c.totalComments
            };

            BarInfo info = bar.AddComponent<BarInfo>();
            info.data = cd;

            spawnedBars.Add(bar);
        }
    }

    private void Update()
    {
        // only react if this view is active
        if (!gameObject.activeInHierarchy)
            return;

        if (cameraToUse == null || videoInfoUI == null)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cameraToUse.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                BarInfo info = hit.collider.GetComponent<BarInfo>();
                if (info != null && info.data != null)
                {
                    ShowChannelInfo(info.data);
                }
            }
        }
    }

    private void ShowChannelInfo(ChannelData d)
    {
        if (videoInfoUI == null || videoInfoUI.infoText == null || d == null)
            return;

        string viewsStr    = d.totalViews.ToString("N0");
        string likesStr    = d.totalLikes.ToString("N0");
        string commentsStr = d.totalComments.ToString("N0");

        videoInfoUI.infoText.text =
            $"Channel: {d.channelName}\n" +
            $"Videos: {d.videoCount}\n" +
            $"Total Views: {viewsStr}\n" +
            $"Total Likes: {likesStr}\n" +
            $"Total Comments: {commentsStr}";
    }
}
