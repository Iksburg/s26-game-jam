using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace CatWorld.Cats.Editor
{
    /// <summary>
    /// Настраивает переходы Idle ↔ Walk в Animator-контроллерах котов:
    /// добавляет float-параметр Speed и проставляет условия на существующие
    /// переходы (скорость больше нуля → Walk, скорость ноль → Idle).
    /// Скорость в параметр пишет CatAnimationController.
    /// </summary>
    public static class CatAnimatorSetup
    {
        private const string KittenControllerPath =
            "Assets/Art/Animations/Kitten/Controller/KittenController.controller";

        [MenuItem("Tools/CatWorld/Setup Idle-Walk Transitions (Kitten)")]
        public static void SetupKitten()
        {
            SetupLocomotion(KittenControllerPath, "KittenIdle", "KittenWalk");
        }

        /// <summary>
        /// Проставляет параметр и условия переходов между состояниями покоя и
        /// ходьбы. Идемпотентно: повторный запуск ничего не ломает.
        /// </summary>
        public static void SetupLocomotion(string controllerPath, string idleStateName, string walkStateName)
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (controller == null)
            {
                Debug.LogError($"[CatAnimatorSetup] Контроллер не найден: {controllerPath}");
                return;
            }

            EnsureFloatParameter(controller, CatAnimationController.SpeedParameter);

            if (controller.layers.Length == 0)
            {
                Debug.LogError($"[CatAnimatorSetup] В контроллере нет слоёв: {controllerPath}");
                return;
            }

            var stateMachine = controller.layers[0].stateMachine;
            var idle = FindState(stateMachine, idleStateName);
            var walk = FindState(stateMachine, walkStateName);
            if (idle == null || walk == null)
            {
                Debug.LogError($"[CatAnimatorSetup] Не найдены состояния " +
                               $"'{idleStateName}' и/или '{walkStateName}' в {controllerPath}");
                return;
            }

            // Скорость не равна нулю → идём в Walk.
            ConfigureTransition(idle, walk, AnimatorConditionMode.Greater);
            // Скорость равна нулю → возвращаемся в Idle.
            ConfigureTransition(walk, idle, AnimatorConditionMode.Less);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CatAnimatorSetup] Переходы {idleStateName} ↔ {walkStateName} настроены " +
                      $"по параметру {CatAnimationController.SpeedParameter}.");
        }

        private static void EnsureFloatParameter(AnimatorController controller, string name)
        {
            foreach (var parameter in controller.parameters)
            {
                if (parameter.name != name)
                    continue;
                if (parameter.type != AnimatorControllerParameterType.Float)
                    Debug.LogWarning($"[CatAnimatorSetup] Параметр '{name}' есть, но не float.");
                return;
            }
            controller.AddParameter(name, AnimatorControllerParameterType.Float);
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
        {
            foreach (var child in stateMachine.states)
            {
                if (child.state != null && child.state.name == name)
                    return child.state;
            }
            return null;
        }

        /// <summary>
        /// Настраивает переход from → to: переиспользует существующий (их создал
        /// дизайнер) либо создаёт новый, отключает Exit Time и задаёт единственное
        /// условие по скорости.
        /// </summary>
        private static void ConfigureTransition(AnimatorState from, AnimatorState to, AnimatorConditionMode mode)
        {
            AnimatorStateTransition transition = null;
            foreach (var existing in from.transitions)
            {
                if (existing.destinationState == to)
                {
                    transition = existing;
                    break;
                }
            }

            if (transition == null)
                transition = from.AddTransition(to);

            // Без Exit Time переход срабатывает сразу по условию, а не по концу клипа.
            transition.hasExitTime = false;
            transition.hasFixedDuration = true;
            transition.duration = 0.05f; // короткий бленд, чтобы смена читалась мгновенно

            transition.conditions = new AnimatorCondition[0];
            transition.AddCondition(mode, CatAnimationController.SpeedThreshold,
                CatAnimationController.SpeedParameter);
        }
    }
}
