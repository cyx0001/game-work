using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodItem : MonoBehaviour
{
    public int x;
    public int y;
    public FoodStaticInfo info;
    
    private Image img;
    private Button btn;
    private TextMeshProUGUI txt;
    private KitchenSceneManager manager;

    public void Init(int x, int y, FoodStaticInfo info, KitchenSceneManager manager)
    {
        this.x = x;
        this.y = y;
        this.info = info;
        this.manager = manager;

        img = GetComponent<Image>();
        btn = GetComponent<Button>();
        txt = GetComponentInChildren<TextMeshProUGUI>();

        img.color = Color.white; 

        // === 核心修改：向场景管理器索要对应序列号的图片 ===
        Sprite foodSprite = manager.GetSpriteByIndex(info.spriteIndex);
        
        if (foodSprite != null)
        {
            img.sprite = foodSprite; // 成功拿到图片并挂载
        }
        else
        {
            // 防呆兜底
            if (info.type == FoodType.Green) img.color = Color.green;
            else if (info.type == FoodType.Yellow) img.color = Color.yellow;
            else if (info.type == FoodType.Red) img.color = Color.red;
        }

        if (txt != null) txt.text = info.name;

        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(OnItemClick);
    }

    private void OnItemClick()
    {
        manager.SelectCard(this);
    }
}