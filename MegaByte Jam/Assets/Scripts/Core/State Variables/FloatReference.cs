using System;

[Serializable]
public class FloatReference
{
    public FloatVariable Variable;
    public float ConstantValue;
    public bool useConstant = false;

    public float Value
    {
        get
        {
            return useConstant ? ConstantValue : Variable.Value;
        }
    }
}
