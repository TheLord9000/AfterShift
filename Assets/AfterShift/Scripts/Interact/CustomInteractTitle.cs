using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UHFPS.Runtime;

namespace AfterShift.Runtime
{
    public class ASCustomInteractTitle : MonoBehaviour, IInteractTitle
    {
        [Serializable]
        public sealed class TitleState
        {
            public string StateName;

            public bool IsFallbackState;
            public MonoBehaviour Target;

            [ASBoolMethod(nameof(Target))]
            public string BoolMethodName;

            public bool InvertCondition;

            public bool DisableInteraction;

            public bool OverrideTitle;
            public GString Title;

            public bool OverrideUseTitle = true;
            public GString UseTitle;

            public bool OverrideExamineTitle;
            public GString ExamineTitle;
        }

        public List<TitleState> States = new();

        [Header("Layer Control")]
        public bool UpdateLayerByInteractionState = true;
        public bool IncludeChildren = true;
        public string InteractLayerName = "Interact";
        public string DisabledLayerName = "Ignore Raycast";

        private int interactLayer = -1;
        private int disabledLayer = -1;
        private bool lastCanInteract = true;

        private void Awake()
        {
            interactLayer = LayerMask.NameToLayer(InteractLayerName);
            disabledLayer = LayerMask.NameToLayer(DisabledLayerName);

            ApplyInteractionLayer(true);
        }

        private void Update()
        {
            ApplyInteractionLayer(false);
        }

        public TitleParams InteractTitle()
        {
            TitleState activeState = GetActiveState();

            if (activeState == null || activeState.DisableInteraction)
            {
                return new TitleParams()
                {
                    title = string.Empty,
                    button1 = string.Empty,
                    button2 = string.Empty
                };
            }

            return new TitleParams()
            {
                title = activeState.OverrideTitle ? activeState.Title : null,
                button1 = activeState.OverrideUseTitle ? activeState.UseTitle : null,
                button2 = activeState.OverrideExamineTitle ? activeState.ExamineTitle : null
            };
        }

        public bool CanInteract()
        {
            TitleState activeState = GetActiveState();

            if (activeState == null)
                return false;

            return !activeState.DisableInteraction;
        }

        public bool IsInteractionDisabled()
        {
            return !CanInteract();
        }

        private void ApplyInteractionLayer(bool force)
        {
            if (!UpdateLayerByInteractionState)
                return;

            bool canInteract = CanInteract();

            if (!force && canInteract == lastCanInteract)
                return;

            lastCanInteract = canInteract;

            int targetLayer = canInteract ? interactLayer : disabledLayer;

            if (targetLayer < 0)
            {
                Debug.LogWarning(
                    $"ASCustomInteractTitle: layer '{(canInteract ? InteractLayerName : DisabledLayerName)}' not found.",
                    this
                );
                return;
            }

            SetLayerRecursive(transform, targetLayer);
        }

        private void SetLayerRecursive(Transform target, int layer)
        {
            target.gameObject.layer = layer;

            if (!IncludeChildren)
                return;

            foreach (Transform child in target)
            {
                SetLayerRecursive(child, layer);
            }
        }

        private TitleState GetActiveState()
        {
            TitleState fallbackState = null;

            foreach (TitleState state in States)
            {
                if (state == null)
                    continue;

                if (state.IsFallbackState)
                {
                    fallbackState ??= state;
                    continue;
                }

                if (EvaluateState(state))
                    return state;
            }

            return fallbackState;
        }

        private bool EvaluateState(TitleState state)
        {
            if (state.IsFallbackState)
                return true;

            if (state.Target == null)
                return false;

            if (string.IsNullOrWhiteSpace(state.BoolMethodName))
                return false;

            Type targetType = state.Target.GetType();

            MethodInfo method = targetType.GetMethod(
                state.BoolMethodName,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
            );

            if (method == null)
            {
                Debug.LogWarning($"ASCustomInteractTitle: method '{state.BoolMethodName}' not found on '{targetType.Name}'.", this);
                return false;
            }

            if (method.ReturnType != typeof(bool))
            {
                Debug.LogWarning($"ASCustomInteractTitle: method '{state.BoolMethodName}' must return bool.", this);
                return false;
            }

            if (method.GetParameters().Length > 0)
            {
                Debug.LogWarning($"ASCustomInteractTitle: method '{state.BoolMethodName}' must have no parameters.", this);
                return false;
            }

            bool result = (bool)method.Invoke(state.Target, null);
            return state.InvertCondition ? !result : result;
        }
    }
}