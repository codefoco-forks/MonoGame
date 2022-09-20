using System.Collections.Concurrent;

namespace Microsoft.Xna.Framework.Input.Touch
{
    /// <summary>
    /// Stores touches to apply them once a frame for platforms that dispatch touches asynchronously
    /// while user code is running.
    /// </summary>
    internal class TouchQueue
    {
        private readonly ConcurrentQueue<TouchEvent> _queue = new ConcurrentQueue<TouchEvent>();

        public void Enqueue(int id, TouchLocationState state, Vector2 pos, DeviceType deviceType, float pressure, float rotation)
        {
            _queue.Enqueue(new TouchEvent(id, state, pos, deviceType, pressure, rotation));
        }

        public void ProcessQueued()
        {
            TouchEvent ev;
            while (_queue.TryDequeue(out ev))                
                TouchPanel.AddEvent(ev.Id, ev.State, ev.Pos, ev.DeviceType, ev.Pressure, ev.Rotation);
        }

        private struct TouchEvent
        {
            public readonly int Id;
            public readonly TouchLocationState State;
            public readonly Vector2 Pos;
            public readonly DeviceType DeviceType;
            public readonly float Pressure;
            public readonly float Rotation;

            public TouchEvent(int id, TouchLocationState state, Vector2 pos, DeviceType deviceType, float pressure, float rotation)
            {
                Id = id;
                State = state;
                Pos = pos;
                DeviceType = deviceType;
                Pressure = pressure;
                Rotation = rotation;
            }
        }

    }
}
