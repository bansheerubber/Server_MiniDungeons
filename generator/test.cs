function testRoom(%size, %type, %difficulty, %index) {
	$MD::DungeonRoomSet = new SimSet();
	BrickGroup_999999.deleteAll();
	createRoom("0 0", %size, %type, %difficulty, %index).roomBuild();

	schedule(100, 0, buildNodesFromBricks, BrickGroup_999999);
}