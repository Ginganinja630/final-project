using UnityEngine;
using UnityEngine.InputSystem;   // new Input System

public class VisualizationManager : MonoBehaviour
{
    [Header("Visualization Root Objects")]
    public GameObject barChartObject;
    public GameObject scatterplotObject;
    public GameObject starPlotObject;

    [Header("UI (optional)")]
    public GameObject infoPanel;   // the panel/label parent (can be null)

    private int currentView = 0;   // 0 = bar, 1 = scatter, 2 = star

    private void Start()
    {
        ShowView(0);  // start with bar chart
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            ShowView(0);   // bar chart

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            ShowView(1);   // scatterplot

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            ShowView(2);   // star plot
    }

    private void ShowView(int index)
    {
        currentView = index;

        if (barChartObject != null)
            barChartObject.SetActive(index == 0);

        if (scatterplotObject != null)
            scatterplotObject.SetActive(index == 1);

        if (starPlotObject != null)
            starPlotObject.SetActive(index == 2);

        
         if (infoPanel != null)
            infoPanel.SetActive(true);  
    }
}