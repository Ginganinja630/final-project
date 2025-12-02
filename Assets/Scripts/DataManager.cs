using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public string csvFileName = "youtube_video"; 
    public List<VideoData> videos = new List<VideoData>();

    private void Awake()
    {
        LoadCsv();
    }

    private void LoadCsv()
    {
        TextAsset csvAsset = Resources.Load<TextAsset>(csvFileName);

        if (csvAsset == null)
        {
            Debug.LogError("Could not find CSV in Resources: " + csvFileName);
            return;
        }

        string[] lines = csvAsset.text.Split('\n');

        
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            List<string> cols = ParseCsvLine(line);
            if (cols.Count < 9) continue; 

            VideoData v = new VideoData();

            v.videoId      = cols[0];
            v.title        = cols[1];
            v.channelName  = cols[2];
            v.channelId    = cols[3];

            long.TryParse(cols[4], out v.viewCount);
            long.TryParse(cols[5], out v.likeCount);
            long.TryParse(cols[6], out v.commentCount);

            
            DateTime dt;
            if (DateTime.TryParse(cols[7], null, DateTimeStyles.AdjustToUniversal, out dt))
            {
                v.publishedDate = dt;
            }

            videos.Add(v);
        }

        Debug.Log("Loaded videos: " + videos.Count);
    }

   
    private List<string> ParseCsvLine(string line)
    {
        List<string> result = new List<string>();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Length = 0;
            }
            else if (c != '\r' && c != '\n')
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}