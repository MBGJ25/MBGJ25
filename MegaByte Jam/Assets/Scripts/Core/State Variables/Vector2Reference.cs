using System;
using UnityEngine;

[Serializable]
public class Vector2Reference
{
    public Vector2Variable Variable;
    public Vector2 ConstantCurrentValue;
    public Vector2 ConstantPreviousValue;
    public bool useConstant = false;

    public Vector2 CurrentValue
    {
        get
        {
            return useConstant ? ConstantCurrentValue : Variable.CurrentValue;
        }
        set
        {
            if (useConstant)
            {
                if (ConstantCurrentValue == null)
                {
                    ConstantPreviousValue = new Vector2(0, 0);
                } else
                {
                    ConstantPreviousValue = ConstantCurrentValue;
                }
                ConstantCurrentValue = value;
            }
            else
            {
                if (Variable.CurrentValue == null)
                {
                    Variable.PreviousValue = new Vector2(0, 0);
                } else
                {
                    Variable.PreviousValue = Variable.CurrentValue;
                }
                Variable.CurrentValue = value;
            }
        }
    }

    public Vector2 PreviousValue {
        get
        {
            return useConstant ? ConstantPreviousValue : Variable.PreviousValue;
        }
        set
        {
            if (useConstant)
            {
                ConstantPreviousValue = value;
            }
            else
            {
                Variable.PreviousValue = value;
            }
        }
    }

}
