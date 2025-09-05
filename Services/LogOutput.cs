using Microsoft.AspNetCore.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AveTranslatorM.Services
{
    public class LogUserOutput : IReadOnlyList<string>, IDisposable
    {
        public List<string> Logs { get; private set; } = new();
        public event Action? OnLogAdded;

        private System.Threading.Timer? _throttleTimer;
        private readonly object _lock = new();
        private bool _hasPendingUpdate = false;
        private bool _hasNotifiedOnce = false;

        public void Clear()
        {
            lock (_lock)
            {
                Logs.Clear();
                _hasNotifiedOnce = false;
                StartOrContinueTimer(forceImmediate: true);
            }
        }

        public void Add(string log)
        {
            lock (_lock)
            {
                Logs.Add($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {log}");
                if (!_hasNotifiedOnce)
                {
                    _hasNotifiedOnce = true;
                    // Негайно повідомляємо про перший лог
                    OnLogAdded?.Invoke();
                }
                else
                {
                    StartOrContinueTimer();
                }
            }
        }

        private void StartOrContinueTimer(bool forceImmediate = false)
        {
            _hasPendingUpdate = true;
            if (forceImmediate)
            {
                OnLogAdded?.Invoke();
                return;
            }
            if (_throttleTimer == null)
            {
                _throttleTimer = new System.Threading.Timer(OnTimerTick, null, 500, 500);
            }
        }

        private void OnTimerTick(object? state)
        {
            bool shouldNotify = false;
            lock (_lock)
            {
                if (_hasPendingUpdate)
                {
                    shouldNotify = true;
                    _hasPendingUpdate = false;
                }
                else
                {
                    _throttleTimer?.Dispose();
                    _throttleTimer = null;
                }
            }
            if (shouldNotify)
                OnLogAdded?.Invoke();
        }

        public int Count => Logs.Count;
        public string this[int index] => Logs[index];
        public IEnumerator<string> GetEnumerator() => Logs.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Dispose()
        {
            lock (_lock)
            {
                _throttleTimer?.Dispose();
                _throttleTimer = null;
            }
        }
    }
}
