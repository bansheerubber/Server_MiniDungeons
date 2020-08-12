deActivatePackage(NTRewrite);
package NTRewrite {
	function SimObject::setNTObjectName(%this, %name) {
		Parent::setNTObjectName(%this, %name);

		%name = trim(%name);
		%name = getSafeVariableName(%name);
		if(getSubStr(%name, 0, 1) !$= "_") {
			%name = "_" @ %name;
		}

		%group = %this.getGroup();
		if(%group && %this.getName() $= %name && %group.NTObjectIndex[%name, %this] $= "") {
			%group.NTObjectIndex[%name, %this] = %group.NTObjectCount[%name] - 1;
		}
	}
	
	function SimObject::clearNTObjectName(%this) {
		%group = %this.getGroup();
		%oldName = %this.getName();
		if(
			%group
			&& %oldName !$= ""
			&& %group.NTObjectIndex[%oldName, %this] !$= ""
		) {
			// use swapping method instead of badspot's stupid way of looping through the entire list
			if(%group.NTObjectCount[%oldName] > 1) {
				%index = %group.NTObjectIndex[%oldName, %this];
				// swap current index with last
				%group.NTObject[%oldName, %index] = %group.NTObject[%oldName, %group.NTObjectCount[%oldName] - 1];
				%group.NTObjectIndex[%oldName, %group.NTObject[%oldName, %index]] = %index;

				%group.NTObject[%oldName, %group.NTObjectCount[%oldName] - 1] = "";
				%group.NTObjectIndex[%oldName, %this] = "";

				%group.NTObjectCount[%oldName]--;
			}
			else {
				%group.NTObject[%oldName, 0] = "";
				%group.NTObjectIndex[%oldName, %this] = "";
				%group.NTObjectCount = 0;
			}

			if(%group.NTObjectCount[%oldName] <= 0) {
				%group.removeNTName(%oldName);
			}
		}
	}

	function SimGroup::addNTName(%this, %name) {
		if(%this.NTNameIndex[%name] $= "") {
			%this.NTNameIndex[%name] = %this.NTNameCount | 0;
		}

		Parent::addNTName(%this, %name);
	}

	function SimGroup::removeNTName(%this, %name) {
		if(%this.NTNameIndex[%name] !$= "") {
			// use swapping method instead of badspot's stupid way of looping through the entire list
			if(%this.NTNameCount > 1) {
				%index = %this.NTNameIndex[%name];
				// swap current index with last
				%this.NTName[%index] = %this.NTName[%this.NTNameCount - 1];
				%this.NTNameIndex[%this.NTName[%index]] = %index;
			
				%this.NTName[%this.NTNameCount - 1] = "";
				%this.NTNameIndex[%name] = "";

				%this.NTNameCount--;
			}
			else {
				%this.NTName[0] = "";
				%this.NTNameIndex[%name] = "";
				%this.NTNameCount = 0;
			}

			%count = ClientGroup.getCount();
			for(%i = 0; %i < %count; %i++) {
				%client = ClientGroup.getObject(%i);
				if(isObject(%client.brickGroup) && %client.brickGroup.getId() == %this.getId()) {
					commandToClient(%client, 'RemoveNTName', %name);
				}
			}
		}
	}
};
activatePackage(NTRewrite);