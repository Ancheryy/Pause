using UnityEngine;
using System.Collections.Generic;

public class BubblePaperCutter : MonoBehaviour
{
    [Header("主Sprite渲染器")]
    public SpriteRenderer mainSpriteRenderer;
    [Header("所有可点击的图案Sprite渲染器")]
    public SpriteRenderer[] clickableSprites;

    [Header("主相机")]
    public Camera mainCamera;
    
    private Texture2D mainTexture; // 主Sprite的纹理
    private Sprite originalSprite; // 原始Sprite备份
    
    private bool isCuttingEnabled = true; // 是否允许挖洞

    private void Awake()
    {
        // 备份原始Sprite
        originalSprite = mainSpriteRenderer.sprite;
        
        // 获取主Image的纹理（注意：必须是可读写的Texture2D）
        mainTexture = mainSpriteRenderer.sprite.texture;
    }

    void Start()
    {
        InitTexture();
    }

    // 处理点击检测
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isCuttingEnabled)
        {
            HandleClick(Input.mousePosition);
        }

        // if (Input.GetMouseButtonDown(0) && isCuttingEnabled)
        // {
        //     Vector3 mouseScreenPos = Input.mousePosition;
        //     // 2. 关键：设置z值为相机到2D平面的距离（正交相机下，通常是相机的z坐标绝对值，因为2D物体一般在z=0）
        //     // 例如：相机在z=-10，则鼠标到2D平面（z=0）的距离是10
        //     mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        //     // 3. 转换为世界坐标
        //     Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        //     // 4. 2D场景中通常忽略z值（或设为0）
        //     worldPos.z = 0;
        //     
        //     CutFixedRadiusCircle(worldPos);
        // }
    }

    
    // 初始化Texture，复制一份 - 保持你原来的稳定逻辑
    void InitTexture()
    {
        Texture2D originalTexture = originalSprite.texture;
        if (!originalTexture.isReadable) return;

        mainTexture = Instantiate(originalTexture);
        mainTexture.name = "ModifiedTexture";

        Debug.Log("Texture初始化完成，修改只在这里进行，退出不会影响原始资源");
    }

    private void HandleClick(Vector2 screenPosition)
    {
        // 将屏幕坐标转换为世界坐标
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(screenPosition);
        worldPosition.z = 0;

        // 检查是否点击在可点击的Sprite上
        RaycastHit2D hit = Physics2D.Raycast(worldPosition, Vector2.zero);
        
        if (hit.collider != null)
        {
            foreach (var spriteRenderer in clickableSprites)
            {
                if (hit.collider.gameObject == spriteRenderer.gameObject)
                {
                    // 点击在图案上
                    OnPatternClicked(spriteRenderer, hit.collider);
                    break;
                }
            }
        }
    }

    private void OnPatternClicked(SpriteRenderer clickedSprite, Collider2D collider)
    {
        Debug.Log($"点击了图案: {clickedSprite.gameObject.name}");
        
        // 隐藏被点击的图案
        clickedSprite.gameObject.SetActive(false);
        
        // 在主Sprite上挖洞
        if (collider is CircleCollider2D circleCollider)
        {
            CutPatternArea(circleCollider);
        }
        
        // 调用游戏逻辑
        if (Checkpoint1_5Gameplay.Instance != null)
        {
            Checkpoint1_5Gameplay.Instance.PressBubble();
        }
    }

    private void CutPatternArea(CircleCollider2D circleCollider)
    {
        // 获取圆心在世界空间中的位置
        Vector3 worldCenter = circleCollider.bounds.center;

        // 正确的本地坐标转换（考虑旋转和缩放）
        // Vector3 localCenter = mainSpriteRenderer.transform.InverseTransformPoint(worldCenter);
        Vector3 localCenter = worldCenter - mainSpriteRenderer.transform.position;

        // 获取Sprite信息
        Sprite sprite = mainSpriteRenderer.sprite;
        float pixelsPerUnit = sprite.pixelsPerUnit;
        Vector2 pivot = sprite.pivot;

        // 获得缩放比例
        Vector3 localScale = mainSpriteRenderer.transform.localScale;
        
        // 直接计算像素坐标（更简单准确的方法）+++++++++++++++++++++++++++++
        int centerX = (int)(localCenter.x * pixelsPerUnit / localScale.x + pivot.x);
        int centerY = (int)(localCenter.y * pixelsPerUnit / localScale.y + pivot.y);

        // 限制在纹理范围内
        centerX = Mathf.Clamp(centerX, 0, mainTexture.width - 1);
        centerY = Mathf.Clamp(centerY, 0, mainTexture.height - 1);

        Vector2 circlePixelPos = new Vector2(centerX, centerY);
        
        // 关键修复：考虑圆形的缩放比例
        Vector3 circleScale = circleCollider.transform.lossyScale; // 使用世界缩放
        float radiusWorld = circleCollider.radius;
        float radiusPixels = radiusWorld * pixelsPerUnit * Mathf.Max(circleScale.x, circleScale.y) / Mathf.Max(localScale.x, localScale.y);

        Debug.Log($"挖洞位置: 世界坐标={worldCenter}, 本地坐标={localCenter}");
        Debug.Log($"像素坐标=({centerX}, {centerY}), 缩放={localScale}");
        Debug.Log($"半径 - 世界: {radiusWorld}, 像素: {radiusPixels}");


        // 调用填充圆形为透明
        FillCircleWithTransparency(new Vector2(centerX, centerY), radiusPixels);
    }

    private void FillCircleWithTransparency(Vector2 centerPixel, float radiusPixels)
    {
        int width = mainTexture.width;
        int height = mainTexture.height;
        // 获取当前像素数据
        Color[] pixels = mainTexture.GetPixels();
        
        // 计算挖洞区域
        int minX = Mathf.Clamp((int)(centerPixel.x - radiusPixels), 0, width - 1);
        int maxX = Mathf.Clamp((int)(centerPixel.x + radiusPixels), 0, width - 1);
        int minY = Mathf.Clamp((int)(centerPixel.y - radiusPixels), 0, height - 1);
        int maxY = Mathf.Clamp((int)(centerPixel.y + radiusPixels), 0, height - 1);

        Debug.Log($"挖洞区域: X[{minX}-{maxX}], Y[{minY}-{maxY}]");

        float radiusSqr = radiusPixels * radiusPixels;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float dx = x - centerPixel.x;
                float dy = y - centerPixel.y;
                float distanceSqr = dx * dx + dy * dy;
                if (distanceSqr <= radiusSqr)
                {
                    int index = y * width + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = new Color(1, 1, 1, 0); // 透明
                    }
                }
            }
        }

        // 应用修改到纹理
        mainTexture.SetPixels(pixels);
        mainTexture.Apply(true); // 强制应用
        
        // 刷新sprite显示
        mainSpriteRenderer.sprite = Sprite.Create(mainTexture, new Rect(0, 0, mainTexture.width, mainTexture.height), new Vector2(0.5f, 0.5f));

        Debug.Log($"挖洞完成: 中心({centerPixel}), 半径{radiusPixels}像素");
    }

    // 调试方法：在Scene视图中显示挖洞区域
    private void OnDrawGizmosSelected()
    {
        if (mainSpriteRenderer != null && mainSpriteRenderer.sprite != null)
        {
            // 显示Sprite边界
            Bounds bounds = mainSpriteRenderer.bounds;
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(bounds.center, bounds.size);
            
            // 显示所有可点击区域的Collider
            Gizmos.color = Color.red;
            foreach (var sprite in clickableSprites)
            {
                if (sprite != null && sprite.gameObject.activeInHierarchy)
                {
                    Collider2D collider = sprite.GetComponent<Collider2D>();
                    if (collider != null)
                    {
                        Gizmos.DrawWireCube(collider.bounds.center, collider.bounds.size);
                    }
                }
            }
        }
    }

    public void EnableCutting(bool isEnable)
    {
        isCuttingEnabled = isEnable;
    }
}