function Player::calculateAcceleration(%this, %test) {
	if(%this.isStaticFX || %this.getState() $= "Dead") {
		return;
	}
	
	%timeCoefficient = 1000 / (getSimTime() - %this.lastVelocityRead);
	%this.acceleration = vectorScale(vectorSub(%this.getVelocity(), %this.lastVelocity), %timeCoefficient);
	%this.jerk = vectorScale(vectorSub(%this.acceleration, %this.lastAcceleration), %timeCoefficient);
	%this.jounce = vectorScale(vectorSub(%this.jerk, %this.lastJerk), %timeCoefficient);

	%this.lastVelocityRead = getSimTime();
	%this.lastVelocity = %this.getVelocity();
	%this.lastAcceleration = %this.acceleration;
	%this.lastJerk = %this.jerk;
	%this.accelerationTick++;

	%this.calculateAccelerationSchedule = %this.schedule(33, calculateAcceleration, %this.accelerationTick);
}

function Player::getPredictedPosition(%this, %time) {
	%linear = vectorScale(%this.getVelocity(), %time);
	%quadratic = vectorScale(%this.acceleration, mPow(%time, 2) * 0.5);

	if(vectorLen(%this.jerk) < 100) {
		%quitdratic = vectorScale(%this.jerk, mPow(%time, 3) / 6);
		%position = vectorAdd(%this.getHackPosition(), vectorAdd(%quintdratic, vectorAdd(%quadratic, %linear)));
		return %position;
	}
	else {
		%position = vectorAdd(%this.getHackPosition(), %linear);
		return %position;
	}
}

deActivatePackage(MiniDungeonsTargetPrediction);
package MiniDungeonsTargetPrediction {
	function Armor::onAdd(%this, %obj) {
		Parent::onAdd(%this, %obj);
		%obj.calculateAcceleration();
	}
};
activatePackage(MiniDungeonsTargetPrediction);