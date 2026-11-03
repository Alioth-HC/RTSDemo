
using UnityEngine;
using UnityEngine.EventSystems;

public class RTSCommandManager : MonoBehaviour
{
    [Header("玩家设置")]
    [SerializeField] private Faction playerFaction = Faction.Blue;

    [Header("点击设置")]
    [SerializeField] private Camera commandCamera;
    [SerializeField, Min(1f)] private float rayDistance = 500f;

    [Header("士兵设置")]
    [SerializeField] private GameObject unitPrefab;

    [Header("建筑信息面板")]
    [SerializeField] private BuildingSelectionUI selectionUI;

    private BuildingBase selectedBuilding;
    private int requestedCount = 5;
    private bool dispatchAll;

    public BuildingBase SelectedBuilding => selectedBuilding;

    private void Awake()
    {
        if (commandCamera == null)
            commandCamera = Camera.main;
    }

    private void Update()
    {
        HandleQuantityInput();

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ClearSourceSelection();
            return;
        }

        bool leftClick = Input.GetMouseButtonDown(0);
        bool rightClick = Input.GetMouseButtonDown(1);

        if (!leftClick && !rightClick)
            return;

        if (commandCamera == null)
        {
            Debug.LogError("没有摄像机，请指定 Command Camera。");
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = commandCamera.ScreenPointToRay(Input.mousePosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance))
        {
            // 点击空地不派兵，也不改变兵源。
            return;
        }

        BuildingBase clickedBuilding =
            hit.collider.GetComponentInParent<BuildingBase>();

        if (clickedBuilding == null)
            return;

        // 右键：打开己方建筑的升级操作。
        if (rightClick)
        {
            if (!clickedBuilding.CanAcceptCommand(playerFaction))
            {
                Debug.Log("只能升级己方建筑。");
                return;
            }

            if (selectionUI != null)
            {
                selectionUI.SelectBuilding(clickedBuilding);
                selectionUI.ShowUpgradeMode();
            }

            Debug.Log("打开升级操作：" + clickedBuilding.name);
            return;
        }

        // 左键第一次点击：选择兵源。
        if (selectedBuilding == null)
        {
            SelectSource(clickedBuilding);
            return;
        }

        // 再次点击当前兵源，不派兵。
        if (clickedBuilding == selectedBuilding)
            return;

        // 左键第二次点击：派兵到目标。
        DispatchToBuilding(clickedBuilding);
    }

    private void SelectSource(BuildingBase building)
    {
        if (building == null ||
            !building.CanAcceptCommand(playerFaction))
        {
            Debug.Log("请选择己方建筑作为兵源。");
            return;
        }

        UnitSpawner spawner = building.GetComponent<UnitSpawner>();

        if (spawner == null)
        {
            Debug.LogWarning(
                building.name + " 没有 UnitSpawner，不能作为兵源。");
            return;
        }

        selectedBuilding = building;

        if (selectionUI != null)
            selectionUI.SelectBuilding(building);

        Debug.Log(
            $"已选择兵源：{building.name}，" +
            $"驻军：{spawner.TroopCount}，" +
            $"可派遣：{spawner.AvailableCount}。");
    }

    private void DispatchToBuilding(BuildingBase target)
    {
        // 先保存本轮兵源，避免后续清空选择影响本轮派兵。
        BuildingBase sourceBuilding = selectedBuilding;

        if (sourceBuilding == null ||
            !sourceBuilding.CanAcceptCommand(playerFaction))
        {
            Debug.Log("兵源无效，请重新选择己方建筑。");
            ClearSourceSelection();
            return;
        }

        if (target == null ||
            !target.IsAlive ||
            !target.IsInitialized)
        {
            Debug.Log("目标建筑不可用。");
            return;
        }

        if (RTSMatchManager.Instance == null ||
            RTSMatchManager.Instance.Phase == MatchPhase.Ended)
        {
            Debug.Log("比赛管理器不存在或比赛已经结束，不能派兵。");
            return;
        }

        if (!target.CanBeCapturedNow(playerFaction))
        {
            Debug.Log("目标建筑当前不能被占领。");
            return;
        }

        if (target.IsInSafeZone &&
            RTSMatchManager.Instance.Phase != MatchPhase.Fever)
        {
            Debug.Log("Fever 开始前不能进入敌方基地安全区。");
            return;
        }

        if (unitPrefab == null)
        {
            Debug.LogError(
                "请在 RTSCommandManager 的 Unit Prefab 槽位中指定士兵预制体。");
            return;
        }

        UnitSpawner sourceSpawner =
            sourceBuilding.GetComponent<UnitSpawner>();

        if (sourceSpawner == null)
        {
            Debug.LogError("兵源建筑缺少 UnitSpawner。");
            ClearSourceSelection();
            return;
        }

        int count = dispatchAll
            ? sourceSpawner.AvailableCount
            : Mathf.Min(requestedCount, sourceSpawner.AvailableCount);

        if (!sourceSpawner.CanDispatch(count))
        {
            Debug.Log(
                $"可派兵力不足。兵源：{sourceBuilding.name}，" +
                $"可派数量：{sourceSpawner.AvailableCount}。");

            // 本次目标点击已经完成，要求下一次重新选择兵源。
            ClearSourceSelection();
            return;
        }

        if (!sourceSpawner.RemoveTroops(count))
        {
            Debug.LogWarning("扣除驻军失败，没有生成士兵。");
            ClearSourceSelection();
            return;
        }

        Debug.Log(
            $"派兵目标确认：{sourceBuilding.name} → {target.name}");

        int spawned = SpawnDispatchUnits(
            sourceBuilding, target, count);

        // 生成失败的士兵退还给本次兵源。
        if (spawned < count)
            sourceSpawner.AddTroops(count - spawned);

        if (spawned > 0)
        {
            Debug.Log(
                $"从 {sourceBuilding.name} 向 {target.name} " +
                $"派出 {spawned} 个士兵。");
        }

        // 本轮结束后清空兵源。
        // 下一轮必须重新点击选择新的兵源。
        ClearSourceSelection();
    }

    private void ClearSourceSelection()
    {
        selectedBuilding = null;

        if (selectionUI != null)
            selectionUI.ClearSelection();
    }

    private void HandleQuantityInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) ||
            Input.GetKeyDown(KeyCode.Keypad1))
        {
            requestedCount = 1;
            dispatchAll = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) ||
                 Input.GetKeyDown(KeyCode.Keypad2))
        {
            requestedCount = 5;
            dispatchAll = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) ||
                 Input.GetKeyDown(KeyCode.Keypad3))
        {
            requestedCount = 10;
            dispatchAll = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) ||
                 Input.GetKeyDown(KeyCode.Keypad4))
        {
            requestedCount = 20;
            dispatchAll = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0) ||
                 Input.GetKeyDown(KeyCode.Keypad0))
        {
            dispatchAll = true;
        }
    }

    private int SpawnDispatchUnits(
        BuildingBase source,
        BuildingBase target,
        int count)
    {
        Bounds bounds = GetBuildingBounds(source.gameObject);

        float spawnRadius =
            Mathf.Max(bounds.extents.x, bounds.extents.z) + 1.5f;

        int spawned = 0;

        for (int i = 0; i < count; i++)
        {
            float angle = i * 2.39996323f;
            float radius = spawnRadius + (i / 8) * 0.6f;

            Vector3 offset = new Vector3(
                Mathf.Cos(angle) * radius,
                0f,
                Mathf.Sin(angle) * radius);

            Vector3 position = source.transform.position + offset;
            position.y = source.transform.position.y;

            GameObject obj = Instantiate(
                unitPrefab,
                position,
                Quaternion.identity);

            Unit unit = obj.GetComponent<Unit>();

            if (unit == null)
            {
                Debug.LogError(
                    "士兵预制体必须挂载 Unit.cs 和 UnitMovement.cs。");

                Destroy(obj);
                continue;
            }

            unit.Initialize(playerFaction);
            unit.MoveTo(target.transform.position, target);
            spawned++;
        }

        return spawned;
    }

    private Bounds GetBuildingBounds(GameObject obj)
    {
        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>(true);

        if (colliders.Length > 0)
        {
            Bounds bounds = colliders[0].bounds;

            for (int i = 1; i < colliders.Length; i++)
                bounds.Encapsulate(colliders[i].bounds);

            return bounds;
        }

        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>(true);

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;

            for (int i = 1; i < renderers.Length; i++)
                bounds.Encapsulate(renderers[i].bounds);

            return bounds;
        }

        return new Bounds(
            obj.transform.position,
            new Vector3(4f, 2f, 4f));
    }
}
