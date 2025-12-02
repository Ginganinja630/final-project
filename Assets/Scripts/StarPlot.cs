using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class StarPlot : MonoBehaviour
{
    [Header("Data & Prefab")]
    public DataManager dataManager;
    public Material starMaterial;

    [Header("Plot Settings")]
    public float radius = 2f;          // size of each star
    public int maxStars = 50;          // limit number of videos drawn
    public float spacing = 5f;         // space between stars

    [Header("Interaction")]
    public Camera cameraToUse;         
    public VideoInfoUI videoInfoUI;

    private List<GameObject> spawnedStars = new List<GameObject>();
    private StarInfo selectedStar = null;

    private void Start()
    {
        BuildPlot();
    }

    // -----------------------------------------------------
    // BUILD STAR PLOT
    // -----------------------------------------------------
    public void BuildPlot()
    {
        // Clear old stars
        foreach (var s in spawnedStars)
            Destroy(s);
        spawnedStars.Clear();

        if (dataManager == null || dataManager.videos.Count == 0)
        {
            Debug.LogWarning("StarPlot: No data.");
            return;
        }

        // Take top N videos by views for readability
        var vids = dataManager.videos
            .OrderByDescending(v => v.viewCount)
            .Take(maxStars)
            .ToList();

        float offset = -(spacing * vids.Count) / 2f;  // center all stars

       for (int i = 0; i < vids.Count; i++)
        {
            VideoData v = vids[i];

            // Create object
            GameObject starObj = new GameObject("Star_" + v.title);
            starObj.transform.parent = this.transform;

            // ---- GRID LAYOUT ----
            int cols = 10;  // adjust for how wide you want it
            int row = i / cols;
            int colIndex = i % cols;

            starObj.transform.localPosition = new Vector3(
                colIndex * spacing,
                0,
                -row * spacing
            );

            // Add LineRenderer
            LineRenderer lr = starObj.AddComponent<LineRenderer>();
            lr.positionCount = 5;
            lr.loop = true;
            lr.material = starMaterial;
            lr.widthMultiplier = 0.05f;
            lr.useWorldSpace = false;

            // normalize values
            float views01    = Normalize(v.viewCount, dataManager.videos, vd => vd.viewCount);
            float likes01    = Normalize(v.likeCount, dataManager.videos, vd => vd.likeCount);
            float comments01 = Normalize(v.commentCount, dataManager.videos, vd => vd.commentCount);

            float ratio = v.commentCount > 0 ? (float)v.likeCount / v.commentCount : 0f;
            float ratio01 = Normalize(ratio, dataManager.videos,
                vd => vd.commentCount > 0 ? (float)vd.likeCount / vd.commentCount : 0);

            // build star shape (4 axes)
            Vector3[] pts = new Vector3[5];
            pts[0] = new Vector3(radius * views01, 0, 0);
            pts[1] = new Vector3(0, radius * likes01, 0);
            pts[2] = new Vector3(-radius * comments01, 0, 0);
            pts[3] = new Vector3(0, -radius * ratio01, 0);
            pts[4] = pts[0];

            lr.SetPositions(pts);

            // collider + star info
            var col = starObj.AddComponent<SphereCollider>();
            col.radius = 0.6f;

            var info = starObj.AddComponent<StarInfo>();
            info.data = v;
            info.line = lr;

            spawnedStars.Add(starObj);
        }
    }

    // -----------------------------------------------------
    // NORMALIZE HELPER 0–1
    // -----------------------------------------------------
    private float Normalize(float value, List<VideoData> vids, System.Func<VideoData, float> selector)
    {
        float min = float.MaxValue;
        float max = float.MinValue;

        foreach (var v in vids)
        {
            float val = selector(v);
            if (val < min) min = val;
            if (val > max) max = val;
        }

        if (max - min < 0.0001f)
            return 0.5f;

        return (value - min) / (max - min);
    }

    // -----------------------------------------------------
    // CLICK INTERACTION
    // -----------------------------------------------------
    private void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (cameraToUse == null || Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            Ray ray = cameraToUse.ScreenPointToRay(pos);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                StarInfo info = hit.collider.GetComponent<StarInfo>();
                if (info != null)
                {
                    SelectStar(info);
                }
            }
        }
    }

    // -----------------------------------------------------
    // STAR SELECTION & HIGHLIGHTING
    // -----------------------------------------------------
    private void SelectStar(StarInfo star)
    {
        // unhighlight previous
        if (selectedStar != null)
        {
            selectedStar.line.startColor = Color.white;
            selectedStar.line.endColor = Color.white;
            selectedStar.line.widthMultiplier = 0.05f;
        }

        // highlight new one
        star.line.startColor = Color.yellow;
        star.line.endColor = Color.yellow;
        star.line.widthMultiplier = 0.15f;

        selectedStar = star;

        // show data in UI
        var d = star.data;

        if (videoInfoUI != null && videoInfoUI.infoText != null)
        {
            videoInfoUI.infoText.text =
                $"Title: {d.title}\n" +
                $"Channel: {d.channelName}\n" +
                $"Views: {d.viewCount:N0}\n" +
                $"Likes: {d.likeCount:N0}\n" +
                $"Comments: {d.commentCount:N0}\n" +
                $"Like/Comment Ratio: {(d.commentCount>0 ? (float)d.likeCount/d.commentCount : 0):0.00}";
        }
    }
}