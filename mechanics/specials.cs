deActivatePackage(MiniDungeonsSpecials);
package MiniDungeonsSpecials {
	function Armor::onTrigger(%this, %obj, %slot, %val) {
		// make sure we aren't trying to parry
		if(
			%slot == 4
			&& %val == 1
			&& getSimTime() > %obj.parryDuration
			&& isObject(%sword = %obj.sword[0])
			&& %sword.getDatablock().specialMethod !$= ""
			&& getSimTime() > %sword.nextSpecial
			&& %sword.getDatablock().call(%sword.getDatablock().specialConditionalMethod, %obj, 0)
		) {
			%sword.nextSpecial = getSimTime() + %sword.getDatablock().specialCooldown + %sword.getDatablock().specialDuration;
			%sword.getDatablock().call(%sword.getDatablock().specialMethod, %obj, 0);
		}
		else {
			Parent::onTrigger(%this, %obj, %slot,  %val);
		}
	}
};
activatePackage(MiniDungeonsSpecials);