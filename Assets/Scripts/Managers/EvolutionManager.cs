using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance;

    [Header("Data")]
    [SerializeField] private List<EvolutionTreeSO> evolutionTrees;
    [SerializeField] private List<EvolutionTierInfo> evolutionTiersInfo;

    [Header("Event SOs")]
    [SerializeField] private VoidPublisherSO onEvolutionTierFinishSelection;

    private Dictionary<SpreadMethodType, EvolutionTreeSO> treeLookup;

    private EvolutionProgressData progress;

    private TimeManager timeManager;
    private UIManager uiManager;
    private SpreadMethodManager spreadMethodManager;
    private PointsManager pointsManager;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);

        progress = new EvolutionProgressData();
    }

    private void BuildLookup()
    {
        treeLookup = new Dictionary<SpreadMethodType, EvolutionTreeSO>();
        foreach (var tree in evolutionTrees)
        {
            treeLookup.Add(tree.spreadType, tree);
        }
    }

    #region Evolution Upgrade

    public void OnInfectedRateChanged(float infectedRate)
    {
        GameState gameState = GameManager.Instance.GetGameState();
        if (gameState != GameState.Playing && gameState != GameState.Paused)
            return;

        TryQueueNextTier(infectedRate, true);
    }

    private bool TryQueueNextTier(float infectedRate, bool showNotification)
    {
        int nextTier = progress.completedTierIndex + 1;

        if (progress.specializationChosen)
        {
            EvolutionTreeSO tree = treeLookup[progress.selectedSpreadType];
            // plus 1 because the first tier is select method tier
            if (nextTier >= tree.tiers.Count + 1)
                return false;
        }

        if (progress.pendingTierQueue.Contains(nextTier))
            return false;

        EvolutionTierInfo nextTierInfo = evolutionTiersInfo[nextTier];
        if (infectedRate >= evolutionTiersInfo[nextTier].infectedRate
            && !nextTierInfo.hasBeenChosen)
        {
            if (!nextTierInfo.hasBeenSetInfo)
            {
                int cost = nextTierInfo.cost;
                uiManager.SetEvolutionSelectionPanelInfo(nextTier, cost);
                nextTierInfo.hasBeenSetInfo = true;
                progress.pendingTierQueue.Enqueue(nextTier);

                if (!progress.specializationChosen)
                {
                    timeManager.SetIsPaused(true);
                    uiManager.ShowSpecializationSelection(evolutionTrees);
                    showNotification = false;
                }

                if (showNotification && !nextTierInfo.hasShownNoti)
                {
                    uiManager.ShowEvolutionUpgradeNotification($"New evolution tier available!", 3f, Color.green);
                    nextTierInfo.hasShownNoti = true;
                }

                return true;
            }
        }

        return false;
    }

    private void ShowCurrentTier()
    {
        // minus 1 because the first tier is select method tier
        int tierIndex = progress.pendingTierQueue.Peek() - 1;

        EvolutionTreeSO tree = treeLookup[progress.selectedSpreadType];

        EvolutionTierData tier = tree.tiers[tierIndex];

        uiManager.ShowEvolutionTier(tier);
    }

    public void OnEvolutionTierSelectionUIOpened()
    {
        if (uiManager.IsEvolutionPanelsOpened())
            return;

        if (!progress.specializationChosen)
        {
            uiManager.ShowSpecializationSelection(evolutionTrees);
            return;
        }

        if (progress.pendingTierQueue.Count == 0)
            return;

        ShowCurrentTier();
    }

    public void SelectSpreadMethod(SpreadMethodType type)
    {
        int tierIndex = progress.pendingTierQueue.Peek();

        progress.pendingTierQueue.Dequeue();
        progress.completedTierIndex = tierIndex;
        evolutionTiersInfo[tierIndex].hasBeenChosen = true;

        progress.specializationChosen = true;
        progress.selectedSpreadType = type;

        InitEvolutionTreePanel();

        if (!TryQueueNextTier(MapManager.Instance.GetInfectedRate(), false))
        {
            onEvolutionTierFinishSelection.RaiseEvent();
        }
    }

    public void SelectUpgrade(EvolutionUpgradeNodeSO node)
    {
        int tierIndex = progress.pendingTierQueue.Peek();

        EvolutionTierInfo evolutionTierInfo = evolutionTiersInfo[tierIndex];

        if (!pointsManager.HaveEnoughEvolutionPoints(evolutionTierInfo.cost))
        {
            uiManager.ShowNotification("Not enough Evolution Points!", 3f, Color.red);
            return;
        }

        pointsManager.SpendEvolutionPoints(evolutionTierInfo.cost);

        SpreadMethodRuntimeData runtime = spreadMethodManager.GetRuntimeData(progress.selectedSpreadType);

        SpreadMethodContext context = spreadMethodManager.GetSpreadMethodContext(progress.selectedSpreadType);
        foreach (var effect in node.effects)
        {
            effect.Apply(runtime, context);
        }

        progress.selectedUpgrades.Add(node);
        progress.pendingTierQueue.Dequeue();
        progress.completedTierIndex = tierIndex;
        evolutionTiersInfo[tierIndex].hasBeenChosen = true;
        evolutionTiersInfo[tierIndex].selectedNode = node;

        bool hasNext = TryQueueNextTier(MapManager.Instance.GetInfectedRate(), false);
        if (!hasNext)
            onEvolutionTierFinishSelection.RaiseEvent();
        else
            ShowCurrentTier();
    }

    #endregion

    #region Evolution Tree

    public void OnEvolutionTreePanelOpened()
    {
        if (!progress.specializationChosen)
            return;

        if (!uiManager.IsEvolutionTreePanelInitialized())
            InitEvolutionTreePanel();

        uiManager.SetEvolutionTreePanelVisibility(true, evolutionTiersInfo);
    }

    private void InitEvolutionTreePanel()
    {
        EvolutionTreeSO tree = treeLookup[progress.selectedSpreadType];

        SpreadMethodDataSO spreadMethodData = SpreadMethodManager.Instance.GetSpreadMethodData(tree.spreadType);
        string spreadMethodName = spreadMethodData.methodName;
        Sprite spreadMethodIcon = spreadMethodData.methodIcon;
        uiManager.InitializeEvolutionTree(tree, spreadMethodName, spreadMethodIcon);
    }

    #endregion

    public void Init()
    {
        timeManager = TimeManager.Instance;
        uiManager = UIManager.Instance;
        spreadMethodManager = SpreadMethodManager.Instance;
        pointsManager = PointsManager.Instance;

        evolutionTiersInfo[0].hasShownNoti = true;
        BuildLookup();
    }

    private void OnValidate()
    {
        if (evolutionTiersInfo != null && evolutionTiersInfo.Count > 0)
        {
            evolutionTiersInfo = evolutionTiersInfo.OrderBy(t => t.tier).ToList();

            for (int i = 0; i < evolutionTiersInfo.Count - 1; i++)
            {
                if (evolutionTiersInfo[i].tier != i + 1)
                    Debug.LogError($"Evolution tiers must be in ascending order starting from 1. " +
                        $"Please fix the tier of {evolutionTiersInfo[i].tier} to be {i + 1}.");

                if (evolutionTiersInfo[i].infectedRate < 0)
                    Debug.LogError($"Evolution tiers must have non-negative infected rate required. " +
                        $"Please fix the infected rate required of tier {evolutionTiersInfo[i].tier}.");

                if (evolutionTiersInfo[i].infectedRate >= evolutionTiersInfo[i + 1].infectedRate)
                {
                    Debug.LogError($"Infected rate required of evolution tiers must be in ascending order. " +
                        $"Please fix the infected rate required of {evolutionTiersInfo[i].tier} and {evolutionTiersInfo[i + 1].tier}.");
                }
            }
            if (evolutionTiersInfo[^1].infectedRate < 0)
                Debug.LogError($"Evolution tiers must have non-negative infected rate required. " +
                    $"Please fix the infected rate required of tier {evolutionTiersInfo[^1].tier}.");
        }
    }
}