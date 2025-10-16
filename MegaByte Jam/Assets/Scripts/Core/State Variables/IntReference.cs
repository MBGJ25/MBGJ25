using System;

[Serializable]
public class IntReference
{
    public IntVariable Variable;
    public int ConstantValue;
    public bool useConstant;

    public int Value
    {
        get
        {
            return useConstant ? ConstantValue : Variable.Value;
        }
    }
}
