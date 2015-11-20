using System;
using System.Collections.Generic;
using UnityEngine;

namespace boosteriferous.UI
{
    public class ProfileEditorWindow : AbstractWindow
    {
		private GUIStyle mRowKeyStyle, mKeyStyle, mThrottleStyle, mSizeStyle, mRedSizeStyle, mYellowSizeStyle, mBtnStyle, mTinyBtnStyle, mGreyBtnStyle;
		private boosteriferous.Modules.ModuleControlledFirework mcf;
		public ProfileEditorWindow (boosteriferous.Modules.ModuleControlledFirework mcf)
            : base(Guid.NewGuid(), String.Format("Thrust Profile"), new Rect(200, 100, 300, 400), WindowAlign.Floating)
        {
			this.mcf = mcf;
            mRowKeyStyle = new GUIStyle(HighLogic.Skin.label)
            {
            	fixedWidth = 80,
                fontStyle = FontStyle.Bold,
                fontSize = 11,
			};
            mKeyStyle = new GUIStyle(HighLogic.Skin.label)
            {
            	alignment = TextAnchor.MiddleRight,
            	fixedWidth = 40,
                fontStyle = FontStyle.Bold,
                fontSize = 11,
            };
            mThrottleStyle = new GUIStyle(HighLogic.Skin.label)
            {
				fixedWidth = 32,
            	fontSize = 11,
			};
            mSizeStyle = new GUIStyle(HighLogic.Skin.label)
            {
				fixedWidth = 96,
            	fontSize = 11,
			};
            mRedSizeStyle = new GUIStyle(mSizeStyle)
            {
                normal = new GUIStyleState() { textColor = Color.red, },
			};
            mYellowSizeStyle = new GUIStyle(mSizeStyle)
            {
                normal = new GUIStyleState() { textColor = Color.yellow, },
			};
            mBtnStyle = new GUIStyle(HighLogic.Skin.button)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 12,
			};
            mTinyBtnStyle = new GUIStyle(HighLogic.Skin.button)
            {
                fontSize = 7,
                fixedWidth = 16,
                padding = new RectOffset(2, 2, 6, 6),
                margin = new RectOffset(),
				border = new RectOffset(1, 1, 1, 1),
            };
            mGreyBtnStyle = new GUIStyle(mBtnStyle)
            {
                fontStyle = FontStyle.Italic,
            };
        }

        public override void Window(int id)
        {
			if (!HighLogic.LoadedSceneIsEditor)
			{
				base.Hide();
			}
			else if (this.mcf != null)
        	{
        		int nSegments = mcf.segSettings.Count;
	            GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Reset", mBtnStyle))
				{
					mcf.resetSettings();
				}
				if (GUILayout.Button("Add", nSegments < mcf.maxSegments ? mBtnStyle : mGreyBtnStyle) && nSegments < mcf.maxSegments)
				{
					mcf.segSettings.Add(1.0);
					mcf.segFractions.Add(0.05);
					mcf.OnTweak();
				}
				GUILayout.EndHorizontal();
	            int segIndex = 0;
	            double accumFrac = 0.0;
				double solidFuel, maxSolidFuel;
				mcf.getSolidFuel(out solidFuel, out maxSolidFuel);
				double flowRate = mcf.nominalFlowRate;
				int toRemove = -1;
				foreach (KeyValuePair<double, double> segment in mcf.segments())
	            {
					GUILayout.BeginHorizontal();
					GUILayout.Label(String.Format("Segment {0}", segIndex), mRowKeyStyle);
					GUILayout.Label("thrust:", mKeyStyle);
					double thrust = segment.Key;
					int tIndex = mcf.indexOfOption(thrust);
					GUILayout.BeginHorizontal(GUILayout.Width(60));
					if (GUILayout.Button("<", mTinyBtnStyle))
					{
						mcf.segSettings[segIndex] = mcf.segOptions[Math.Min(tIndex + 1, mcf.segOptions.Count -1)];
						mcf.OnTweak();
					}
					GUILayout.Label(String.Format("{0:F0}%", thrust * 100.0), mThrottleStyle);
					if (GUILayout.Button(">", mTinyBtnStyle))
					{
						mcf.segSettings[segIndex] = mcf.segOptions[Math.Max(tIndex - 1, 0)];
						mcf.OnTweak();
					}
					GUILayout.EndHorizontal();
					double fraction = segment.Value;
					GUIStyle vstyle = mSizeStyle;
					if (fraction < 0)
						fraction = Math.Max(Math.Round(1.0 - accumFrac, 2), 0.0);
					accumFrac += fraction;
					if (accumFrac <= 1.0 - (solidFuel / maxSolidFuel))
						vstyle = mYellowSizeStyle; /* segment entirely before start */
					if (accumFrac > 1.0) /* segment runs past end */
						vstyle = mRedSizeStyle;
					double duration = maxSolidFuel * fraction / (flowRate * thrust);
					GUILayout.Label("size:", mKeyStyle);
					bool moreSegs = mcf.segFractions.Count > segIndex;
					GUILayout.BeginHorizontal(GUILayout.Width(112));
					if (GUILayout.Button("<", mTinyBtnStyle) && moreSegs)
					{
						mcf.segFractions[segIndex] = Math.Round(Math.Max(fraction - 0.05, 0.05), 2);
						mcf.OnTweak();
					}
					GUILayout.Label(String.Format("{0}% ({1:F1}s)", fraction * 100.0, duration), vstyle);
					if (GUILayout.Button(">", mTinyBtnStyle) && moreSegs)
					{
						mcf.segFractions[segIndex] = Math.Round(Math.Min(fraction + 0.05, 1.0), 2);
						mcf.OnTweak();
					}
					GUILayout.EndHorizontal();
					if (GUILayout.Button("x", mTinyBtnStyle))
					{
						toRemove = segIndex;
					}
	            	GUILayout.EndHorizontal();
	            	segIndex++;
	            }
	            GUILayout.EndVertical();
				if (toRemove >= 0)
				{
					if (nSegments > 1)
					{
						mcf.segSettings.RemoveAt(toRemove);
						mcf.segFractions.RemoveAt(Math.Min(toRemove, mcf.segFractions.Count - 1));
						mcf.OnTweak();
					}
					else
					{
						mcf.resetSettings();
					}
				}
            }
            else
			{
				GUILayout.BeginHorizontal(GUILayout.Width(120));
				GUILayout.Label("I have no Module!", mRowKeyStyle);
				GUILayout.EndHorizontal();
            	base.Hide();
            }
            base.Window(id);
        }
    }
}