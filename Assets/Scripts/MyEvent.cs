
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Events;
namespace Scoz.Func {
    public class MyEvent : MonoBehaviour {
        public List<UnityEvent> Events;
        public void DoEvent(int _idx) {
            if (_idx < Events.Count) Events[_idx]?.Invoke();
        }


    }
}