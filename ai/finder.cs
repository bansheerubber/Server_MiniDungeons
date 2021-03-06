// set of all possible targets we can select from
if(!isObject(MiniDugneonsTargetSet)) {
	new SimSet(MiniDugneonsTargetSet);
}

function AiPlayer::hasValidTarget(%this) {
	return isObject(%this.target) && %this.target.getState() !$= "Dead" && %this.target.getCurrentRoom() == %this.room;
}

// find a potential target
function AiPlayer::findTarget(%this) {
	%position = %this.getPosition();
	%eyePoint = %this.getEyePoint();
	
	%count = MiniDugneonsTargetSet.getCount();
	%minDist = %this.idleDrawDistance;
	for(%i = 0; %i < %count; %i++) {
		%target = MiniDugneonsTargetSet.getObject(%i);
		%targetPosition = %target.getHackPosition();

		// stagger out if statements in order of expensiveness
		if(
			%this.room == %target.getCurrentRoom()
			&& (%dist = vectorDist(%position, %targetPosition)) < %this.idleDrawDistance && %dist < %minDist
		) {
			// if we are in alert phase, do not do the raycast
			// %raycast = %this.roomSet.alert ? 0 : containerRaycast(%eyePoint, %targetPosition, $TypeMasks::StaticObjectType | $TypeMasks::fxBrickObjectType, false);
			
			// determine if target is in fov
			%theta = mRadToDeg(mACos(vectorDot(%this.getForwardVector(), vectorNormalize(vectorSub(%targetPosition, %position)))));
			if(!isObject(%raycast) && (%dist < %this.idleThirdSenseDistance || %theta < %this.idleFOV)) {
				// we've found a target if they're within our draw distance and are the closest target found so far
				%minDist = %dist;
				%this.target = %target;
			}
		}
	}

	%this.target.numAttackers++;
	return isObject(%this.target);
}

function AiPlayer::loseTarget(%this) {
	%this.target.numAttackers--;
	%this.target = 0;
}

deActivatePackage(MiniDungeonsFinder);
package MiniDungeonsFinder {
	function GameConnection::spawnPlayer(%this) {
		Parent::spawnPlayer(%this);

		if(isObject(%this.player)) {
			MiniDugneonsTargetSet.add(%this.player);
		}
	}

	function Armor::onDisabled(%this, %obj) {
		if(MiniDugneonsTargetSet.isMember(%obj)) {
			MiniDugneonsTargetSet.remove(%obj);	
		}
		Parent::onDisabled(%this, %obj);
	}
};
activatePackage(MiniDungeonsFinder);