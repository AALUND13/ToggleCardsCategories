using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnboundLib;
using UnityEngine.Events;

namespace ToggleCardsCategories.Extensions {
    internal static class UnityEventExtensions {
        public static void AddListenerLast<T0>(this UnityEvent<T0> unityEvent, UnityAction<T0> call) {
            var invokableCallList = unityEvent.GetFieldValue("m_Calls");
            var runtimeCalls = invokableCallList.GetFieldValue("m_RuntimeCalls");

            var delegateMethod = unityEvent.InvokeMethod("GetDelegate", new Type[] { typeof(UnityAction<T0>) }, call);
            runtimeCalls.InvokeMethod("Insert", 0, delegateMethod);
        }
    }
}
