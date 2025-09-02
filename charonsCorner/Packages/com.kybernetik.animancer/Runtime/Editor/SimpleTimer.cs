// Animancer // https://kybernetik.com.au/animancer // Copyright 2018-2025 Kybernetik //

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace Animancer
{
    /// <summary>A very simple timer system based on a <see cref="System.Diagnostics.Stopwatch"/>.</summary>
    public struct SimpleTimer : IDisposable
    {
        /************************************************************************************************************************/

        /// <summary>The default <see cref="format"/> contains 3 decimal places.</summary>
        const string Format3DP = "0.000";

        /// <summary>A default timer that hasn't been started.</summary>
        public static SimpleTimer Default = new(null);

        /************************************************************************************************************************/

        /// <summary>The system used to track time.</summary>
        public static readonly Stopwatch
             Stopwatch = Stopwatch.StartNew();

        /************************************************************************************************************************/

        /// <summary>An optional prefix for <see cref="ToString"/>.</summary>
        public string name;

        /// <summary>The string format to use for <see cref="ToString"/>.</summary>
        /// <remarks>
        /// If <c>null</c>, ticks will be used directly.
        /// Otherwise, the ticks will be converted to seconds and this format will be used.
        /// </remarks>
        public string format;

        /// <summary>The <see cref="Stopwatch.ElapsedTicks"/> from when this timer was started.</summary>
        /// <remarks>If not started, this value will be <c>-1</c>.</remarks>
        public long startTicks;

        /// <summary>The total number of ticks that have elapsed since the <see cref="startTicks"/>.</summary>
        /// <remarks>This value is updated by <see cref="Count"/>.</remarks>
        public long totalTicks;

        /************************************************************************************************************************/

        /// <summary>Converts the <see cref="startTicks"/> to seconds.</summary>
        public readonly double StartTimeSeconds
            => startTicks / (double)Stopwatch.Frequency;

        /// <summary>Converts the <see cref="totalTicks"/> to seconds.</summary>
        public readonly double TotalTimeSeconds
            => totalTicks / (double)Stopwatch.Frequency;

        /************************************************************************************************************************/

        /// <summary>Has <see cref="Start()"/> been called and <see cref="Count"/> not?</summary>
        public readonly bool IsStarted
            => startTicks != -1;

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SimpleTimer"/> with the specified `name`.</summary>
        /// <remarks>
        /// You will need to call <see cref="Start()"/> to start the timer.
        /// Or use the static <see cref="Start(string, string)"/>.
        /// <para></para>
        /// Use <c>null</c> as the `format` to have <see cref="Format"/> return the ticks instead of seconds.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleTimer(string name, string format = Format3DP)
        {
            this.name = name;
            this.format = format;
            startTicks = -1;
            totalTicks = 0;
        }

        /************************************************************************************************************************/

        /// <summary>Creates a new <see cref="SimpleTimer"/> with the specified `name` and starts it.</summary>
        /// <remarks>Use <c>null</c> as the `format` to have <see cref="Format"/> return the ticks instead of seconds.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static SimpleTimer Start(string name = null, string format = Format3DP)
             => new()
             {
                 name = name,
                 format = format,
                 startTicks = Stopwatch.ElapsedTicks,
             };

        /************************************************************************************************************************/

        /// <summary>
        /// Stores the <see cref="Stopwatch.ElapsedTicks"/> in <see cref="startTicks"/>
        /// so that <see cref="Count"/> will be able to calculate how much time has passed.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Start()
            => startTicks = Stopwatch.ElapsedTicks;

        /// <summary>Clears the <see cref="startTicks"/>.</summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Cancel()
            => startTicks = -1;

        /************************************************************************************************************************/

        /// <summary>
        /// Calculates the amount of time that has passed since the <see cref="startTicks"/>
        /// and returns it after adding it to the <see cref="totalTicks"/>.
        /// Also resumes this timer.
        /// </summary>
        /// <remarks>Returns -1 if this timer wasn't started.</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long Count()
        {
            var endTicks = Stopwatch.ElapsedTicks;

            long count;
            if (startTicks >= 0)
            {
                count = endTicks - startTicks;
                totalTicks += count;
            }
            else
            {
                count = -1;
            }

            startTicks = endTicks;

            return count;
        }

        /************************************************************************************************************************/

        private static StringBuilder _StringBuilder;

        /// <summary>Calls <see cref="Count"/> and returns a string describing the current values of this timer.</summary>
        public override string ToString()
        {
            var count = Count();

            if (_StringBuilder == null)
                _StringBuilder = new();
            else
                _StringBuilder.Length = 0;

            if (!string.IsNullOrEmpty(name))
                _StringBuilder.Append(name)
                    .Append(": ");

            if (count != totalTicks && count >= 0)
            {
                _StringBuilder
                    .Append("Count ")
                    .Append(Format(count))
                    .Append(", Total ");
            }

            _StringBuilder.Append(Format(totalTicks));

            return _StringBuilder.ToString();
        }

        /************************************************************************************************************************/

        /// <summary>Converts the given `ticks` to a string using the <see cref="format"/>.</summary>
        public readonly string Format(long ticks)
            => format is null
            ? $"{ticks} Ticks"
            : $"{(ticks / (double)Stopwatch.Frequency).ToString(format)}s";

        /************************************************************************************************************************/

        /// <summary>Logs <see cref="ToString"/> and calls <see cref="Cancel"/>.</summary>
        public void Dispose()
        {
            UnityEngine.Debug.Log(ToString());
            Cancel();
            totalTicks = 0;
        }

        /************************************************************************************************************************/
    }
}

