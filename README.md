# multi-threaded-profiler-unity
Profiler class for profiling multi-threaded Unity apps. Unity's profiler only profiles the main thread. If you need to measure your other threads, you need a different solution. This Profiler class is simple but very useful.

Usage:

Create an instance of Profiler in any Awake():

new Profiler(bStartNow);

Control the profiler by calling its static methods. (Start, Stop, Reset)

Bracket any code you want to time with Profiler.BeginMethod("MethodName") and Profiler.EndMethod("MethodName") - make sure strings match. You can also think of these names as generic named timers - they don't have to represent whole methods.

Call Profiler.WriteLog() to output results. Typically you'd call this once when the program ends, but you can also call it multiple times during execution to get current totals.
