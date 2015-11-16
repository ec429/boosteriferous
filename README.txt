Boosteriferous
==============

Boosteriferous is a mod for the game Kerbal Space Program by Squad.
It allows you to configure the thrust profile of solid rocket boosters, giving
 more nuanced control than the 'Thrust Limiter' slider.  However, you still
 have to work out what you want ahead of time and set it up in the VAB/SPH -
 you can't throttle these solids in real time!
Note that Boosteriferous completely overrides the 'Thrust Limiter' slider;
 that setting will have no effect on engines which support thrust profiles.

Boosteriferous is currently in alpha test.

Boosteriferous is developed by Edward Cree, who goes by the handle 'soundnfury'
 on IRC and 'ec429' elsewhere; his website is at http://jttlov.no-ip.org.

Boosteriferous is licensed under the GNU General Public License, version 2.


Thrust Profiles
---------------

A 'thrust profile' is an ordered list of pairs (<throttle>, <fraction>), where
 <throttle> is the throttle setting (expressed as a percentage of maximum
 thrust) and <fraction> is the segment length, expressed as a percentage of
 total fuel capacity.  Each segment will burn to completion at its own rate,
 then the next segment will take over.
For example, let's say you have a solid rocket that contains a total of 200
 units of SolidFuel and, at full thrust, consumes 10 units per second.  Then
 your thrust profile might be, say:
| throttle | fraction |
+----------+----------+
|   100%   |   25%    |
|    20%   |   15%    |
|    60%   |   60%    |
In the first segment, we run at full throttle.  This lasts until 50 units of
 SolidFuel have been consumed, which takes 5 seconds.
For the second segment, the thrust drops to 20%, so we are now consuming just
 2 units of fuel per second.  This segment lasts for 30 units of SolidFuel,
 which takes 15 seconds.
Finally, the thrust comes back up to 60%, which takes 6 units of SolidFuel per
 second.  The segment's 120 units of SolidFuel last for 20 seconds.

One caveat to note: the fractions are based on the part's total fuel capacity;
 if you reduce the part's SolidFuel resource in the VAB/SPH, you will
 effectively chop off the beginning of your thrust profile.


The Editor GUI
--------------

To edit the thrust profile of a solid rocket in the VAB/SPH, click 'Thrust
 Profile' in its right-click menu.  This will bring up a window.
At the top of this window are two buttons "Reset" and "Add".  "Reset" will
 clear the current profile and replace it with (100, 100), i.e. full throttle
 throughout the burn.  "Add" will append a segment to the existing profile.
 Note that each type of solid rocket has a maximum number of segments; note
 also that each segment after the first increases the part's cost by 15%.
Following these buttons are a series of rows, one per segment, each giving
 that segment's throttle setting and size fraction.  The throttle setting can
 be changed with the arrow buttons on either side; note that there is a fixed
 list of allowed settings associated with the part, corresponding to the
 different burn rates the manufacturer can achieve with various design tricks.
The size fraction, again, can be set with the arrow buttons, in 5% steps.  The
 last segment in the list will always be automatically sized to make the total
 up to 100%.  If the total of sizes exceeds 100%, those segments which run off
 the end will be highlighted in red.  Meanwhile, any segments which are
 entirely before the starting point (which can occur if the part is only
 partially filled with SolidFuel) will be highlighted in yellow.
