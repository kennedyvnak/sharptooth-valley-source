using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using NFHGame.ScriptableSingletons;

namespace NFHGame.Input {
    public class InputReader : ScriptableSingleton<InputReader>, Configs.IBootableSingleton {
        [Flags]
        public enum InputMap {
            None = -1,
            Gameplay = 1 << 0,
            UI = 1 << 1,
            Dialogue = 1 << 2,
            QuickTimeEvents = 1 << 3,
            All = Gameplay | UI | Dialogue
        }

        public event System.Action<int> OnMoveAxis; // Direction. -1 | 0 | 1
        public event System.Action<bool> OnRun;
        public event System.Action<Vector2> OnMouseClick; // Screen Position 
        public event System.Action<Vector2> OnPointerPosition; // Screen Position 
        public event System.Action OnOpenInventory;
        public event System.Action OnToggleHalo;
        public event System.Action<bool> OnForceField;
        public event System.Action OnPause;
        public event System.Action<int> OnVerticalAxis; // Direction. -1 | 0 | 1

        public event System.Action OnCancel;
        public event System.Action OnCloseInventory;
        public event System.Action<Vector2> OnNavigate;

        public event System.Action OnStepDialogue;
        public event System.Action<int> OnDialogueNavigate;

        public event System.Action QTE_ForceField;
        public event System.Action QTE_ToggleHalo;
        public event System.Action QTE_GetUp;
        public event System.Action QTE_OpenInventory;

        public event System.Action<InputMap> MapToggled;

        private InputMap _defaultMap;
        private Stack<InputMap> _mapStack;

        public InputMap currentMap { get; private set; }
        public InputMap defaultMap {
            get => _defaultMap;
            set {
                GameLogger.input.Log($"Set default map to {value}", LogLevel.Verbose);
                _defaultMap = value;
                if (_mapStack.Count == 0)
                    EnableMap(_defaultMap, true);
            }
        }

        void Configs.IBootableSingleton.Initialize() {
            _mapStack = new Stack<InputMap>();
            defaultMap = InputMap.Gameplay | InputMap.UI;
        }

        public void PushMap(InputMap map) {
            GameLogger.input.Log($"PushMap({map})", LogLevel.Verbose);
            _mapStack.Push(map);
            EnableMap(map);
        }

        public void PopMap(InputMap map = InputMap.All) {
            GameLogger.input.Log($"PopMap({map})", LogLevel.Verbose);
            if (_mapStack.Count == 0) {
                EnableMap(defaultMap);
                return;
            }
            if (_mapStack.Peek() != map) return;

            GameLogger.input.Log($"_mapStack.Pop())", LogLevel.FunctionScope);
            _mapStack.Pop();

            if (_mapStack.Count > 0) {
                EnableMap(_mapStack.Peek());
            } else {
                EnableMap(defaultMap);
            }
        }

        public void EnableMap(InputMap map, bool force = false) {
            if (map == currentMap && !force) return;

            GameLogger.input.Log($"Enabled map {map}", LogLevel.Default);

            currentMap = map;

            var actions = InputManager.instance.playerInput.actions;

            ToggleMap(InputMap.Gameplay);
            ToggleMap(InputMap.UI);
            ToggleMap(InputMap.Dialogue);
            ToggleMap(InputMap.QuickTimeEvents);

            MapToggled?.Invoke(currentMap);

            void ToggleMap(InputMap inputMap) {
                var acMap = actions.FindActionMap(Enum.GetName(typeof(InputMap), inputMap), true);
                if (map != InputMap.None && map.HasFlag(inputMap)) acMap.Enable();
                else acMap.Disable();
                GameLogger.input.Log($"{acMap.name}: {acMap.enabled}", LogLevel.FunctionScope);
            }
        }

        public void INPUT_OnMoveAxis(InputAction.CallbackContext context) {
            if (context.performed) {
                OnMoveAxis?.Invoke((int)context.ReadValue<float>());
            } else if (context.canceled) {
                OnMoveAxis?.Invoke(0);
            }
        }

        public void INPUT_OnRun(InputAction.CallbackContext context) {
            if (context.performed) {
                OnRun?.Invoke(true);
            } else if (context.canceled) {
                OnRun?.Invoke(false);
            }
        }

        public void INPUT_OnMouseClick(InputAction.CallbackContext context) {
            if (context.started) {
                OnMouseClick?.Invoke(context.ReadValue<Vector2>());
            }
        }

        public void INPUT_OnPointerPosition(InputAction.CallbackContext context) {
            if (context.performed) {
                OnPointerPosition?.Invoke(context.ReadValue<Vector2>());
            }
        }

        public void INPUT_OnOpenInventory(InputAction.CallbackContext context) {
            if (context.performed) {
                OnOpenInventory?.Invoke();
            }
        }

        public void INPUT_OnToggleHalo(InputAction.CallbackContext context) {
            if (context.performed) {
                OnToggleHalo?.Invoke();
            }
        }

        public void INPUT_OnForceField(InputAction.CallbackContext context) {
            if (context.performed) {
                OnForceField?.Invoke(true);
            } else if (context.canceled) {
                OnForceField?.Invoke(false);
            }
        }

        public void INPUT_OnPause(InputAction.CallbackContext context) {
            if (context.performed) {
                OnPause?.Invoke();
            }
        }

        public void INPUT_OnVerticalAxis(InputAction.CallbackContext context) {
            if (context.performed) {
                OnVerticalAxis?.Invoke((int)context.ReadValue<float>());
            } else if (context.canceled) {
                OnVerticalAxis?.Invoke(0);
            }
        }

        public void INPUT_OnNavigate(InputAction.CallbackContext context) {
            if (context.performed) {
                OnNavigate?.Invoke(context.ReadValue<Vector2>());
            } else if (context.canceled) {
                OnNavigate?.Invoke(Vector2.zero);
            }
        }

        public void INPUT_OnSubmit(InputAction.CallbackContext context) {
        }

        public void INPUT_OnCancel(InputAction.CallbackContext context) {
            if (context.performed) {
                OnCancel?.Invoke();
            }
        }

        public void INPUT_OnCloseInventory(InputAction.CallbackContext context) {
            if (context.performed) {
                OnCloseInventory?.Invoke();
            }
        }

        public void INPUT_OnPoint(InputAction.CallbackContext context) {
        }

        public void INPUT_OnClick(InputAction.CallbackContext context) {
        }

        public void INPUT_OnScrollWheel(InputAction.CallbackContext context) {
        }

        public void INPUT_OnMiddleClick(InputAction.CallbackContext context) {
        }

        public void INPUT_OnRightClick(InputAction.CallbackContext context) {
        }

        public void INPUT_OnStepDialogue(InputAction.CallbackContext context) {
            if (context.performed) {
                OnStepDialogue?.Invoke();
            }
        }

        public void INPUT_OnDialogueNavigate(InputAction.CallbackContext context) {
            if (context.performed) {
                int y = (int)context.ReadValue<Vector2>().y;
                if (y != 0.0f) {
                    OnDialogueNavigate?.Invoke(y);
                }
            }
        }

        public void INPUT_QTE_OnForceField(InputAction.CallbackContext context) {
            if (context.performed) {
                QTE_ForceField?.Invoke();
            }
        }

        public void INPUT_QTE_OnToggleHalo(InputAction.CallbackContext context) {
            if (context.performed) {
                QTE_ToggleHalo?.Invoke();
            }
        }

        public void INPUT_QTE_OnGetUp(InputAction.CallbackContext context) {
            if (context.performed) {
                QTE_GetUp?.Invoke();
            }
        }

        public void INPUT_QTE_OnOpenInventory(InputAction.CallbackContext context) {
            if (context.performed) {
                QTE_OpenInventory?.Invoke();
            }
        }
    }
}
