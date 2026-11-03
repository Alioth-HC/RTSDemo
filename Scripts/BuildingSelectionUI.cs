
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingSelectionUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject panel;

    [Header("建筑信息")]
    [SerializeField] private TMP_Text buildingNameText;
    [SerializeField] private TMP_Text ownerText;
    [SerializeField] private TMP_Text troopText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text resourceText;
    [SerializeField] private TMP_Text upgradeCostText;

    [Header("升级按钮")]
    [SerializeField] private Button upgradeButton;

    private BuildingBase selectedBuilding;
    private Component upgradeComponent;
    private CanvasGroup panelCanvasGroup;
    private bool upgradeMode;

    private void Awake()
    {
        if (panel == null)
            panel = gameObject;

        panelCanvasGroup = panel.GetComponent<CanvasGroup>();

        if (panelCanvasGroup == null && panel == gameObject)
            panelCanvasGroup = panel.AddComponent<CanvasGroup>();

        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveListener(UpgradeSelectedBuilding);
            upgradeButton.onClick.AddListener(UpgradeSelectedBuilding);
            upgradeButton.gameObject.SetActive(false);
        }

        ClearSelection();
    }

    private void Update()
    {
        if (selectedBuilding == null)
            return;

        if (!selectedBuilding.IsAlive)
        {
            ClearSelection();
            return;
        }

        RefreshUI();
    }

    public void SelectBuilding(BuildingBase building)
    {
        if (building == null)
        {
            ClearSelection();
            return;
        }

        selectedBuilding = building;
        upgradeComponent = FindUpgradeComponent(building);

        // 普通选择建筑时，绝不显示升级按钮。
        upgradeMode = false;

        ShowPanel();
        RefreshUI();
    }

    // 由 RTSCommandManager 在右键点击己方建筑时调用。
    public void ShowUpgradeMode()
    {
        if (selectedBuilding == null)
            return;

        upgradeMode = true;

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(true);

        RefreshUI();
    }

    public void ClosePanel()
    {
        ClearSelection();
    }

    public void ClearSelection()
    {
        selectedBuilding = null;
        upgradeComponent = null;
        upgradeMode = false;

        if (upgradeButton != null)
            upgradeButton.gameObject.SetActive(false);

        if (panel == null)
            return;

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 0f;
            panelCanvasGroup.interactable = false;
            panelCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            panel.SetActive(false);
        }
    }

    private void ShowPanel()
    {
        if (panel == null)
            return;

        panel.SetActive(true);

        if (panelCanvasGroup != null)
        {
            panelCanvasGroup.alpha = 1f;
            panelCanvasGroup.interactable = true;
            panelCanvasGroup.blocksRaycasts = true;
        }

        SetTextActive(buildingNameText);
        SetTextActive(ownerText);
        SetTextActive(troopText);
        SetTextActive(levelText);
        SetTextActive(resourceText);
        SetTextActive(upgradeCostText);
    }

    private void SetTextActive(TMP_Text textComponent)
    {
        if (textComponent != null)
            textComponent.gameObject.SetActive(true);
    }

    private void RefreshUI()
    {
        if (selectedBuilding == null)
            return;

        if (buildingNameText != null)
            buildingNameText.text = selectedBuilding.name;

        if (ownerText != null)
            ownerText.text = "所属阵营：" + selectedBuilding.Owner;

        UnitSpawner spawner =
            selectedBuilding.GetComponent<UnitSpawner>();

        if (troopText != null)
        {
            int count = spawner != null ? spawner.TroopCount : 0;
            troopText.text = "驻军数量：" + count;
        }

        int level = GetLevel();

        if (levelText != null)
            levelText.text = "建筑等级：Lv." + level;

        if (upgradeCostText != null)
        {
            upgradeCostText.text = level >= 4
                ? "已达到最高等级"
                : "升级费用：" + GetUpgradeCost(level);
        }

        if (resourceText != null)
            resourceText.text = "阵营：" + selectedBuilding.Owner;

        if (upgradeButton != null)
        {
            // 只有进入升级模式后才显示按钮。
            upgradeButton.gameObject.SetActive(upgradeMode);

            upgradeButton.interactable =
                upgradeMode
                && selectedBuilding.Owner != Faction.Neutral
                && upgradeComponent != null
                && level < 4;
        }
    }

    private Component FindUpgradeComponent(BuildingBase building)
    {
        foreach (Component component in building.GetComponents<Component>())
        {
            if (component != null &&
                component.GetType().Name == "BuildingUpgrade")
                return component;
        }

        return null;
    }

    private int GetLevel()
    {
        if (upgradeComponent == null)
            return 1;

        Type type = upgradeComponent.GetType();

        string[] names =
        {
            "Level", "level", "CurrentLevel", "currentLevel"
        };

        foreach (string memberName in names)
        {
            PropertyInfo property = type.GetProperty(
                memberName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance);

            if (property != null && property.PropertyType == typeof(int))
                return (int)property.GetValue(upgradeComponent);

            FieldInfo field = type.GetField(
                memberName,
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(int))
                return (int)field.GetValue(upgradeComponent);
        }

        return 1;
    }

    private int GetUpgradeCost(int level)
    {
        switch (level)
        {
            case 1: return 50;
            case 2: return 100;
            case 3: return 175;
            default: return 0;
        }
    }


    public void UpgradeSelectedBuilding()
    {
        if (!upgradeMode ||
            selectedBuilding == null ||
            upgradeComponent == null)
            return;

        if (selectedBuilding.Owner == Faction.Neutral)
        {
            Debug.Log("中立建筑不能升级，请先占领。");
            return;
        }

        MethodInfo method = upgradeComponent.GetType().GetMethod(
            "Upgrade",
            BindingFlags.Public | BindingFlags.Instance);

        if (method == null)
        {
            Debug.LogError("升级失败：BuildingUpgrade 没有公开的 Upgrade() 方法。");
            return;
        }

        try
        {
            object result = method.Invoke(upgradeComponent, null);

            // 如果升级方法返回 bool，false 表示升级失败。
            if (method.ReturnType == typeof(bool) &&
                result is bool success && !success)
            {
                Debug.Log("升级失败，请检查资源是否足够。");
                return;
            }

            // 升级成功：退出升级模式，但保留建筑信息面板。
            upgradeMode = false;

            if (upgradeButton != null)
                upgradeButton.gameObject.SetActive(false);

            RefreshUI();

            Debug.Log("升级操作完成，已退出升级模式。");
        }
        catch (Exception exception)
        {
            Debug.LogError(
                "建筑升级出错：" +
                exception.GetBaseException().Message);
        }
    }

}
