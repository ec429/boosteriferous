using System;
using System.Collections.Generic;
using UnityEngine;

namespace boosteriferous.Modules
{
	public class ModuleControlledFirework : ModuleEngines, IPartCostModifier
	{
		[KSPField()]
		public int maxSegments;
		public List<double> segOptions;
		public List<double> segSettings;
		public List<double> segFractions;

		private boosteriferous.UI.AbstractWindow mProfileWindow;

		[KSPEvent(name = "EventEdit", guiName = "Thrust Profile", guiActive = true, guiActiveEditor = true)]
		public void EventEdit()
        {
			if (mProfileWindow == null)
			{
				if (HighLogic.LoadedSceneIsEditor)
				{
					mProfileWindow = new boosteriferous.UI.ProfileEditorWindow(this);
                }
                else
                {
                	mProfileWindow = new boosteriferous.UI.ProfileDisplayWindow(this);
                }
            }
            mProfileWindow.Show();
        }
		public float GetModuleCost(float defaultCost)
		{
			// Add 15% of base part cost per segment
			return part.partInfo.cost * 0.15f * (segSettings.Count - 1);
		}
		private void listLoad(ConfigNode node, List<double> into, string what)
		{
			foreach (string val in node.GetValues("val"))
			{
				double d;
				if (Double.TryParse(val, out d))
					into.Add(d);
				else
					Logging.Log(String.Format("Bad {0} {1}", what, val));
			}
		}
		public override void OnLoad (ConfigNode node)
		{
			segOptions = new List<double>();
			if (node.HasNode("segOptions"))
			{
				listLoad(node.GetNode("segOptions"), segOptions, "segOption");
			}
			else
			{
				segOptions.Add(1.0);
			}
			segSettings = new List<double>();
			if (node.HasNode("segSettings"))
			{
				listLoad(node.GetNode("segSettings"), segSettings, "segSetting");
			}
			else
			{
				segSettings.Add(1.0);
			}
			segFractions = new List<double>();
			if (node.HasNode("segFractions"))
			{
				listLoad(node.GetNode("segFractions"), segFractions, "segFraction");
			}
			base.OnLoad(node);
		}
		private void listSave(string name, List<double> l, ConfigNode into)
		{
			ConfigNode node = new ConfigNode(name);
			foreach (double d in l)
			{
				node.AddValue("val", d);
			}
			into.AddNode(node);
		}
		public override void OnSave (ConfigNode node)
		{
			listSave("segOptions", segOptions, node);
			listSave("segSettings", segSettings, node);
			listSave("segFractions", segFractions, node);
			base.OnSave(node);
		}
		public void getSolidFuel(out double amount, out double maxAmount)
		{
			amount = 0;
			maxAmount = 0;
			foreach(PartResource pr in part.Resources)
			{
				if (pr.resourceName.Equals("SolidFuel"))
				{
					amount += pr.amount;
					maxAmount += pr.maxAmount;
				}
			}
		}
		public double nominalFlowRate { get {
			foreach (Propellant p in propellants)
			{
				if (p.name.Equals("SolidFuel"))
					return getMaxFuelFlow(p);
			}
			Logging.Log("No propellants matched!");
			return 1.0;
		}}
		public List<KeyValuePair<double, double>> segments()
		{
			List<KeyValuePair<double, double>> rv = new List<KeyValuePair<double, double>>();
			int segIndex = 0;
			foreach (double setting in segSettings)
			{
				if (segIndex < segFractions.Count)
					rv.Add(new KeyValuePair<double, double>(setting, segFractions[segIndex]));
				else
					rv.Add(new KeyValuePair<double, double>(setting, -1.0));
				segIndex++;
			}
			return rv;
		}
		public int indexOfOption(double value)
		{
			if (segOptions.Contains(value))
				return segOptions.IndexOf(value);
			return -1;
		}
		public void resetSettings()
		{
			segSettings = new List<Double>();
			segSettings.Add(1.0);
			segFractions = new List<Double>();
		}
		private int lastSegIndex = -1;
		public override void OnFixedUpdate()
		{
			double solidFuel, maxSolidFuel;
			getSolidFuel(out solidFuel, out maxSolidFuel);
			if (maxSolidFuel > 1e-3)
			{
				double fraction = 1.0 - (solidFuel / maxSolidFuel);
				int segIndex = 0;
				foreach (double sF in segFractions)
				{
					if (sF > fraction) break;
					segIndex++;
					fraction -= sF;
				}
				segIndex = Math.Min(segIndex, segSettings.Count - 1);
				thrustPercentage = (float)segSettings[segIndex] * 100.0f;
				if (segIndex != lastSegIndex)
					Logging.Log(String.Format("entered segment {0}, throttle = {1}", segIndex, thrustPercentage));
				lastSegIndex = segIndex;
				UpdateThrottle();
			}
			base.OnFixedUpdate();
		}
	}
}

