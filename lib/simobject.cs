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