
namespace CerealDevelopment.TimeManagement
{
    /// <summary>
    /// <see cref="UnityEngine.Object"/> restriction for updatables
    /// </summary>
    public interface IUnityComponent
    {
        int GetInstanceID();
    }

    /// <summary>
    /// Update interface to use with <see cref="TimeManager"/>
    /// </summary>
    public interface IUpdatable : IUnityComponent
    {
        void OnUpdate();
    }
    /// <summary>
    /// LateUpdate interface to use with <see cref="TimeManager"/>
    /// </summary>
    public interface ILateUpdatable : IUnityComponent
    {
        void OnLateUpdate();
    }

    /// <summary>
    /// FixedUpdate interface to use with <see cref="TimeManager"/>
    /// </summary>
    public interface IFixedUpdatable : IUnityComponent
    {
        void OnFixedUpdate();
    }
}