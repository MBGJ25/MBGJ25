#region State Interfaces
public interface IInitializable 
{
    void Initialize();
    void CleanUp();
}

public interface ISaveable 
{
    void Save();
    void Load();
}

public interface IResettable 
{
    void Reset();
}

#endregion