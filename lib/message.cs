function messageClientsInArea(%position, %radius, %type, %message) {
	initContainerRadiusSearch(%position, %radius, $TypeMasks::PlayerObjectType);
	while(%col = containerSearchNext()) {
		if(isObject(%col.client)) {
			messageClient(%col.client, %type, %message);
		}
	}
}