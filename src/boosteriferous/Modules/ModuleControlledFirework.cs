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
				m.Flameout("Thrust terminated");
		}
		public override string GetInfo()
        {
            return "Engine has Thrust Termination Ports";
        }
	}

	public class ModuleControlledFirework : PartModule
	{
		[KSPField()]
		public float minThrottle = 0f;
		[KSPField()]
		public float rampWidth = 0.01f;
		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Throttle-down point"),
		 UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f, affectSymCounterparts = UI_Scene.All, scene = UI_Scene.Editor)]
		public float throttleDownPoint = 100.0f;
		[KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "Throttle-down amount"),
		 UI_FloatRange(minValue = 0f, maxValue = 100f, stepIncrement = 1f, affectSymCounterparts = UI_Scene.All, scene = UI_Scene.Editor)]
		public float throttleDownAmount = 50.0f;
		[KSPField()]
		public float maxThrust; // Must be the original maxThrust of this part's ModuleEngines[FX]

		private void recalcThrustCurve(BaseField f, object o)
		{
			float tdp = 1f - throttleDownPoint / 100f, tda = throttleDownAmount / 100f;
			Debug.Log(String.Format("[bfer] Recalculating thrust curve: tdp = {0:F3}, tda = {1:F3}", tdp, tda));
			// Have to multiply curve points by this to scale maxThrust (almost) correctly
			float timeScale = (1f - tdp) + (tda > 0f ? tdp / tda : 0f);
			FloatCurve fc = new FloatCurve();
			// Curve is backwards, because that's how thrustCurve works
			fc.Add(0f, tda * timeScale, 0f, 0f);
			fc.Add(1f - tdp - rampWidth, tda * timeScale, 0f, 0f);
			fc.Add(1f - tdp + rampWidth, timeScale, 0f, 0f);
			fc.Add(1f, timeScale, 0f, 0f);
			Debug.Log(String.Format("[bfer] timeScale = {0:F3}, maxThrust = {1:F3}", timeScale, maxThrust / timeScale));
			// Apply to the engine
			foreach (ModuleEngines m in part.FindModulesImplementing<ModuleEngines>())
			{
				m.thrustCurve = fc;
				m.useThrustCurve = true;
				m.maxThrust = maxThrust / timeScale;
				// Have to update maxFuelFlow as well
				m.maxFuelFlow = m.maxThrust / (m.atmosphereCurve.Evaluate(0f) * m.g);
				Debug.Log(String.Format("[bfer] Applied; maxFuelFlow = {0:F3}", m.maxFuelFlow));
			}
		}

		public override void OnAwake()
		{
			UI_FloatRange tdaRange = (UI_FloatRange)this.Fields["throttleDownAmount"].uiControlEditor;
			tdaRange.minValue = (float)minThrottle;
			tdaRange.onFieldChanged = recalcThrustCurve;
			UI_FloatRange tdpRange = (UI_FloatRange)this.Fields["throttleDownPoint"].uiControlEditor;
			tdpRange.onFieldChanged = recalcThrustCurve;
			foreach (ModuleEngines m in part.FindModulesImplementing<ModuleEngines>())
				m.Fields["thrustPercentage"].guiActiveEditor = false;
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
