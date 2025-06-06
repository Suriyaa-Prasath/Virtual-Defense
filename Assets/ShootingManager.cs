using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShootingManager : MonoBehaviour
{
    [Header("Target & UI")]
    public GameObject board; // Target board (must have a Collider)
    public Renderer planeRenderer; // Plane's Renderer to display heatmap
    public Button resetButton; // Reset button
    public TextMeshPro scoreDisplayText; // Single 3D TextMeshPro for score display

    [Header("Heatmap Settings")]
    private Texture2D heatmapTexture;
    private int textureSize = 256;
    private float[,] hitIntensity;
    private float maxIntensity = 30f;

    [Header("Scoring & Spread")]
    private int totalScore = 0;
    private int shotCount = 0;
    private int maxScore = 10;
    private int baseSpreadRadius;
    private int maxSpreadRadius;
    private float spreadGrowthFactor = 1.5f;

    void Start()
    {
        if (board == null || board.GetComponent<Collider>() == null)
        {
            Debug.LogError("❌ ShootingManager: Board object is missing a Collider!");
            return;
        }

        baseSpreadRadius = textureSize / 16;
        maxSpreadRadius = textureSize / 4;

        heatmapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        heatmapTexture.filterMode = FilterMode.Bilinear;
        heatmapTexture.wrapMode = TextureWrapMode.Clamp;

        if (planeRenderer != null)
        {
            planeRenderer.material.mainTexture = heatmapTexture; // Apply to Plane's Material
        }
        else Debug.LogError("❌ ShootingManager: Plane Renderer not assigned!");

        if (scoreDisplayText == null)
            Debug.LogError("❌ ShootingManager: Score Display Text not assigned!");

        hitIntensity = new float[textureSize, textureSize];

        if (resetButton != null) resetButton.onClick.AddListener(ResetAll);
        else Debug.LogWarning("⚠️ ShootingManager: Reset button is not assigned.");

        ResetAll();
    }

    Vector2 WorldToUV(Vector3 worldPos)
    {
        Bounds bounds = board.GetComponent<Collider>().bounds;
        Vector3 localPos = board.transform.InverseTransformPoint(worldPos);

        float u = Mathf.InverseLerp(-bounds.extents.x, bounds.extents.x, localPos.x);
        float v = Mathf.InverseLerp(-bounds.extents.y, bounds.extents.y, localPos.y);

        return new Vector2(u, v);
    }

    public void RegisterHit(Vector3 worldPosition)
    {
        Vector2 uv = WorldToUV(worldPosition);

        int x = Mathf.RoundToInt(uv.x * (textureSize - 1));
        int y = Mathf.RoundToInt(uv.y * (textureSize - 1));

        x = Mathf.Clamp(x, 0, textureSize - 1);
        y = Mathf.Clamp(y, 0, textureSize - 1);

        ApplyGaussianImpact(x, y);
        UpdateHeatmap();

        float score = CalculateCircularScore(worldPosition); // Uses new circular scoring
        totalScore += Mathf.RoundToInt(score);

        Debug.Log($"🎯 Shot {shotCount}: Score = {score} | Total Score = {totalScore}");
        UpdateScoreUI(score);
    }

    void ApplyGaussianImpact(int centerX, int centerY)
    {
        int currentHits = Mathf.RoundToInt(hitIntensity[centerX, centerY]);
        int spreadRadius = Mathf.Clamp(baseSpreadRadius + Mathf.RoundToInt(currentHits * spreadGrowthFactor), baseSpreadRadius, maxSpreadRadius);

        for (int dx = -spreadRadius; dx <= spreadRadius; dx++)
        {
            for (int dy = -spreadRadius; dy <= spreadRadius; dy++)
            {
                int x = centerX + dx;
                int y = centerY + dy;

                if (x < 0 || x >= textureSize || y < 0 || y >= textureSize) continue;

                float distance = Mathf.Sqrt(dx * dx + dy * dy);
                if (distance <= spreadRadius)
                {
                    float intensityFactor = Mathf.Exp(-Mathf.Pow(distance / (spreadRadius * 0.7f), 2)) * (1.8f + (currentHits * 0.1f));
                    hitIntensity[x, y] = Mathf.Min(hitIntensity[x, y] + intensityFactor, maxIntensity);
                }
            }
        }
    }

    float CalculateCircularScore(Vector3 worldPos)
    {
        Bounds bounds = board.GetComponent<Collider>().bounds;
        Vector3 center = bounds.center;

        float maxDistance = Mathf.Min(bounds.extents.x, bounds.extents.y);
        float distance = Vector3.Distance(new Vector3(worldPos.x, worldPos.y, 0), new Vector3(center.x, center.y, 0));

        float normalizedDistance = Mathf.Clamp(distance / maxDistance, 0, 1);
        float score = Mathf.Lerp(maxScore, 1, normalizedDistance / 0.05f); // Decreases every 0.05 units
        return Mathf.Max(1, Mathf.Round(score * 10f) / 10f); // Minimum score is 1
    }

    void UpdateHeatmap()
    {
        Color[] colors = new Color[textureSize * textureSize];

        for (int x = 0; x < textureSize; x++)
        {
            for (int y = 0; y < textureSize; y++)
            {
                float intensity = hitIntensity[x, y] / maxIntensity;
                colors[y * textureSize + x] = GetHeatmapColor(intensity);
            }
        }

        heatmapTexture.SetPixels(colors);
        heatmapTexture.Apply(false);
    }

    Color GetHeatmapColor(float value)
    {
        if (value < 0.3f)
            return Color.Lerp(Color.green, Color.yellow, value * 3);
        else if (value < 0.6f)
            return Color.Lerp(Color.yellow, Color.red, (value - 0.3f) * 3);
        else
            return Color.Lerp(Color.red, new Color(0.4f, 0, 0), (value - 0.6f) * 3);
    }

    public void IncreaseShotCount()
    {
        shotCount++;
        UpdateScoreUI(0);
        Debug.Log($"🔫 Shot fired! Total Shots: {shotCount}");
    }

    void UpdateScoreUI(float lastScore)
    {
        //scoreDisplayText.text = $"Last: {lastScore}/10\nTotal: {totalScore} (Shots: {shotCount})";
        scoreDisplayText.text = $"Shots: {shotCount}";
    }

    public void ResetAll()
    {
        totalScore = 0;
        shotCount = 0;

        for (int x = 0; x < textureSize; x++)
            for (int y = 0; y < textureSize; y++)
                hitIntensity[x, y] = 0;

        UpdateHeatmap();
        UpdateScoreUI(0);
        Debug.Log("♻️ Heatmap & Score Reset!");
    }
}
