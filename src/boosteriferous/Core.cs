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
		public string techRequired = null;
		public bool isAvailable(ModuleControlledFirework mcf)
		{
			if (!string.IsNullOrEmpty(techRequired) && ResearchAndDevelopment.GetTechnologyState(techRequired) != RDTech.State.Available)
				return false;
			return true;
		}
    	public void setFieldVisible(ModuleControlledFirework mcf, string fieldName)
    	{
    		mcf.Fields[fieldName].guiActive = true;
    		mcf.Fields[fieldName].guiActiveEditor = true;
    	}
    };

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
			Debug.Log(String.Format("[bfer] Recalculating thrust curve: tdp = {0:F3}, tda = {1:F3}", tdp, tda));
			// Have to multiply curve points by this to scale maxThrust (almost) correctly
			timeScale = (1f - tdp) + (tda > 0f ? tdp / tda : 0f);
			fc = new FloatCurve();
			// Curve is backwards, because that's how thrustCurve works
			fc.Add(0f, tda * timeScale, 0f, 0f);
			fc.Add(1f - tdp - mcf.rampWidth, tda * timeScale, 0f, 0f);
			fc.Add(1f - tdp + mcf.rampWidth, timeScale, 0f, 0f);
			fc.Add(1f, timeScale, 0f, 0f);
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
            addProfile(new ProfileStep());
        }

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
					Debug.Log("No name in a BoosteriferousProfileShape node");
                    continue;
                }
                string name = cn.GetValue("name");
                if (!profiles.ContainsKey(name))
                {
					Debug.Log(String.Format("No such BoosteriferousProfileShape '{0}'", name));
                    continue;
                }
				ProfileShape ps = profiles[name];
                if (cn.HasValue("techRequired"))
                {
                    ps.techRequired = cn.GetValue("techRequired");
                }
            }
		}
	}
}

