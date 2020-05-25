function Player::setSpeedFactor(%this, %factor) {
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