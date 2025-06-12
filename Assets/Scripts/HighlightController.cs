using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class HighlightController : MonoBehaviour
{
    [Header("高亮设置")]
    public Color highlightColor = new Color(1, 1, 0.5f, 1);
    public float highlightRadius = 1.0f;
    public float intensity = 2.0f;
    public float smoothness = 0.2f;

    [Header("变形设置")]
    [Range(0, 0.3f)] public float deformationDepth = 0.1f;
    [Range(0.1f, 5f)] public float deformationHardness = 2f;

    [Header("状态控制")]
    public Transform highlightCenter;

    private Material _material;

    // 初始状态存储
    private Color _initialColor;
    private float _initialRadius;
    private float _initialIntensity;
    private float _initialSmoothness;
    private float _initialDeformationDepth;
    private float _initialDeformationHardness;

    private bool _isHighlighting = false;

    void Start()
    {
        InitializeMaterial();
        SaveInitialStates();
        UpdateMaterialProperties();
    }

    void Update()
    {
        SetDeformation(deformationDepth, deformationHardness);
        if (_isHighlighting)
        {
            UpdateColorAnimation();
            UpdateMaterialProperties();
        }
    }

    #region Public Methods
    /// <summary>
    /// 启动交互效果（颜色+变形）
    /// </summary>
    public void StartHighlight()
    {
        _isHighlighting = true;
        ResetToInitialColors();
    }

    /// <summary>
    /// 停止所有效果并恢复初始状态
    /// </summary>
    public void StopHighlight()
    {
        _isHighlighting = false;
        ResetAllParameters();
        UpdateMaterialProperties();
    }

    /// <summary>
    /// 设置变形参数（独立于颜色控制）
    /// </summary>
    public void SetDeformation(float depth, float hardness)
    {
        deformationDepth = Mathf.Clamp(depth, 0, 0.3f);
        deformationHardness = Mathf.Clamp(hardness, 0.1f, 5f);
        UpdateMaterialProperties();
    }
    #endregion

    #region Private Methods
    private void InitializeMaterial()
    {
        _material = GetComponent<Renderer>().materials[0];
    }

    private void SaveInitialStates()
    {
        _initialColor = highlightColor;
        _initialRadius = highlightRadius;
        _initialIntensity = intensity;
        _initialSmoothness = smoothness;
        _initialDeformationDepth = deformationDepth;
        _initialDeformationHardness = deformationHardness;
    }

    private void UpdateColorAnimation()
    {
        highlightColor.a = Mathf.PingPong(Time.time, 1f);
    }

    private void ResetToInitialColors()
    {
        highlightColor = _initialColor;
        intensity = _initialIntensity;
    }

    private void ResetAllParameters()
    {
        highlightColor = _initialColor;
        highlightRadius = _initialRadius;
        intensity = _initialIntensity;
        smoothness = _initialSmoothness;
        deformationDepth = _initialDeformationDepth;
        deformationHardness = _initialDeformationHardness;
    }

    private void UpdateMaterialProperties()
    {
        // 颜色相关参数
        _material.SetVector("_HighlightCenter", highlightCenter.position);
        _material.SetColor("_HighlightColor", highlightColor);
        _material.SetFloat("_HighlightRadius", highlightRadius);
        _material.SetFloat("_HighlightIntensity", intensity);
        _material.SetFloat("_EdgeSmoothness", smoothness);

        // 变形相关参数
        _material.SetFloat("_DeformationDepth", deformationDepth);
        _material.SetFloat("_DeformationHardness", deformationHardness);
    }
    #endregion

    void OnDestroy()
    {
        if (_material != null)
            Destroy(_material);
    }
}