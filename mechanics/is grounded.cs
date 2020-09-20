deActivatePackage(MiniDungeonsIsGrounded);
package MiniDungeonsIsGrounded {
	function Armor::onAdd(%this, %obj) {
		Parent::onAdd(%this, %obj);
		%obj.isGroundedLoop();
	}

	function Armor::onCollision(%this, %obj, %col, %vec, %speed) {
		Parent::onCollision(%this, %obj, %col, %vec, %speed);

		if(isObject(%col) && isFunction(%col.getClassName(), "getState")) {
			if(%obj.getState() !$= "Dead" && %col.getState() !$= "Dead") {
				%obj.isGrounded = true;
			}
		}
	}
};
activatePackage(MiniDungeonsIsGrounded);

function Player::isGroundedLoop(%this) {
	if(%this.isStaticFX || %this.getState() $= "Dead") {
		return;
	}

	%this.isGrounded = !containerBoxEmpty($TypeMasks::fxBrickObjectType | $TypeMasks::StaticObjectType, %this.getPosition(), 0.6, 0.6, 0.8);
	%this.isGroundedSchedule = %this.schedule(100, isGroundedLoop);
}