function vectorLerp(%start, %end, %percent) {
	return vectorAdd(vectorScale(%start, 1 - %percent), vectorScale(%end, %percent));
}

function vectorLerpUnit(%start, %end, %units) {
	%percent = %units / vectorDist(%start, %end);
	return vectorAdd(vectorScale(%start, 1 - %percent), vectorScale(%end, %percent));
}

function bezier3pt(%point1, %point2, %point3, %t) {
	return vectorLerp(vectorLerp(%point1, %point2, %t), vectorLerp(%point2, %point3, %t), %t);
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