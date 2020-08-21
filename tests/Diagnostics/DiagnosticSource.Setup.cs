﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Unity.Diagnostics
{
    [TestClass]
    public partial class DiagnosticSourceTests
    {
        public void TestMethod()
        {

            using Activity activity = UnityDiagnosticSource.StartActivity(nameof(TestMethod));

            //try
            //{
            //    throw new Exception();
            //}
            //catch (Exception exception)
            //{
            //}
            //finally
            //{
            //}
        }

        protected ConcurrentQueue<(string eventName, object payload, Activity activity)> CreateEventQueue() =>
            new ConcurrentQueue<(string eventName, object payload, Activity activity)>();

        protected FakeDiagnosticListener CreateEventListener(string entityName, ConcurrentQueue<(string eventName, object payload, Activity activity)> eventQueue) =>
            new FakeDiagnosticListener(kvp =>
            {
                eventQueue?.Enqueue((kvp.Key, kvp.Value, Activity.Current));
            });
    }

    public sealed class FakeDiagnosticListener : IObserver<DiagnosticListener>, IDisposable
    {
        private IDisposable subscription;
        private class FakeDiagnosticSourceWriteObserver : IObserver<KeyValuePair<string, object>>
        {
            private readonly Action<KeyValuePair<string, object>> writeCallback;

            public FakeDiagnosticSourceWriteObserver(Action<KeyValuePair<string, object>> writeCallback)
            {
                this.writeCallback = writeCallback;
            }

            public void OnCompleted()
            {
            }

            public void OnError(Exception error)
            {
            }

            public void OnNext(KeyValuePair<string, object> value)
            {
                this.writeCallback(value);
            }
        }

        private readonly Action<KeyValuePair<string, object>> writeCallback;

        private Func<string, object, object, bool> writeObserverEnabled = (name, arg1, arg2) => true;

        public FakeDiagnosticListener(Action<KeyValuePair<string, object>> writeCallback)
        {
            this.writeCallback = writeCallback;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        public void OnNext(DiagnosticListener value)
        {
            if (value.Name.Equals("Unity.Container"))
            {
                this.subscription = value.Subscribe(new FakeDiagnosticSourceWriteObserver(this.writeCallback), this.IsEnabled);
            }
        }

        public void Enable()
        {
            this.writeObserverEnabled = (name, arg1, arg2) => true;
        }

        public void Enable(Func<string, bool> writeObserverEnabled)
        {
            this.writeObserverEnabled = (name, arg1, arg2) => writeObserverEnabled(name);
        }

        public void Enable(Func<string, object, object, bool> writeObserverEnabled)
        {
            this.writeObserverEnabled = writeObserverEnabled;
        }

        public void Disable()
        {
            this.writeObserverEnabled = (name, arg1, arg2) => false;
        }

        private bool IsEnabled(string s, object arg1, object arg2) =>
            this.writeObserverEnabled(s, arg1, arg2);

        public void Dispose()
        {
            this.Disable();
            this.subscription?.Dispose();
        }
    }
}