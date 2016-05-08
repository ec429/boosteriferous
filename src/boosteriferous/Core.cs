using System;
using System.Collections.Generic;
using UnityEngine;
using boosteriferous.Modules;

namespace boosteriferous
{
	public abstract class ProfileShape
	{
		public abstract string name { get; }
		public abstract void setFieldVisibility(ModuleControlledFirework mcf);
		public abstract void recalcCurve(ModuleControlledFirework mcf, out FloatCurve fc, out float timeScale);
		public bool disable = false;
		public string techRequired = null;
		public float costFactor = 0f;
		public float minThrottleFactor = 1f;
		public bool isAvailable(ModuleControlledFirework mcf)
		{
			if (disable)
				return false;
			if (!string.IsNullOrEmpty(techRequired) && ResearchAndDevelopment.GetTechnologyState(techRequired) != RDTech.State.Available)
				return false;
			return true;
		}
		private void setFieldVisibleState(ModuleControlledFirework mcf, string fieldName, bool state)
		{
			mcf.Fields[fieldName].guiActive = state;
			mcf.Fields[fieldName].guiActiveEditor = state;
		}
		public void setFieldVisible(ModuleControlledFirework mcf, string fieldName)
		{
			setFieldVisibleState(mcf, fieldName, true);
		}
		public void setFieldInvisible(ModuleControlledFirework mcf, string fieldName)
		{
			setFieldVisibleState(mcf, fieldName, false);
		}
	};

	public class ProfileFlat : ProfileShape
	{
		public override string name { get { return "Flat"; } }
		public override void setFieldVisibility(ModuleControlledFirework mcf)
		{
			setFieldInvisible(mcf, "throttleDownPoint");
			setFieldVisible(mcf, "throttleDownAmount");
		}
		public override void recalcCurve(ModuleControlledFirework mcf, out FloatCurve fc, out float timeScale)
		{
			float tda = mcf.throttleDownAmount / 100f;
			Debug.LogFormat("[bfer] Recalculating thrust curve: flat, tda = {0:F3}", tda);
			timeScale = tda > 0f ? 1f / tda : 0f;
			fc = new FloatCurve();
			// Curve is backwards, because that's how thrustCurve works
			fc.Add(0f, 1f, 0f, 0f);
			fc.Add(1f, 1f, 0f, 0f);
		}
	}

	public class ProfileStep : ProfileShape
	{
		public override string name { get { return "Step"; } }
		public override void setFieldVisibility(ModuleControlledFirework mcf)
		{
			setFieldVisible(mcf, "throttleDownPoint");
			setFieldVisible(mcf, "throttleDownAmount");
		}
		public override void recalcCurve(ModuleControlledFirework mcf, out FloatCurve fc, out float timeScale)
		{
			float tdp = mcf.throttleDownPoint / 100f, tda = mcf.throttleDownAmount / 100f;
			Debug.LogFormat("[bfer] Recalculating thrust curve: step, tdp = {0:F3}, tda = {1:F3}", tdp, tda);
			// Have to multiply curve points by this to scale maxThrust (almost) correctly
			timeScale = (1f - tdp) + (tda > 0f ? tdp / tda : 0f);
			fc = new FloatCurve();
			// Curve is backwards, because that's how thrustCurve works
			fc.Add(0f, tda * timeScale, 0f, 0f);
			fc.Add(tdp - mcf.rampWidth, tda * timeScale, 0f, 0f);
			fc.Add(tdp + mcf.rampWidth, timeScale, 0f, 0f);
			fc.Add(1f, timeScale, 0f, 0f);
		}
	}

	// Default profile for 'realistic' SRBs: thrust increases over duration of burn
	public class ProfileProgressive : ProfileShape
	{
		public override string name { get { return "Progressive"; } }
		public override void setFieldVisibility(ModuleControlledFirework mcf)
		{
			setFieldInvisible(mcf, "throttleDownPoint");
			setFieldInvisible(mcf, "throttleDownAmount");
		}
		public override void recalcCurve(ModuleControlledFirework mcf, out FloatCurve fc, out float timeScale)
		{
			// T = 1 - 0.2P
			// t = integ[1/(1-0.2P) dP] = (1/0.2) ln (1/(1-0.2P))
			// so burn time = 5 ln (1.25)
			Debug.LogFormat("[bfer] Recalculating thrust curve: progressive");
			// Have to multiply curve points by this to scale maxThrust (almost) correctly
			timeScale = (float)(5f * Math.Log(1.25));
			fc = new FloatCurve();
			fc.Add(0f, timeScale, 0f, -0.2f);
			fc.Add(1f, timeScale * 0.8f, -0.2f, 0f);
		}
	}
	
	[KSPAddon(KSPAddon.Startup.Instantly, false)]
	public class Core : MonoBehaviour
	{
		public static Core Instance { get; protected set; }
		public Core()
		{
			if (Core.Instance != null)
			{
				Destroy(this);
				return;
			}

			Core.Instance = this;
			profiles = new Dictionary<string, ProfileShape>();
			addProfile(new ProfileFlat());
			addProfile(new ProfileStep());
			addProfile(new ProfileProgressive());
		}

		public string defaultProfile = "Flat";

		public Dictionary<string, ProfileShape> profiles;
		private void addProfile(ProfileShape ps)
		{
			profiles.Add(ps.name, ps);
		}

		protected void Start()
		{
			foreach (ConfigNode cn in GameDatabase.Instance.GetConfigNodes("BoosteriferousProfileShape"))
			{
				if (!cn.HasValue("name"))
				{
					Debug.Log("[bfer] No name in a BoosteriferousProfileShape node");
					continue;
				}
				string name = cn.GetValue("name");
				if (!profiles.ContainsKey(name))
				{
					Debug.LogErrorFormat("[bfer] No such BoosteriferousProfileShape '{0}'", name);
					continue;
				}
				ProfileShape ps = profiles[name];
				if (cn.HasValue("techRequired"))
				{
					ps.techRequired = cn.GetValue("techRequired");
				}
				bool isDefault = false;
				if (cn.TryGetValue("isDefault", ref isDefault) && isDefault)
					defaultProfile = name;
				if (cn.HasValue("costFactor"))
				{
					if (!float.TryParse(cn.GetValue("costFactor"), out ps.costFactor))
						Debug.LogWarningFormat("[bfer] Malformed costFactor '{0}' in BoosteriferousProfileShape '{1}'", cn.GetValue("costFactor"), name);
				}
				if (cn.HasValue("minThrottleFactor"))
				{
					if (!float.TryParse(cn.GetValue("minThrottleFactor"), out ps.minThrottleFactor))
						Debug.LogWarningFormat("[bfer] Malformed minThrottleFactor '{0}' in BoosteriferousProfileShape '{1}'", cn.GetValue("minThrottleFactor"), name);
				}
				cn.TryGetValue("disable", ref ps.disable);
			}
		}
	}
}