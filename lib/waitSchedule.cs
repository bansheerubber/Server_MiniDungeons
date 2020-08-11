// creates a wait schedules. wait schedules are only evaluted when the specified conditions are met. if they are not met, the schedule waits until the conditions are met, with conditions checked every tick
function SimObject::waitSchedule(%this, %time, %call, %arg0, %arg1, %arg2, %arg3, %arg4, %arg5, %arg6, %arg7, %arg8, %arg9, %arg10) {
	%waitSchedule = new ScriptObject(WaitSchedule) {
		call = %call;
		time = %time;
		owner = %this;	
		conditionCount = 0;
	};

	for(%i = 0; %i <= 11; %i++) {
		%waitSchedule.arg[%i] = %arg[%i];
	}

	%waitSchedule.schedule(%time, loop);

	if(!isObject(%this.waitScheduleGroup)) {
		%this.waitScheduleGroup = new ScriptGroup();
	}
	%this.waitScheduleGroup.add(%waitSchedule);

	return %waitSchedule;
}

function WaitSchedule::loop(%this) {
	// test the conditions and see if they match
	for(%i = 0; %i < %this.conditionCount; %i++) {
		%obj = %this.conditions[%i, "object"];
		if(isObject(%obj)) {
			if(%this.conditions[%i, "field"] !$= "") {
				if(%this.conditions[%i, "isString"]) {
					if(%obj.getField(%this.conditions[%i, "field"]) !$= %this.conditions[%i, "value"]) {
						%this.schedule(100, loop);
						return;
					}
				}
				else {
					if(%obj.getField(%this.conditions[%i, "field"]) != %this.conditions[%i, "value"]) {
						%this.schedule(100, loop);
						return;
					}
				}
			}
			else {
				%methodValue = %obj.call(%this.conditions[%i, "method"]);
				if(%this.conditions[%i, "isString"]) {
					if(%methodValue !$= %this.conditions[%i, "value"]) {
						%this.schedule(100, loop);
						return;
					}
				}
				else {
					if(%methodValue != %this.conditions[%i, "value"]) {
						%this.schedule(100, loop);
						return;
					}
				}
			}
		}
	}

	%this.owner.call(%this.call, %this.arg[0], %this.arg[1], %this.arg[2], %this.arg[3], %this.arg[4], %this.arg[5], %this.arg[6], %this.arg[7], %this.arg[8], %this.arg[9], %this.arg[10], %this.arg[11], %this.arg[12], %this.arg[13], %this.arg[14]);

	if(isObject(%this)) {
		%this.delete();
	}
}

function WaitSchedule::addCondition(%this, %object, %field, %value, %isString) {
	%this.conditions[%this.conditionCount, "object"] = %object;
	%this.conditions[%this.conditionCount, "field"] = %field;
	%this.conditions[%this.conditionCount, "value"] = %value;
	%this.conditions[%this.conditionCount, "isString"] = %isString;
	%this.conditionCount++;
	
	return %this;
}

function WaitSchedule::addMethodCondition(%this, %object, %method, %value, %isString) {
	%this.conditions[%this.conditionCount, "object"] = %object;
	%this.conditions[%this.conditionCount, "method"] = %method;
	%this.conditions[%this.conditionCount, "value"] = %value;
	%this.conditions[%this.conditionCount, "isString"] = %isString;
	%this.conditionCount++;
	
	return %this;
}

function SimObject::onRemove(%this) {
	
}

deActivatePackage(WaitSchedule);
package WaitSchedule {
	function SimObject::onRemove(%this) {
		if(isObject(%this.waitScheduleGroup)) {
			%this.waitScheduleGroup.delete();
		}

		Parent::onRemove(%this);
	}
};
activatePackage(WaitSchedule);

function Player::testWaitSchedule(%this) {
	%this.frog = new ScriptObject();
	%this.waitSchedule(5000, "speak", "hello how do you do").addCondition(%this.frog, "isFrog", true).addCondition(%this, "isGay", true);
}

function Player::speak(%this, %message) {
	messageAll('', %message);
}