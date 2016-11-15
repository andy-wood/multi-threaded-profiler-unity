using System.Diagnostics;
using System.Collections.Generic;

public class Profiler
{
	class Timer
	{
		public readonly string name;
		public readonly Stopwatch stopwatch = new Stopwatch ();
		public int calls;
		public Timer(string name) { this.name = name; } 
	}

	static Profiler _this;
	static object _lock = new object ();

	Stopwatch appTimer = new Stopwatch ();
	Dictionary<string, Timer> timers = new Dictionary<string, Timer> (100);

	public Profiler(bool start)
	{
		_this = this;

		if (start)
			Start ();
	}

	public static void Start()
	{
		if (!_this.appTimer.IsRunning)
			_this.appTimer.Start ();
	}

	public static void Stop()
	{
		if (_this.appTimer.IsRunning)
		{
			lock (_lock)
			{
				_this.appTimer.Stop ();

				for (var timer = _this.timers.Values.GetEnumerator (); timer.MoveNext ();)
					timer.Current.stopwatch.Stop ();
			}
		}
	}

	public static void Reset()
	{
		lock (_lock)
		{
			_this.appTimer.Reset ();
			_this.timers.Clear ();
		}
	}

	public static void BeginMethod(string name)
	{
		if (!_this.appTimer.IsRunning)
			return;

		lock (_lock)
		{
			if (!_this.timers.ContainsKey (name))
				_this.timers [name] = new Timer (name);

			var timer = _this.timers [name];
			
			if (timer.stopwatch.IsRunning)
			{
				UnityEngine.Debug.LogError ("Profiler.BeginMethod: Previous BeginMethod was not paired with EndMethod");
				return;
			}

			timer.stopwatch.Start ();
			++timer.calls;
		}		
	}

	public static void EndMethod(string name)
	{
		if (!_this.appTimer.IsRunning)
			return;

		lock (_lock)
		{
			if (!_this.timers.ContainsKey (name) || !_this.timers [name].stopwatch.IsRunning)
			{
				UnityEngine.Debug.LogError ("Profiler.EndMethod: Not paired with any previous BeginMethod");
				return;
			}

			_this.timers [name].stopwatch.Stop ();
		}
	}

	public static void WriteLog()
	{
		if (_this.timers.Count == 0)
			return;

		double appSeconds = _this.appTimer.Elapsed.TotalSeconds;

		UnityEngine.Debug.Log ("Profiler, Running Time: " + appSeconds + "s");
		
		List<Timer> sortedTimers;

		lock (_lock)
			sortedTimers = new List<Timer> (_this.timers.Values);

		sortedTimers.Sort ((timer1, timer2) => timer2.stopwatch.ElapsedTicks.CompareTo (timer1.stopwatch.ElapsedTicks));
			
		for (int i = 0; i < sortedTimers.Count; ++i)
		{
			var timer = sortedTimers [i];

			if (timer.stopwatch.IsRunning)
			{
				timer.stopwatch.Stop ();
				UnityEngine.Debug.LogWarning ("EndMethod was not called for " + timer.name);
			}
		}

		for (int i = 0; i < sortedTimers.Count; ++i)
		{
			var timer = sortedTimers [i];
			float seconds = (float)(timer.stopwatch.Elapsed.TotalSeconds);
			int percent = (int)(seconds * 100 / appSeconds);
			int milliseconds = (int)(seconds * 1000); 
			UnityEngine.Debug.Log (timer.name + " (" + percent + "%, " + timer.calls + " calls)" + ": " + seconds + "s (" + milliseconds + "ms)");
		}
	}
}
