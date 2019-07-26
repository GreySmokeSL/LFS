Solution requirements (test task conditions):
- generate large text file (up to about 100GB) with line pattern {Number. String}, e.g.
	415. Apple
	30432. Something something something
	1. Apple
	32. Cherry is the best
	2. Banana is yellow...

	Target file may contain several lines with same String suffix or Number prefix.

- arrange file lines and save to new file: sort by String suffix first, then by Number prefix. 
	Only RAM (up to 16GB) can be used, intermediate storages (file, DB etc.) are not allowed.
	An unlimited number of file reading.

Multithreading and good WPF UI are obligatory! No time limits.





Solution description.

 "Text File Processor" is WPF UI MVVM desktop application, provides next functionality according to requirements:
 - text file generation of specified count of lines by pattern
 - arrangement lines by sorting in memory to a new sorted file
 - additionally: file content preview 

Projects:
- Core - contains common extensions and helpers;
- Domain - contains base classes, enums, models and application constants;
- BLL - business logic (repositories, processors, comparers etc.)
- LFSApp - main WPF project, contains views, viewmodels, custom user controls etc.
- Tester - additional console application, provide file processing without UI

Third-part libraries:
- Bogus - library for fake data generation
- DevExpressMVVM - MVVM components (e.g. command wrapper)
- Extended.WPF.Toolkit - custom control (e.g. SpinEdit)
Also Themes folder - contains partially modified controls styles 



Algorithms and main classes.
User in the main application window must choose a mode, set obligatory parameters and run execution.
Progress of task can be observed using progressbars and logs.
Executing can be terminated.
After successful file processing app executes switch to preview mode.


1. FakeDataGenerator:
	- receives user params from model
	- splits execution on portion and generate results
	- simultaneously flush to target file.

2. DataProcessor:
	- receives user params from model
	- creates a set of readers, splitting source file into blocks
	- on first file reading, analyzes data structure for each reader, determines set of String suffix first words to preorder data (markers)
	- orders markers and splits by groups with allowed line count (for sorting and memory usage)
	- provides file reading for each group, aggregating lines for sort and flush to target file, mark lines as done.



Test environment:
OS		Windows 10 Pro x64

		- Pagefile: system defaults, base size = 5GB (located on SSD)

	
PC		Notebook Dell G3

		Intel(R) Core(TM) i7-8750H CPU @ 2.20GHz 6 core (12 logical processors)
		16GB RAM
		SSD INTEL SSDSCKKF256G8 SATA 256GB 
		HDD ST2000LM007-1R8174 2TB


Data generation:
	- source = RAM+CPU
	- target = SSD

Data arrangement:
	- source = SSD
	- target = HDD
