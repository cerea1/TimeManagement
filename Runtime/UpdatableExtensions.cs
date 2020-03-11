namespace CerealDevelopment.TimeManagement
{
    public static class UpdatableExtensions
    {
        /// <summary>
        /// Invokes <see cref="TimeManager.AddUpdatable(IUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void EnableUpdates(this IUpdatable updatable)
        {
            TimeManager.AddUpdatable(updatable);
        }

        /// <summary>
        /// Invokes <see cref="TimeManager.RemoveUpdatable(IUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void DisableUpdates(this IUpdatable updatable)
        {
            TimeManager.RemoveUpdatable(updatable);
        }

        /// <summary>
        /// Invokes <see cref="TimeManager.AddLateUpdatable(ILateUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void EnableLateUpdates(this ILateUpdatable updatable)
        {
            TimeManager.AddLateUpdatable(updatable);
        }

        /// <summary>
        /// Invokes <see cref="TimeManager.RemoveLateUpdatable(ILateUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void DisableLateUpdates(this ILateUpdatable updatable)
        {
            TimeManager.RemoveLateUpdatable(updatable);
        }


        /// <summary>
        /// Invokes <see cref="TimeManager.AddFixedUpdatable(IFixedUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void EnableFixedUpdates(this IFixedUpdatable updatable)
        {
            TimeManager.AddFixedUpdatable(updatable);
        }

        /// <summary>
        /// Invokes <see cref="TimeManager.RemoveFixedUpdatable(IFixedUpdatable)"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void DisableFixedUpdates(this IFixedUpdatable updatable)
        {
            TimeManager.RemoveFixedUpdatable(updatable);
        }

        /// <summary>
        /// Invokes <see cref="IUpdatable.OnUpdate"/>
        /// </summary>
        /// <param name="updatable"></param>
        public static void OnUpdate(this IUpdatable updatable)
        {
            updatable.OnUpdate();
        }

    }
}