function setMDGlobal(%value, %name, %arg0, %arg1, %arg2, %arg3) {
	%lastArg = -1;
	for(%i = 0; %i < 4; %i++) {
		if(%arg[%i] !$= "") {
			%lastArg = %i;
		}
	}

	%argString = "";
	for(%i = 0; %i <= %lastArg; %i++) {
		%argString = %argString SPC %arg[%i];
	}
	%argString = strReplace(trim(%argString), " ", "_");
	eval("$MD::" @ %name @ %argString SPC "= \"" @ %value @ "\";");
}

function getMDGlobal(%name, %arg0, %arg1, %arg2, %arg3) {
	%lastArg = -1;
	for(%i = 0; %i < 4; %i++) {
		if(%arg[%i] !$= "") {
			%lastArg = %i;
		}
	}

	%argString = "";
	for(%i = 0; %i <= %lastArg; %i++) {
		%argString = %argString SPC %arg[%i];
	}
	%argString = strReplace(trim(%argString), " ", "_");
	eval("%result = $MD::" @ %name @ %argString @ ";");
	return %result;
}