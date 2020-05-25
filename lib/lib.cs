// and so begins the great hell file

function mATan2(%y, %x) {
	%result = mATan(%y, %x);
	if(%x > 0) {
		return %result;
	}
	else if(%x < 0 && %y >= 0) {
		return %result + $PI;
	}
	else if(%x < 0 && %y < 0) {
		return %result - $PI;
	}
	else if(%x == 0) {
		if(%y > 0) {
			return $PI / 2;
		}
		else if(%y < 0) {
			return -$PI / 2;
		}
		else {
			return 0;
		}
	}
}

function vectorLerp(%start, %end, %percent) {
	return vectorAdd(vectorScale(%start, 1 - %percent), vectorScale(%end, %percent));
}

function bezier3pt(%point1, %point2, %point3, %t) {
	return vectorLerp(vectorLerp(%point1, %point2, %t), vectorLerp(%point2, %point3, %t), %t);
}

function SimObject::unGhost(%this, %client) {
	%this.setNetFlag(6, 1);
	%this.clearScopeToClient(%client);
}

function SimObject::reGhost(%this, %client) {
	%this.setNetFlag(6, 0);
	%this.scopeToClient(%client);
}

function SimObject::unGhostAll(%this) {
	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.unGhost(ClientGroup.getObject(%i));
	}
}

function SimObject::reGhostAll(%this) {
	%count = ClientGroup.getCount();
	for(%i = 0; %i < %count; %i++) {
		%this.reGhost(ClientGroup.getObject(%i));
	}
}

function Player::unMountImageSafe(%this, %imageSlot) {
	if(%this.getMountedImage(%imageSlot)) {
		%this.unMountImage(%imageSlot);
	}
}

// returns a random spread vector based on degrees input
function getRandomSpread(%vector, %radius) {
	%right = vectorNormalize(vectorCross(%vector, "0 0 1"));
	%up = vectorCross(%vector, %right);

	%r = %radius * mSqrt(getRandom());
	%theta = getRandom() * 2 * $PI;

	return vectorNormalize(vectorAdd(%vector, vectorAdd(vectorScale(%right, %r * mCos(%theta)), vectorScale(%up, %r * mSin(%theta)))));
}

function vectorRelativeShift(%forwardVector, %shift) {
	%rightVector = vectorNormalize(vectorCross(%forwardVector, "0 0 1"));
	%upVector = vectorNormalize(vectorCross(%rightVector, %forwardVector));
	%shiftX = getWord(%shift, 0);
	%shiftY = getWord(%shift, 1);
	%shiftZ = getWord(%shift, 2);
	
	%result = "0 0 0";
	%result = vectorAdd(%result, vectorScale(%rightVector, %shiftX));
	%result = vectorAdd(%result, vectorScale(%forwardVector, %shiftY));
	%result = vectorAdd(%result, vectorScale(%upVector, %shiftZ));
	
	return %result;
}

function vectorProject(%a, %b) {
	%norm = vectorNormalize(%b);
	%component = vectorDot(%a, %norm);
	return vectorScale(%norm, %component);
}

function Player::getAimVector(%this) {
	%fvec = %this.getForwardVector();
	%fX = getWord(%fvec, 0);
	%fY = getWord(%fvec, 1);

	%evec = %this.getEyeVector();
	%eX = getWord(%evec, 0);
	%eY = getWord(%evec, 1);
	%eZ = getWord(%evec, 2);

	%eXY = mSqrt(%eX*%eX+%eY*%eY);

	return %fX*%eXY SPC %fY*%eXY SPC %eZ;
}

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

function vectorRotateVector(%vec1, %vec2) {
	return vectorRotateAxis(%vec1, vectorToAxis(%vec2));
}

function vectorRotateEuler(%vec, %euler) {
	return vectorRotateAxis(%vec, eulerToAxis(%euler));
}

function vectorRotateAxis(%vec, %axis) { //Epic function found online. Credits to Blocki <3. Id also like to thank Zeblote for finding this for me <3
	%u["x"] = getword(%axis,0);
	%u["y"] = getword(%axis,1);
	%u["z"] = getword(%axis,2);

	%angl = getword(%axis,3) * -1;
	%cos = mcos(%angl);
	%sin = msin(%angl);

	%a[1,1] = %cos + (%u["x"] * %u["x"] * (1 - %cos));
	%a[1,2] = (%u["x"] * %u["y"] * (1 - %cos)) - (%u["z"] * %sin);
	%a[1,3] = (%u["x"] * %u["z"] * (1 - %cos)) + (%u["y"] * %sin);

	%a[2,1] = (%u["y"] * %u["x"] * (1 - %cos)) + (%u["z"] * %sin);
	%a[2,2] = %cos + (%u["y"] * %u["y"] * (1 - %cos));
	%a[2,3] = (%u["y"] * %u["z"] * (1 - %cos)) - (%u["x"] * %sin);

	%a[3,1] = (%u["z"] * %u["x"] * (1 - %cos)) - (%u["y"] * %sin);
	%a[3,2] = (%u["z"] * %u["y"] * (1 - %cos)) + (%u["x"] * %sin);
	%a[3,3] = %cos + (%u["z"] * %u["z"] * (1 - %cos));

	%x = getWord(%vec, 0);
	%y = getWord(%vec, 1);
	%z = getWord(%vec, 2);

	%newx = (%a[1,1] * %x) + (%a[1,2] * %y) + (%a[1,3] * %z);
	%newy = (%a[2,1] * %x) + (%a[2,2] * %y) + (%a[2,3] * %z);
	%newz = (%a[3,1] * %x) + (%a[3,2] * %y) + (%a[3,3] * %z);

	%pos = %newx SPC %newy SPC %newz;
	return %pos;
}

function eulerToVector(%ang) {
	if(getWordCount(%ang) == 2) //Support for giving pitch and yaw, but not roll.
		%ang = getWord(%ang, 0) SPC 0 SPC getWord(%ang, 1);

	%yaw = mDegToRad(getWord(%ang, 2) * -1);
	%pitch = mDegToRad(getWord(%ang, 0));
	%x = mSin(%yaw) * mCos(%pitch) * 1;
	%y = mCos(%yaw) * mCos(%pitch);
	%z = mSin(%pitch);

	return %x SPC %y SPC %z;
}

function axisToVector(%ang) {
	return eulerToVector(axisToEuler(%ang));
}

function vectorToEuler(%vec) {
	%vec = vectorNormalize(%vec);
	%pitch = mRadToDeg(mASin(getWord(%vec, 2)));
	%yaw   = mRadToDeg(mATan(getWord(%vec, 0), getWord(%vec, 1)));
	return %pitch SPC 0 SPC %yaw * -1;
}

function vectorToAxis(%vec) {
	return eulerToAxis(vectorToEuler(%vec));
}

function eulerToAxis(%euler) {
	%euler = VectorScale(%euler,$pi / 180);
	%matrix = MatrixCreateFromEuler(%euler);
	return getWords(%matrix,3,6);
}

function axisToEuler(%axis) {
	%angleOver2 = getWord(%axis,3) * 0.5;
	%angleOver2 = -%angleOver2;
	%sinThetaOver2 = mSin(%angleOver2);
	%cosThetaOver2 = mCos(%angleOver2);
	%q0 = %cosThetaOver2;
	%q1 = getWord(%axis,0) * %sinThetaOver2;
	%q2 = getWord(%axis,1) * %sinThetaOver2;
	%q3 = getWord(%axis,2) * %sinThetaOver2;
	%q0q0 = %q0 * %q0;
	%q1q2 = %q1 * %q2;
	%q0q3 = %q0 * %q3;
	%q1q3 = %q1 * %q3;
	%q0q2 = %q0 * %q2;
	%q2q2 = %q2 * %q2;
	%q2q3 = %q2 * %q3;
	%q0q1 = %q0 * %q1;
	%q3q3 = %q3 * %q3;
	%m13 = 2.0 * (%q1q3 - %q0q2);
	%m21 = 2.0 * (%q1q2 - %q0q3);
	%m22 = 2.0 * %q0q0 - 1.0 + 2.0 * %q2q2;
	%m23 = 2.0 * (%q2q3 + %q0q1);
	%m33 = 2.0 * %q0q0 - 1.0 + 2.0 * %q3q3;
	return mRadToDeg(mAsin(%m23)) SPC mRadToDeg(mAtan(-%m13, %m33)) SPC mRadToDeg(mAtan(-%m21, %m22));
}

function vectorLerp(%init, %end, %t) {
	return vectorAdd(%init, vectorScale(vectorSub(%end, %init), %t));
}

function SimGroup::getNTObject(%this, %input_name) {
	%rtn = -1;
	
	for(%i = 0; %i < %this.NTNameCount; %i++) {
		%name = %this.NTName[%i];
		
		if(%name $= %input_name) {
			for(%x = 0; %x < %this.NTObjectCount[%name]; %x++) {
				%brick = %this.NTObject[%name, %x];
				%stop = true;
				
				if(%rtn == -1)
					%rtn = %brick;
				else
					%rtn = %rtn TAB %brick;
			}
		}
		
		if(%stop == true) {
			break;
		}
	}
	return %rtn;
}

function SimGroup::NTObjectCall(%this, %input_name, %call, %a1, %a2, %a3, %a4, %a5, %a6, %a7) {
	%objects = %this.getNTObject(%input_name);
	
	%count = getFieldCount(%objects);
	
	for(%i = 0; %i < %count; %i++) {
		%obj = getField(%objects, %i);
		%obj.schedule(%i * 5, call, %call, %a1, %a2, %a3, %a4, %a5, %a6, %a7);
	}
}

// gets arbitrary field without eval
function SimObject::getField(%this, %field) {
	%first = getSubStr(%field, 0, 1);
	%ending = getSubStr(%field, 1, strLen(%field));
	switch$(strLwr(%first)) {
		case "a": return %this.a[%ending];
		case "b": return %this.b[%ending];
		case "c": return %this.c[%ending];
		case "d": return %this.d[%ending];
		case "e": return %this.e[%ending];
		case "f": return %this.f[%ending];
		case "g": return %this.g[%ending];
		case "h": return %this.h[%ending];
		case "i": return %this.i[%ending];
		case "j": return %this.j[%ending];
		case "k": return %this.k[%ending];
		case "l": return %this.l[%ending];
		case "m": return %this.m[%ending];
		case "n": return %this.n[%ending];
		case "o": return %this.o[%ending];
		case "p": return %this.p[%ending];
		case "q": return %this.q[%ending];
		case "r": return %this.r[%ending];
		case "s": return %this.s[%ending];
		case "t": return %this.t[%ending];
		case "u": return %this.u[%ending];
		case "v": return %this.v[%ending];
		case "w": return %this.w[%ending];
		case "x": return %this.x[%ending];
		case "y": return %this.y[%ending];
		case "z": return %this.z[%ending];
		case "_": return %this._[%ending];
	}
}

function SimObject::call(%this,%method,%v0,%v1,%v2,%v3,%v4,%v5,%v6,%v7,%v8,%v9,%v10,%v11,%v12,%v13,%v14,%v15,%v16,%v17)
{
	%lastNull = -1;
	for(%i = 0; %i < 18; %i ++)
	{
		%a = %v[%i];
		if(%a $= "")
		{
			if(%lastNull < 0)
				%lastNull = %i;
			continue;
		}
		else
		{
			if(%lastNull >= 0)
			{
				for(%e = %lastNull; %e < %i; %e ++)
				{
					if(%args !$= "")
						%args = %args @ ",";
					%args = %args @ "\"\"";
				}
				%lastNull = -1;
			}
			if(%args !$= "")
				%args = %args @ ",";
			%args = %args @ "\"" @ %a @ "\"";
		}
	}

	eval(%this @ "." @ %method @ "(" @ %args @ ");");
}

function serverPlay3dTimescale(%data, %position, %timescale) {
	%old = getTimescale();
	setTimescale(%timescale);
	serverPlay3d(%data, %position);
	setTimescale(%old);
}

function getRotFromAngleID(%angle) {
	switch(%angle) {
		case 0:
			return "1 0 0 0";
		
		case 1:
			return "0 0 1 90";
		
		case 2:
			return "0 0 1 180";
		
		case 3:
			return "0 0 -1 90";
	}
}

// ------------------------------------------------------------------------
//  Title:   Create Brick
//  Author:  Electrk
//  Version: 1
//  Updated: January 7th, 2020
// ------------------------------------------------------------------------
//  Utility function for brick creation.
// ------------------------------------------------------------------------
//  Include this code in your own scripts as an *individual file* called
//  "Support_CreateBrick.cs".  Do not modify this code.
// ------------------------------------------------------------------------
//  Notes:
//    + Use $CreateBrick::LastError to check for errors.
//    + Use $CreateBrick::DebugMode to enable debug messages.
// ------------------------------------------------------------------------

$CreateBrick::Version = 1;

//* Error codes -- Do not change these. *//

$CreateBrick::Error::None          = 0;  // There was no error and brick creation was successful.
$CreateBrick::Error::PlantOverlap  = 1;  // "Overlap" plant error.
$CreateBrick::Error::PlantFloat    = 2;  // "Float" plant error.
$CreateBrick::Error::PlantStuck    = 3;  // "Stuck" plant error.
$CreateBrick::Error::PlantUnstable = 4;  // "Unstable" plant error.
$CreateBrick::Error::PlantBuried   = 5;  // "Buried" plant error.
$CreateBrick::Error::Generic       = 6;  // There was some unspecified error creating the brick.
$CreateBrick::Error::DataBlock     = 7;  // Tried to create brick with invalid/nonexistent datablock.
$CreateBrick::Error::AngleID       = 8;  // Tried to create brick with an invalid angle ID.
$CreateBrick::Error::BrickGroup    = 9;  // Tried to create brick with a nonexistent brick group.

// Use this variable to check for errors.
$CreateBrick::LastError = $CreateBrick::Error::None;

// Whether or not to print debug messages.
$CreateBrick::DebugMode = false;

// ------------------------------------------------


// Function for brick creation.
//
// @param {fxDTSBrickData} data          - The datablock of the brick we want to create.
// @param {Vector3D}       pos           - The position we want to create the brick at.
// @param {AngleID}        angID         - The angle ID (0-3) of the brick (converts to rotation).
// @param {ColorID}        color         - The color ID (0-63) of the brick.
// @param {boolean}        plant         - Whether or not we want the brick to be planted.
// @param {BrickGroup}     [group]       - The brick group we want to add the brick to.
//                                         Defaults to BrickGroup_888888.
// @param {boolean}        [ignoreStuck] - Whether to ignore the "stuck" error and plant anyway.
//                                         Defaults to false.
// @param {boolean}        [ignoreFloat] - Whether to ignore the "float" error and plant anyway.
//                                         Defaults to false.
//
// @returns {fxDTSBrick|-1} Returns -1 if there was an issue creating or planting the brick.
//
function createNewBrick ( %data, %pos, %angID, %color, %plant, %group, %ignoreStuck, %ignoreFloat )
{
	// Make sure that the datablock exists and is even a brick datablock.
	if ( !isObject (%data)  ||  %data.getClassName () !$= "fxDTSBrickData" )
	{
		createBrickError ($CreateBrick::Error::DataBlock, "Invalid datablock '" @ %data @ "'");
		return -1;
	}

	switch ( %angID )
	{
		case 0: %rotation = "1 0 0 0";
		case 1: %rotation = "0 0 1 90.0002";
		case 2: %rotation = "0 0 1 180";
		case 3: %rotation = "0 0 -1 90.0002";

		default:
			createBrickError ($CreateBrick::Error::AngleID, "Invalid angleID '" @ %angID @ "'");
			return -1;
	}

	if ( %group $= "" )
	{
		%group = BrickGroup_888888;
	}
	else if ( !isObject (%group) )
	{
		createBrickError ($CreateBrick::Error::BrickGroup, "Brick group '" @ %group @ "' does not exist");
		return -1;
	}

	%brick = new fxDTSBrick ()
	{
		dataBlock = %data;

		position = %pos;
		rotation = %rotation;
		angleID  = %angID;

		colorID = %color;

		isPlanted = %plant;
	};

	// If brick creation failed for whatever reason, set the error to generic and return.
	if ( !isObject (%brick) )
	{
		createBrickError ($CreateBrick::Error::Generic, "Error creating fxDTSBrick object");
		return -1;
	}

	%group.add (%brick);

	if ( isObject (%group.client) )
	{
		%brick.client = %group.client;
	}
	
	if ( %plant )
	{
		%error = %brick.plant ();

		// Any value other than 0 returned from plant() means an error occurred.
		if ( (%error == 1  &&  !%ignoreStuck)  ||  (%error == 2  &&  !%ignoreFloat)  ||  %error >= 3 )
		{
			%brick.delete ();
			createBrickError (%error, "Error planting brick (code: " @ %error @ ")");

			return -1;
		}

		%brick.onPlant ();
		%brick.setTrusted (true);
	}

	$CreateBrick::LastError = $CreateBrick::Error::None;

	return %brick;
}

// Internal use only.  Do not use this function.
//
// @param {CreateBrickError} errorCode
// @param {string}           errorMessage
//
// @private
//
function createBrickError ( %errorCode, %errorMessage )
{
	$CreateBrick::LastError = %errorCode;

	if ( $CreateBrick::DebugMode )
	{
		error ("ERROR: createNewBrick () - " @ %errorMessage);
	}
}

function Player::setSpeedFactor(%this, %factor)
{
	%data = %this.getDataBlock();
	
	%this.setMaxForwardSpeed(%data.maxForwardSpeed * %factor);
	%this.setMaxBackwardSpeed(%data.maxBackwardSpeed * %factor);
	%this.setMaxSideSpeed(%data.maxSideSpeed * %factor);
	%this.setMaxCrouchForwardSpeed(%data.maxForwardCrouchSpeed * %factor);
	%this.setMaxCrouchBackwardSpeed(%data.maxBackwardCrouchSpeed * %factor);
	%this.setMaxCrouchSideSpeed(%data.maxSideCrouchSpeed * %factor);
	%this.setMaxUnderwaterForwardSpeed(%data.maxUnderwaterForwardSpeed * %factor);
	%this.setMaxUnderwaterBackwardSpeed(%data.maxUnderwaterBackwardSpeed * %factor);
	%this.setMaxUnderwaterSideSpeed(%data.maxUnderwaterSideSpeed * %factor);
}

function Player::getHorizontalAngle(%this) {
	return mACos(vectorDot("1 0 0", getWords(%this.getEyeVector(), 0, 1))) * (getWord(%this.getEyeVector(), 1) < 0 ? -1 : 1);
}

function Player::getVerticalAngle(%this) {
	return mASin(getWord(%this.getEyeVector(), 2));
}

function AiPlayer::setHorizontalAngle(%this, %angle) {
	%theta = %angle;
	%phi = %this.getVerticalAngle();
	%x = mCos(%theta) * mCos(%phi);
	%y = mSin(%theta) * mCos(%phi);
	%z = mSin(%phi);
	%this.setAimVector(%x SPC %y SPC %z);
}

function AiPlayer::setVerticalAngle(%this, %angle) {
	%theta = %this.getHorizontalAngle();
	%phi = %angle;
	%x = mCos(%theta) * mCos(%phi);
	%y = mSin(%theta) * mCos(%phi);
	%z = mSin(%phi);
	%this.setAimVector(%x SPC %y SPC %z);
}

function Player::unMountImageSafe(%this, %slot) {
	if(%this.getMountedImage(%slot)) {
		%this.unMountImage(%slot);
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

function mMin(%a1, %a2) {
	if(%a1 < %a2) {
		return %a1;
	}
	else {
		return %a2;
	}
}

function mMax(%a1, %a2) {
	if(%a1 > %a2) {
		return %a1;
	}
	else {
		return %a2;
	}
}