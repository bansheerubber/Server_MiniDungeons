function traceMemory(%max) {
	deleteVariables("$Memory*");
	
	$Memory::Count = 0;
	for(%i = 0; %i < %max; %i++) {
		if(isObject(%i)) {
			if($Memory[%i.getClassName()] $= "") {
				$Memory[$Memory::Count] = %i.getClassName();
				$Memory::Count++;
			}
			
			$Memory[%i.getClassName()]++;
		}
	}

	printMemoryReport();
}

function printMemoryReport() {
	for(%i = 0; %i < $Memory::Count; %i++) {
		talk($Memory[%i] SPC $Memory[$Memory[%i]]);
	}
}