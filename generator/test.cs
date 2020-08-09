function testRoom(%size, %index) {
	$MD::DungeonRoomSet = new SimSet();
	BrickGroup_999999.deleteAll();
	createRoom("0 0", %size).roomBuild();
}