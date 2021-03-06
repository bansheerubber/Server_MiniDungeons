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

function mFloorMultiple(%number, %multiple) {
	return mFloor(%number / %multiple) * %multiple;
}

function mFloorMultipleCenter(%number, %multiple) {
	return mFloor((%number + %multiple / 2) / %multiple) * %multiple;
}

function mRound(%number) {
	%decimal = %number - mFloor(%number);

	if(%number < 0) {
		if(%decimal > 0.5) {
			return mCeil(%number);
		}
		else {
			return mFloor(%number);
		}
	}
	else {
		if(%decimal >= 0.5) {
			return mCeil(%number);
		}
		else {
			return mFloor(%number);
		}
	}
}