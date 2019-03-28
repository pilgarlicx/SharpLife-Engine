﻿/***
*
*	Copyright (c) 1996-2001, Valve LLC. All rights reserved.
*	
*	This product contains software technology licensed from Id 
*	Software, Inc. ("Id Technology").  Id Technology (c) 1996 Id Software, Inc. 
*	All Rights Reserved.
*
*   This source code contains proprietary and confidential information of
*   Valve LLC and its suppliers.  Access to this code is restricted to
*   persons who have executed a written SDK license with Valve.  Any access,
*   use or distribution of this code by or to any unlicensed person is illegal.
*
****/

using System;
using System.Collections.Generic;

namespace SharpLife.Utility.Events
{
    /// <summary>
    /// The event system allows named events to be dispatched to listeners that want to know about them
    /// Events can contain data, represented as classes inheriting from a base event data class
    /// Listeners cannot be added or removed while an event dispatch is ongoing, they will be queued up and processed after the dispatch
    /// </summary>
    public class EventSystem
    {
        /// <summary>
        /// Indicates whether the event system is currently dispatching events
        /// </summary>
        public bool IsDispatching => _inDispatchCount > 0;

        private readonly Dictionary<string, EventMetaData> _events = new Dictionary<string, EventMetaData>();

        /// <summary>
        /// Keeps track of our nested dispatch count
        /// </summary>
        private int _inDispatchCount;

        private readonly List<Delegates.PostDispatchCallback> _postDispatchCallbacks = new List<Delegates.PostDispatchCallback>();

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Event name must be valid", nameof(name));
            }
        }

        /// <summary>
        /// Returns whether the given event exists
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasEvent(string name)
        {
            ValidateName(name);

            return _events.ContainsKey(name);
        }

        private void InternalRegisterEvent(string name, Type dataType)
        {
            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot register events while dispatching");
            }

            if (HasEvent(name))
            {
                throw new ArgumentException($"Event \"{name}\" has already been registered");
            }

            _events.Add(name, new EventMetaData(name, dataType));
        }

        /// <summary>
        /// Registers an event name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public void RegisterEvent(string name)
        {
            RegisterEvent<EmptyEventData>(name);
        }

        /// <summary>
        /// Registers an event name
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="name"></param>
        public void RegisterEvent<TDataType>(string name) where TDataType : EventData
        {
            ValidateName(name);

            InternalRegisterEvent(name, typeof(TDataType));
        }

        /// <summary>
        /// Registers an event name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dataType"></param>
        public void RegisterEvent(string name, Type dataType)
        {
            ValidateName(name);

            if (dataType == null)
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            if (!typeof(EventData).IsAssignableFrom(dataType))
            {
                throw new InvalidOperationException($"Event \"{name}\" has data type {dataType.FullName}\", not compatible with data type \"{typeof(EventData).FullName}\"");
            }

            InternalRegisterEvent(name, dataType);
        }

        /// <summary>
        /// Unregisters an event
        /// Also removes all listeners for the event
        /// </summary>
        /// <param name="name"></param>
        public void UnregisterEvent(string name)
        {
            ValidateName(name);

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot unregister events while dispatching");
            }

            _events.Remove(name);
        }

        /// <summary>
        /// Adds a listener for a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void AddListener(string name, Delegates.Listener listener)
        {
            AddListener<EmptyEventData>(name, listener);
        }

        private void InternalAddListener<TDataType>(string name, Invoker invoker) where TDataType : EventData
        {
            if (!_events.TryGetValue(name, out var metaData))
            {
                throw new InvalidOperationException($"Event \"{name}\" has not been registered");
            }

            var dataType = typeof(TDataType);

            if (!metaData.DataType.IsAssignableFrom(dataType))
            {
                throw new InvalidOperationException($"Event \"{name}\" has data type {metaData.DataType.FullName}\", not compatible with data type \"{dataType.FullName}\"");
            }

            metaData.Listeners.Add(invoker);
        }

        /// <summary>
        /// Adds a listener for a specific event
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void AddListener<TDataType>(string name, Delegates.Listener listener) where TDataType : EventData
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            InternalAddListener<TDataType>(name, new PlainInvoker(listener));
        }

        /// <summary>
        /// Adds a listener for a specific event
        /// The event name is inferred from the type
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="listener"></param>
        public void AddListener<TDataType>(Delegates.Listener listener) where TDataType : EventData
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            var name = EventUtils.EventName<TDataType>();

            InternalAddListener<TDataType>(name, new PlainInvoker(listener));
        }

        /// <summary>
        /// Adds a listener for a specific event, taking the data as a separate argument
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void AddListener<TDataType>(string name, Delegates.DataListener<TDataType> listener) where TDataType : EventData
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            InternalAddListener<TDataType>(name, new DataInvoker<TDataType>(listener));
        }

        /// <summary>
        /// Adds a listener to multiple events
        /// <seealso cref="M:SharpLife.Utility.Events.IEventSystem.AddListener(System.String,SharpLife.Utility.Events.Delegates.Listener)" />
        /// </summary>
        /// <param name="names">List of names</param>
        /// <param name="listener"></param>
        public void AddListeners(string[] names, Delegates.Listener listener)
        {
            AddListeners<EmptyEventData>(names, listener);
        }

        /// <summary>
        /// Adds a listener for a specific event, taking the data as a separate argument
        /// The event name is inferred from the type
        /// </summary>
        /// <typeparam name="TDataType"></typeparam>
        /// <param name="listener"></param>
        public void AddListener<TDataType>(Delegates.DataListener<TDataType> listener) where TDataType : EventData
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            var name = EventUtils.EventName<TDataType>();

            InternalAddListener<TDataType>(name, new DataInvoker<TDataType>(listener));
        }

        /// <summary>
        /// Adds a listener to multiple events
        /// <seealso cref="M:SharpLife.Utility.Events.IEventSystem.AddListener(System.String,SharpLife.Utility.Events.Delegates.Listener)" />
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="names">List of names</param>
        /// <param name="listener"></param>
        public void AddListeners<TDataType>(string[] names, Delegates.Listener listener) where TDataType : EventData
        {
            if (names == null)
            {
                throw new ArgumentNullException(nameof(names));
            }

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot add listeners while dispatching");
            }

            foreach (var name in names)
            {
                AddListener<TDataType>(name, listener);
            }
        }

        /// <summary>
        /// Removes all listeners of a specific event
        /// </summary>
        /// <param name="name"></param>
        public void RemoveListeners(string name)
        {
            ValidateName(name);

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            if (_events.TryGetValue(name, out var metaData))
            {
                metaData.Listeners.Clear();
            }
        }

        /// <summary>
        /// Removes a listener by delegate
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(Delegates.Listener listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(invoker => ReferenceEquals(invoker.Target, listener));
            }
        }

        /// <summary>
        /// Removes a listener from a specific event
        /// </summary>
        /// <param name="name"></param>
        /// <param name="listener"></param>
        public void RemoveListener(string name, Delegates.Listener listener)
        {
            ValidateName(name);

            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            if (_events.TryGetValue(name, out var metaData))
            {
                var index = metaData.Listeners.FindIndex(invoker => invoker.Delegate.Equals(listener));

                if (index != -1)
                {
                    metaData.Listeners.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// Removes the given listener from all events that is it listening to
        /// </summary>
        /// <param name="listener"></param>
        public void RemoveListener(object listener)
        {
            if (listener == null)
            {
                throw new ArgumentNullException(nameof(listener));
            }

            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.RemoveAll(delegateListener => delegateListener.Target == listener);
            }
        }

        /// <summary>
        /// Removes all listeners
        /// </summary>
        public void RemoveAllListeners()
        {
            if (IsDispatching)
            {
                throw new InvalidOperationException("Cannot remove listeners while dispatching");
            }

            foreach (var metaData in _events)
            {
                metaData.Value.Listeners.Clear();
            }
        }

        /// <summary>
        /// Dispatches an event to all listeners of that event
        /// An instance of <see cref="EmptyEventData" /> is provided as data
        /// </summary>
        /// <param name="name"></param>
        public void DispatchEvent(string name)
        {
            DispatchEvent(name, EmptyEventData.Instance);
        }

        private void InternalDispatchEvent<TDataType>(string name, TDataType data) where TDataType : EventData
        {
            if (_events.TryGetValue(name, out var metaData))
            {
                var @event = new Event(this, name, data);

                ++_inDispatchCount;

                for (var i = 0; i < metaData.Listeners.Count; ++i)
                {
                    metaData.Listeners[i].Invoke(@event);
                }

                --_inDispatchCount;

                if (_inDispatchCount == 0 && _postDispatchCallbacks.Count > 0)
                {
                    _postDispatchCallbacks.ForEach(callback => callback(this));
                    _postDispatchCallbacks.Clear();
                    //Avoid wasting memory, since this is a rarely used operation
                    _postDispatchCallbacks.Capacity = 0;
                }
            }
        }

        /// <summary>
        /// Dispatches an event to all listeners of that event
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="name"></param>
        /// <param name="data">Data to provide to listeners</param>
        /// <exception cref="ArgumentNullException">If name or data are null</exception>
        public void DispatchEvent<TDataType>(string name, TDataType data) where TDataType : EventData
        {
            ValidateName(name);

            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            InternalDispatchEvent(name, data);
        }

        /// <summary>
        /// Dispatches an event to all listeners of that event
        /// The event name is inferred from the data type
        /// </summary>
        /// <typeparam name="TDataType">Event data type</typeparam>
        /// <param name="data">Data to provide to listeners</param>
        /// <exception cref="ArgumentNullException">If name or data are null</exception>
        public void DispatchEvent<TDataType>(TDataType data) where TDataType : EventData
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var name = EventUtils.EventName<TDataType>();

            InternalDispatchEvent(name, data);
        }

        /// <summary>
        /// Adds a post dispatch callback
        /// Use this when adding or removing listeners or events while in an event dispatch
        /// </summary>
        /// <param name="callback"></param>
        /// <exception cref="InvalidOperationException">If a callback is added while not in an event dispatch</exception>
        public void AddPostDispatchCallback(Delegates.PostDispatchCallback callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (!IsDispatching)
            {
                throw new InvalidOperationException("Can only add post dispatch callbacks while dispatching events");
            }

            _postDispatchCallbacks.Add(callback);
        }
    }
}
