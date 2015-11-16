using System;
using System.Collections.Generic;
using UnityEngine;

namespace boosteriferous.UI
{
    public class ProfileDisplayWindow : AbstractWindow
    {
		private GUIStyle mKeyStyle, mValueStyle, mRedValueStyle;
		private boosteriferous.Modules.ModuleControlledFirework mcf;
		public ProfileDisplayWindow (boosteriferous.Modules.ModuleControlledFirework mcf)
            : base(Guid.NewGuid(), String.Format("Thrust Profile"), new Rect(200, 100, 300, 300), WindowAlign.Floating)
        {
			this.mcf = mcf;
            mKeyStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fixedWidth = 80,
                fontStyle = FontStyle.Bold,
                fontSize = 11,
            };
            mValueStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fixedWidth = 48,
                fontSize = 11,
			};
            mRedValueStyle = new GUIStyle(HighLogic.Skin.label)
            {
                fixedWidth = 48,
				fontSize = 11,
                normal = new GUIStyleState() { textColor = Color.red, },
			};
        }

        public override void Window(int id)
        {
        	if (this.mcf != null)
        	{
				GUILayout.BeginVertical();
	            int segIndex = 0;
	            double accumFrac = 0.0;
				double solidFuel, maxSolidFuel;
				mcf.getSolidFuel(out solidFuel, out maxSolidFuel);
				double flowRate = mcf.nominalFlowRate;
				foreach (KeyValuePair<double, double> segment in mcf.segments())
	            {
					GUILayout.BeginHorizontal();
					GUILayout.Label(String.Format("Segment {0}", segIndex), mKeyStyle);
					GUILayout.Label(String.Format("thrust: {0:F0}%", segment.Key * 100.0), mValueStyle);
					double fraction = segment.Value;
					GUIStyle vstyle = mValueStyle;
					if (fraction < 0)
						fraction = Math.Round(Math.Max(1.0 - accumFrac, 0.0), 2);
	            	accumFrac += fraction;
					if (accumFrac > 1.0)
						vstyle = mRedValueStyle;
					double duration = maxSolidFuel * fraction / (flowRate * segment.Key);
					GUILayout.Label(String.Format("size: {0}% ({1:F1}s)", fraction * 100.0, duration), vstyle);
	            	GUILayout.EndHorizontal();
	            	segIndex++;
	            }
	            GUILayout.EndVertical();
            }
            else
            {
				GUILayout.BeginHorizontal(GUILayout.Width(120));
				GUILayout.Label("I have no Module!", mKeyStyle);
				GUILayout.EndHorizontal();
            	base.Hide();
			}
            base.Window(id);
        }
    }
}