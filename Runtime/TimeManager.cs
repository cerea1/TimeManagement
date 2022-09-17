using System;
using System.Collections.Generic;

using Unity.Profiling;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

namespace CerealDevelopment.TimeManagement
{
	public interface ITimeScaleModifier
	{
		float TimeScale { get; }
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
			}
		}
		#endregion

		private List<ITimeScaleModifier> modifiers = new List<ITimeScaleModifier>();

		[SerializeField]
		private bool updatePhysics;


		private UnityComponentsList<IUpdatable> updatables = new UnityComponentsList<IUpdatable>();

		private UnityComponentsList<IFixedUpdatable> fixedUpdatables = new UnityComponentsList<IFixedUpdatable>();

		private UnityComponentsList<ILateUpdatable> lateUpdatables = new UnityComponentsList<ILateUpdatable>();

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
			{
				_instance = this;
				DontDestroyOnLoad(gameObject);
			}
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
			var index = 0;
			var deltaTime = Mathf.Min(Time.deltaTime, 1f / 30f);
			while (index < updatables.Count)
			{
				try
				{
					for (; index < updatables.Count; index++)
					{
						using (var profiler = new ProfilerMarker((updatables[index] as Component).name).Auto())
						{
							updatables[index].OnUpdate(deltaTime);
						}
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
		}


		/// <summary>
		/// Add <see cref="UnityEngine.Time.timeScale"/> modular modification
		/// </summary>
		/// <param name="modifier">Modifier to apply. Modifier value will multiply with others modifiers</param>
		public static void AddTimeScaleModifier(ITimeScaleModifier modifier)
		{
			Instance.modifiers.Add(modifier);
			Instance.UpdateTimeScale();
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
	}
}
