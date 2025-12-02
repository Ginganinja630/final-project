using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;   // new Input System

public class Scatterplot : MonoBehaviour
{
    [Header("Data & Prefab")]
    public DataManager dataManager;
    public GameObject pointPrefab;
    public int maxPoints = 200;

    [Header("Layout")]
    public float width = 20f;
    public float height = 10f;
    public float depth = 10f;

    [Header("Color Mapping (Engagement)")]
    public Color lowEngagementColor = Color.blue;
    public Color highEngagementColor = Color.red;

    [Header("Interaction")]
    public Camera cameraToUse;        // drag Main Camera here
    public VideoInfoUI videoInfoUI;   // drag Label (with VideoInfoUI) here

    private readonly List<GameObject> spawnedPoints = new List<GameObject>();

    private float minEngagement = 0f;
    private float maxEngagement = 1f;

    private void Start()
    {
        BuildPlot();
    }

    public void BuildPlot()
    {
        // Clear old points
        foreach (var p in spawnedPoints)
            Destroy(p);
        spawnedPoints.Clear();

        if (dataManager == null || dataManager.videos == null || dataManager.videos.Count == 0)
        {
            Debug.LogWarning("Scatterplot: No data to build plot.");
            return;
        }

        // Take top N videos by views, then sort by published date
        var vids = dataManager.videos
            .OrderByDescending(v => v.viewCount)
            .Take(maxPoints)
            .OrderBy(v => v.publishedDate)
            .ToList();

        if (vids.Count == 0)
            return;

        // ----- Compute like/comment ratio ranges + engagement range -----
        double minLikeRatio = double.MaxValue;
        double maxLikeRatio = double.MinValue;
        double minCommentRatio = double.MaxValue;
        double maxCommentRatio = double.MinValue;

        double minEng = double.MaxValue;
        double maxEng = double.MinValue;

        foreach (var v in vids)
        {
            double likeRatio    = v.viewCount > 0 ? (double)v.likeCount    / v.viewCount : 0.0;
            double commentRatio = v.viewCount > 0 ? (double)v.commentCount / v.viewCount : 0.0;

            // for Y/Z axes
            if (likeRatio < minLikeRatio)       minLikeRatio = likeRatio;
            if (likeRatio > maxLikeRatio)       maxLikeRatio = likeRatio;
            if (commentRatio < minCommentRatio) minCommentRatio = commentRatio;
            if (commentRatio > maxCommentRatio) maxCommentRatio = commentRatio;

            // for color (engagement = likes per view)
            if (likeRatio < minEng) minEng = likeRatio;
            if (likeRatio > maxEng) maxEng = likeRatio;
        }

        if (double.IsInfinity(minLikeRatio)    || double.IsNaN(minLikeRatio))    minLikeRatio = 0.0;
        if (double.IsInfinity(maxLikeRatio)    || double.IsNaN(maxLikeRatio))    maxLikeRatio = 1.0;
        if (double.IsInfinity(minCommentRatio) || double.IsNaN(minCommentRatio)) minCommentRatio = 0.0;
        if (double.IsInfinity(maxCommentRatio) || double.IsNaN(maxCommentRatio)) maxCommentRatio = 1.0;

        float likeRange    = Mathf.Max(0.0001f, (float)(maxLikeRatio    - minLikeRatio));
        float commentRange = Mathf.Max(0.0001f, (float)(maxCommentRatio - minCommentRatio));

        // store engagement range for color mapping
        if (double.IsInfinity(minEng) || double.IsNaN(minEng)) minEng = 0.0;
        if (double.IsInfinity(maxEng) || double.IsNaN(maxEng)) maxEng = 1.0;

        minEngagement = (float)minEng;
        maxEngagement = (float)maxEng;
        float engRange = Mathf.Max(0.0001f, maxEngagement - minEngagement);

        // ----- Create points -----
        int n = vids.Count;

        for (int i = 0; i < n; i++)
        {
            var v = vids[i];

            // 0..1 along time *by rank* (oldest -> newest)
            float t = (n > 1) ? (float)i / (n - 1) : 0.5f;

            // engagement ratios
            double likeRatio    = v.viewCount > 0 ? (double)v.likeCount    / v.viewCount : 0.0;
            double commentRatio = v.viewCount > 0 ? (double)v.commentCount / v.viewCount : 0.0;

            // normalize to 0..1 for Y/Z
            float like01 = (float)((likeRatio    - minLikeRatio)    / likeRange);
            float comm01 = (float)((commentRatio - minCommentRatio) / commentRange);

            // map to 3D space, centered around 0
            float x = (t      - 0.5f) * width;
            float y = (like01 - 0.5f) * height;
            float z = (comm01 - 0.5f) * depth;

            Vector3 pos = new Vector3(x, y, z);

            GameObject point = Instantiate(pointPrefab, pos, Quaternion.identity, this.transform);
            point.name = v.title;

            // attach VideoPoint so we can show details & store engagement
            var vp = point.AddComponent<VideoPoint>();
            vp.data = v;
            vp.engagementScore = (float)likeRatio;   // likes per view

            // COLOR MAPPING based on engagement
            float eng01 = (vp.engagementScore - minEngagement) / engRange;
            eng01 = Mathf.Clamp01(eng01);

            var renderer = point.GetComponent<Renderer>();
            if (renderer != null)
            {
                // use .material so each point has its own instance
                renderer.material.color = Color.Lerp(lowEngagementColor, highEngagementColor, eng01);
            }

            spawnedPoints.Add(point);
        }
    }

    private void Update()
    {
        // only interact if this visualization is active
        if (!gameObject.activeInHierarchy)
            return;

        if (cameraToUse == null || videoInfoUI == null)
            return;

        if (Mouse.current == null)
            return;

        // CLICK: show details for the clicked point
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cameraToUse.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var vp = hit.collider.GetComponent<VideoPoint>();
                if (vp != null && vp.data != null)
                {
                    videoInfoUI.ShowVideo(vp.data);
                }
                else if (videoInfoUI.infoText != null)
                {
                    // fallback: just show hit name
                    videoInfoUI.infoText.text = "Hit: " + hit.collider.name;
                }
            }
        }
    }
}
