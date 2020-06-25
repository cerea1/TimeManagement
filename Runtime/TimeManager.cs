using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.Profiling;

namespace CerealDevelopment.TimeManagement
{
	public interface ITimeScaleModifier
	{
		float TimeScale { get; }
	}

	internal class AsyncUpdater
	{
		public bool shouldRun => threadedUpdatables.Count > 0;

		private UnityComponentsList<ISeparateThreadUpdatable> threadedUpdatables = new UnityComponentsList<ISeparateThreadUpdatable>();

		public void AddUpdatable(ISeparateThreadUpdatable updatable)
		{
			lock (threadedUpdatables)
			{
				threadedUpdatables.AddUnique(updatable);
			}
		}

		public void RemoveUpdatable(ISeparateThreadUpdatable updatable)
		{
			lock (threadedUpdatables)
			{
				threadedUpdatables.RemoveSwapBack(updatable);
			}
		}

		public void RunBeforeUpdate()
		{
			var index = 0;

			while (index < threadedUpdatables.Count)
			{
				try
				{
					for (; index < threadedUpdatables.Count; index++)
					{
						threadedUpdatables[index].OnMainThreadUpdate();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
				}
			}
		}

		public void Run()
		{
			Profiler.BeginSample("TimeManager.AsyncUpdater.Run");

			var index = 0;

			while (index < threadedUpdatables.Count)
			{
				try
				{
					for (; index < threadedUpdatables.Count; index++)
					{
						threadedUpdatables[index].OnSeparateThreadUpdate();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
				}
			}

			Profiler.EndSample();
		}

		public void RunComplete()
		{
			var index = 0;

			while (index < threadedUpdatables.Count)
			{
				try
				{
					for (; index < threadedUpdatables.Count; index++)
					{
						threadedUpdatables[index].OnSeparateThreadComplete();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
				}
			}
		}
	}

	public class TimeManager : MonoBehaviour
	{
		#region Singleton
		private static bool IsRunning = true;
		private static TimeManager _instance;
		public static TimeManager Instance
		{
			get
			{
				if (IsRunning)
				{
					if (_instance == null)
					{
						_instance = FindObjectOfType<TimeManager>();
					}
					if (_instance == null)
					{
						var go = new GameObject(typeof(TimeManager).Name);
						go.AddComponent<TimeManager>();
					}
					if (_instance.asyncUpdater == null)
					{
						_instance.asyncUpdater = new AsyncUpdater();
					}

					return _instance;
				}
				return null;
			}
		}

		private void OnApplicationQuit()
		{
			if (_instance == this)
			{
				IsRunning = false;
				task?.Dispose();
			}
		}
		#endregion

		private List<ITimeScaleModifier> modifiers = new List<ITimeScaleModifier>();

		[SerializeField]
		private bool updatePhysics;

		private UnityComponentsList<IUpdatable> updatables = new UnityComponentsList<IUpdatable>();

		private UnityComponentsList<IFixedUpdatable> fixedUpdatables = new UnityComponentsList<IFixedUpdatable>();

		private UnityComponentsList<ILateUpdatable> lateUpdatables = new UnityComponentsList<ILateUpdatable>();

		private AsyncUpdater asyncUpdater;

		private static float _targetDeltaTime;
		public static float TargetUnscaledDeltaTime => _targetDeltaTime;
		public static float TargetDeltaTime => Time.timeScale * _targetDeltaTime;

		[RuntimeInitializeOnLoadMethod]
		private static void InitializeOnLoad()
		{
			Debug.Log(Instance.GetType().FullName);
		}

		private void Awake()
		{
			if (_instance != null)
			{
				if (_instance != this)
				{
					Destroy(gameObject);
					return;
				}
			}
			_instance = this;

			DontDestroyOnLoad(gameObject);
		}

		private void Start()
		{
			if (updatePhysics)
			{
				UpdatePhysics();
			}
		}

		private void Update()
		{
			OnPostRender();

			UpdateTimeScale();

			_targetDeltaTime = 1f / 60f;
			var index = 0;

			while (index < updatables.Count)
			{
				try
				{
					for (; index < updatables.Count; index++)
					{
						updatables[index].OnUpdate();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
					//updatables[hash].RemoveAt(index);
				}
			}
			UpdateTimeScale();
		}

		private void FixedUpdate()
		{
			var index = 0;

			while (index < fixedUpdatables.Count)
			{
				try
				{
					for (; index < fixedUpdatables.Count; index++)
					{
						fixedUpdatables[index].OnFixedUpdate();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
					//updatables[hash].RemoveAt(index);
				}
			}
		}

		Task task;
		bool taskRunning;
		private void LateUpdate()
		{
			if (updatePhysics)
			{
				UpdatePhysics();
			}

			var index = 0;

			while (index < lateUpdatables.Count)
			{
				try
				{
					for (; index < lateUpdatables.Count; index++)
					{
						lateUpdatables[index].OnLateUpdate();
					}
				}
				catch (Exception e)
				{
					Debug.LogException(e);
					index++;
					//updatables[hash].RemoveAt(index);
				}
			}

			OnPreRender();
		}

		private void OnPreRender()
		{
			if (asyncUpdater.shouldRun)
			{
				asyncUpdater.RunBeforeUpdate();

				taskRunning = true;
				task = Task.Factory.StartNew(asyncUpdater.Run);
			}
		}

		private void OnPostRender()
		{
			if (taskRunning)
			{
				taskRunning = false;
				task.Wait();
				task.Dispose();
				task = null;

				asyncUpdater.RunComplete();
			}
		}

		/// <summary>
		/// Add <see cref="UnityEngine.Time.timeScale"/> modular modification
		/// </summary>
		/// <param name="modifier">Modifier to apply. Modifier value will multiply with others modifiers</param>
		public static void AddTimeScaleModifier(ITimeScaleModifier modifier)
		{
			if (!Instance.modifiers.Contains(modifier))
			{
				Instance.modifiers.Add(modifier);
				Instance.UpdateTimeScale();
			}
		}

		/// <summary>
		/// Remove <see cref="UnityEngine.Time.timeScale"/> modular modification
		/// </summary>
		/// <param name="modifier">Previously added modifier</param>
		public static void RemoveTimeScaleModifier(ITimeScaleModifier modifier)
		{
			Instance.modifiers.Remove(modifier);
			Instance.UpdateTimeScale();
		}

		public static float GetTimeScaleExcept(params ITimeScaleModifier[] exceptModifiers)
		{
			return Instance.GetTimeScaleExceptInternal(exceptModifiers);
		}

		public float GetTimeScaleExceptInternal(params ITimeScaleModifier[] exceptModifiers)
		{
			var timeScale = 1f;
			for (int i = 0; i < modifiers.Count; i++)
			{
				var notFound = true;
				for (int j = 0; j < exceptModifiers.Length && notFound; j++)
				{
					notFound = modifiers[i] == exceptModifiers[j];
				}
				if (notFound)
				{
					timeScale *= modifiers[i].TimeScale;
				}
			}
			return timeScale;
		}

		private void UpdateTimeScale()
		{
			var timeScale = 1f;
			for (int i = 0; i < modifiers.Count; i++)
			{
				timeScale *= modifiers[i].TimeScale;
			}
			UnityEngine.Time.timeScale = timeScale;
		}

		private void UpdatePhysics()
		{
			var deltaTime = UnityEngine.Time.unscaledDeltaTime;
			UnityEngine.Time.fixedDeltaTime = deltaTime;
			if (UnityEngine.Time.deltaTime > 0f)
			{
				var clampedTime = Mathf.Clamp(UnityEngine.Time.deltaTime, 0f, 1f / 15f);
				//Physics.SyncTransforms();
				Physics.Simulate(clampedTime);
			}
		}

		/// <summary>
		/// Enable <see cref="IUpdatable"/> updates
		/// </summary>
		/// <param name="updatable">Updatable to enable</param>
		public static void AddUpdatable(IUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.updatables.AddUnique(updatable);
			}
		}

		/// <summary>
		/// Disable <see cref="IUpdatable"/> updates
		/// </summary>
		/// <param name="updatable">Updatable to disable</param>
		public static void RemoveUpdatable(IUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.updatables.RemoveSwapBack(updatable);
			}
		}
		/// <summary>
		/// Enable <see cref="IFixedUpdatable"/> fixed updates
		/// </summary>
		/// <param name="updatable">Updatable to enable</param>
		public static void AddFixedUpdatable(IFixedUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.fixedUpdatables.AddUnique(updatable);
			}
		}

		/// <summary>
		/// Disable <see cref="IFixedUpdatable"/> fixed updates
		/// </summary>
		/// <param name="updatable">Updatable to disable</param>
		public static void RemoveFixedUpdatable(IFixedUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.fixedUpdatables.RemoveSwapBack(updatable);
			}
		}

		/// <summary>
		/// Enable <see cref="ILateUpdatable"/> late updates
		/// </summary>
		/// <param name="updatable">Updatable to enable</param>
		public static void AddLateUpdatable(ILateUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.lateUpdatables.AddUnique(updatable);
			}
		}

		/// <summary>
		/// Disable <see cref="ILateUpdatable"/> late updates
		/// </summary>
		/// <param name="updatable">Updatable to disable</param>
		public static void RemoveLateUpdatable(ILateUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.lateUpdatables.RemoveSwapBack(updatable);
			}
		}

		/// <summary>
		/// Enable <see cref="ILateUpdatable"/> late updates
		/// </summary>
		/// <param name="updatable">Updatable to enable</param>
		public static void AddAsyncUpdatable(ISeparateThreadUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.asyncUpdater.AddUpdatable(updatable);
			}
		}

		/// <summary>
		/// Disable <see cref="ILateUpdatable"/> late updates
		/// </summary>
		/// <param name="updatable">Updatable to disable</param>
		public static void RemoveAsyncUpdatable(ISeparateThreadUpdatable updatable)
		{
			if (IsRunning)
			{
				Instance.asyncUpdater.RemoveUpdatable(updatable);
			}
		}
	}
}