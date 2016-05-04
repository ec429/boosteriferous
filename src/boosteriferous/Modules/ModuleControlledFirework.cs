using System;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

namespace boosteriferous.Modules
{
	public class ModuleThrustTermination : PartModule
	{
		[KSPField(isPersistant=true)]
		public bool thrustTerminated = false;
		[KSPAction("Terminate Thrust", KSPActionGroup.Abort)]
		public void terminateThrust(KSPActionParam param)
		{
			if (param.type == KSPActionType.Activate)
				Events["terminateThrust"].Invoke();
		}
		[KSPEvent(name = "terminateThrust", guiName = "Terminate Thrust", guiActive = true, guiActiveEditor = false)]
		public void terminateThrust()
		{
			foreach (ModuleEngines m in part.FindModulesImplementing<ModuleEngines>())
			{
				m.allowRestart = false;
				m.allowShutdown = true;
				m.Flameout("Thrust terminated");
				m.Shutdown();
			}
		}
		public override string GetInfo()
        {
            return "Engine has Thrust Termination Ports";
        }
	}

	public class ModuleControlledFirework : PartModule, IPartCostModifier
	{
		[KSPField()]
		public float minThrottle = 0f;
		[KSPField()]
		public float rampWidth = 0.01f;
		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Throttle-down point"),
		 UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f, affectSymCounterparts = UI_Scene.All, scene = UI_Scene.Editor)]
		public float throttleDownPoint = 50f;
		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Throttle-down amount"),
		 UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f, affectSymCounterparts = UI_Scene.All, scene = UI_Scene.Editor)]
		public float throttleDownAmount = 100f;
		[KSPField()]
		public float maxThrust; // Must be the original maxThrust of this part's ModuleEngines[FX]
		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Profile Type"),
		 UI_ChooseOption(scene = UI_Scene.Editor, affectSymCounterparts = UI_Scene.All)]
        public string profileTypeName = Core.Instance.defaultProfile;

        private ProfileShape getProfileType()
        {
			ProfileShape ps;
			if (profileTypeName == null)
			{
				Debug.Log("[bfer] profileTypeName is null");
				return null;
			}
			if (!Core.Instance.profiles.TryGetValue(profileTypeName, out ps))
			{
				Debug.LogFormat("[bfer] No such ProfileShape '{0}'", profileTypeName);
				return null;
			}
			return ps;
        }

		private void recalcThrustCurve(BaseField f, object o)
		{
			ProfileShape ps = getProfileType();
			ps.setFieldVisibility(this);
			FloatCurve fc;
			float timeScale;
			ps.recalcCurve(this, out fc, out timeScale);
			Debug.LogFormat("[bfer] timeScale = {0:F3}, maxThrust = {1:F3}", timeScale, maxThrust / timeScale);
			// Apply to the engine
			foreach (ModuleEngines m in part.FindModulesImplementing<ModuleEngines>())
			{
				m.thrustCurve = fc;
				m.useThrustCurve = true;
				m.maxThrust = maxThrust / timeScale;
				// Have to update maxFuelFlow as well
				m.maxFuelFlow = m.maxThrust / (m.atmosphereCurve.Evaluate(0f) * m.g);
				Debug.LogFormat("[bfer] Applied; maxFuelFlow = {0:F3}", m.maxFuelFlow);
			}
		}

		#region IPartCostModifier implementation

		public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
		{
			ProfileShape ps = getProfileType();
			if (ps == null)
				return 0f;
			return defaultCost * ps.costFactor;
		}

		public ModifierChangeWhen GetModuleCostChangeWhen()
		{
			return ModifierChangeWhen.FIXED;
		}

		#endregion

		public override void OnAwake()
		{
			UI_FloatRange tdaRange = (UI_FloatRange)this.Fields["throttleDownAmount"].uiControlEditor;
			tdaRange.minValue = (float)minThrottle;
			tdaRange.onFieldChanged = recalcThrustCurve;
			UI_FloatRange tdpRange = (UI_FloatRange)this.Fields["throttleDownPoint"].uiControlEditor;
			tdpRange.onFieldChanged = recalcThrustCurve;
			UI_ChooseOption ptnChoose = (UI_ChooseOption)this.Fields["profileTypeName"].uiControlEditor;
			ptnChoose.onFieldChanged = recalcThrustCurve;
			foreach (ModuleEngines m in part.FindModulesImplementing<ModuleEngines>())
				m.Fields["thrustPercentage"].guiActiveEditor = false;
			BaseField field = Fields["profileTypeName"];
			Dictionary<string, ProfileShape> availableProfiles = new Dictionary<string, ProfileShape>();
			foreach (ProfileShape ps in Core.Instance.profiles.Values)
			{
				if (ps.isAvailable(this))
				{
					availableProfiles.Add(ps.name, ps);
				}
			}
			switch (availableProfiles.Count)
            {
                case 0: // If nothing appears available, just use the default
                    profileTypeName = Core.Instance.defaultProfile;
					field.guiActiveEditor = false;
                    Debug.LogFormat("[bfer] Defaulting profileTypeName = {0}", profileTypeName);
                    break;
                case 1:
					profileTypeName = new List<string>(availableProfiles.Keys)[0];
                    field.guiActiveEditor = false;
                    Debug.LogFormat("[bfer] Forcing profileTypeName = {0}", profileTypeName);
                    break;
                default:
                    field.guiActiveEditor = true;
                    UI_ChooseOption range = (UI_ChooseOption)field.uiControlEditor;
                    range.options = new List<string>(availableProfiles.Keys).ToArray();
                    break;
            }
			recalcThrustCurve(null, null);
		}

		public override void OnLoad(ConfigNode node)
		{
			base.OnLoad(node);
			this.OnAwake();
		}

		public override string GetInfo()
        {
            return "Adjustable Thrust Profile";
		}
	}
}
