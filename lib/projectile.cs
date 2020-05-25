// changed the upper limit of damage
function ProjectileData::damage(%this, %obj, %col, %fade, %pos, %normal) {
	if(%this.directDamage <= 0) {
		return;
	}

	%damageType = $DamageType::Direct;
	if(%this.DirectDamageType)
	%damageType = %this.DirectDamageType;

	%scale = getWord(%obj.getScale(), 2);
	%directDamage = mClampF(%this.directDamage, -10000, 10000) * %scale;

	if(%col.getType() & $TypeMasks::PlayerObjectType) {
		%col.damage(%obj, %pos, %directDamage, %damageType);
	}
	else {
		%col.damage(%obj, %pos, %directDamage, %damageType);
	}	
}

function calculateParabolicTrajectory(%pos, %target, %vz, %gm) {
	%g = 9.84 * %gm;

	%xi = getWord(%pos, 0);
	%yi = getWord(%pos, 1);
	%zi = getWord(%pos, 2);
	%xf = getWord(%target, 0);
	%yf = getWord(%target, 1);
	%zf = getWord(%target, 2);
	
	%solution = mSolveQuadratic(-0.5 * %g, %vz, %zi - %zf);
	%t = -(getMin(getWord(%solution, 1), getWord(%solution, 2)));
	
	%vx = (%xf - %xi) / %t;
	%vy = (%yf - %yi) / %t;
	
	return %vx SPC %vy SPC %vz;
}

function calculateProjectileZAngle(%position, %targetPosition, %speed, %gravityFactor) {
	%distance = vectorDist(getWords(%position, 0, 1), getWords(%targetPosition, 0, 1));
	%height = getWord(%position, 2) - getWord(%targetPosition, 2);
	%gravity = -9.84 * %gravityFactor;
	// https://bansheerubber.com/i/f/uBI3D.png, from wikipedia
	return $pi - mATan(mPow(%speed, 2) - mSqrt(mPow(%speed, 4) - %gravity * (%gravity * mPow(%distance, 2) + 2 * %height * mPow(%speed, 2))), %gravity * %distance);
}

function calculateProjectileFlightTime(%position, %targetPosition, %speed, %gravityFactor) {
	%angle = calculateProjectileZAngle(%position, %targetPosition, %speed, %gravityFactor);
	return vectorDist(getWords(%position, 0, 1), getWords(%targetPosition, 0, 1)) / (%speed * mCos(%angle));
}

function calculateProjectileZAngle2(%position, %targetPosition, %speed, %gravityFactor) {
	%distance = vectorDist(getWords(%position, 0, 1), getWords(%targetPosition, 0, 1));
	%height = getWord(%position, 2) - getWord(%targetPosition, 2);
	%gravity = -9.84 * %gravityFactor;
	// https://bansheerubber.com/i/f/uBI3D.png, from wikipedia

	return $pi - mATan(mPow(%speed, 2) + mSqrt(mPow(%speed, 4) - %gravity * (%gravity * mPow(%distance, 2) + 2 * %height * mPow(%speed, 2))), %gravity * %distance);
}

function calculateProjectileFlightTime2(%position, %targetPosition, %speed, %gravityFactor) {
	%angle = calculateProjectileZAngle2(%position, %targetPosition, %speed, %gravityFactor);
	return vectorDist(getWords(%position, 0, 1), getWords(%targetPosition, 0, 1)) / (%speed * mCos(%angle));
}

// idk how an equation can be so broken. idk why this works. it just does. it gives the min speed within a decent amount of accuracy
function calculateMinProjectileSpeed(%position, %targetPosition, %gravityFactor) {
	%distance = vectorDist(%position, %targetPosition);
	%height = getWord(%position, 2) - getWord(%targetPosition, 2);
	%gravity = 9.84 * %gravityFactor;
	return mSqrt((%distance * %distance * %gravity) / (%distance - %height));
}

function calculateVelocityFromTime(%position, %targetPosition, %time, %gravityFactor) {
	%x = (getWord(%targetPosition, 0) - getWord(%position, 0)) / %time;
	%y = (getWord(%targetPosition, 1) - getWord(%position, 1)) / %time;
	%z = (getWord(%targetPosition, 2) - getWord(%position, 2)) / %time + 9.8 * %gravityFactor * %time / 2;
	return %x SPC %y SPC %z;
}

// returns a random spread vector based on degrees input
function getRandomSpread(%vector, %radius) {
	%right = vectorNormalize(vectorCross(%vector, "0 0 1"));
	%up = vectorCross(%vector, %right);

	%r = %radius * mSqrt(getRandom());
	%theta = getRandom() * 2 * $PI;

	return vectorNormalize(vectorAdd(%vector, vectorAdd(vectorScale(%right, %r * mCos(%theta)), vectorScale(%up, %r * mSin(%theta)))));
}