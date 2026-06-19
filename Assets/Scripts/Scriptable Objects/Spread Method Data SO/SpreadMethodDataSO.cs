using UnityEngine;

public abstract class SpreadMethodDataSO : ScriptableObject
{
    public string methodName;
    [TextArea]
    public string description;
    public Sprite methodIcon;

    public SpreadMethodConfig baseConfig;

    public abstract ISpreadMethod CreateMethod(SpreadMethodContext context);
}
