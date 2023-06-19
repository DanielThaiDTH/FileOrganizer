# About

A WinfForms file organizer application that can tag files, search up known files, categorize files and create symlinks to files.

## Building

This application uses the .NET Framework version `>=4.7.2`. To build it, you either need Visual Studio 2019 or above, or you need to have MSBuild installed and on your PATH. First restore the packages with `msbuild -t:Restore`, then build with `msbuild`.

## Usage

To let the organizer know about files to manage, start by opening the application and then clicking on `Add files`. Select the files you wish to add through the file picker. Basic information like the location, name, creation date and hash is automatically added. 

Added files can be searched up using the search bar on top. Plain entry of a query will match the filename. Case is ignored in plain queries. You can also create more advanced queries (explanation to be added later). Double clicking on any result of the query (located in the large empty space in the middle) will open up a file info modal. Editing of file metadata and deleting the metadata on a file is also possible here. Right clicking any result in the query will show a list of tags to the left. For image files, an image preview is present on the right half. There is also a button to open the file itself on the top.

When one file is selected in the file results panel, there several key commands available for performing specific actions:
- `Enter` or `Ctrl+O` to open the file with the system specified default program
- `Ctrl+Enter` to open up the folder the file is stored in
- `Ctrl+A` to select all items in results

On the side bar there will be another search bar, along with a section to add a new tag. Searches of tags simply checks if the query is found in anywhere in the name of a tag. The results of a tag search will be listed in the box in the middle. Double clicking any lets you edit or delete a tag. Tags will be group into user defined categories. You can see the category of the tag by hovering over the tag or looking at the colour icon to the left of it. New tags can be assigned a category by using the dropdown. Tags can be assigned to files by selecting tags in the side panel and files in the main panel and clicking assign. When you right click a file and the select the associated tags, you can remove tags from the file by clicking the remove button.

Files can be grouped into collections. They can be found on the other tab on the side panel. Each file will have a position in the collection that can be set by double clicking the collection and moving the files in the dialog. 

The advanced buttons opens up a window to manage tag categories, batch update paths, export file data and more to come. The tag category tab has fields for adding, renaming, deleting and assigning colours to. The update tab can either change the path for all files with a certain path, or it can change the path for files in the query result view. The export tab has controls for specifying what information about files to export. It can be a JSON file with detailed information or just a text file for filenames/filepaths.

The settings button lets you save a copy of the DB and also set the location to generate symlinks. Note that the program will clear all symlinks in the folder selected. On a similar note, the DB is backed-up each time the program is started up. The backup will be `${database_name}.bak`. The database name and other settings are in the app.config file. 

This application can generate symlinks from queried files or from file collections. The symlink button on the bottom right is for queried files, while the button on the left is for file collections. Symlinks for collections will be named by the position the file is in in the collection. This feature is only enabled if you run with elevated permissions.

This program also generates logs in the same folder as the executable.

## Testing

The command to test the FileDBManager is `msbuild  FileDBManagerTest\FileDBManagerTest.csproj -t:Test`.

The command to test the SymLinkMaker is `msbuild SymLinkMakerTest\SymLinkMakerTest.csproj -t:Test`. You 
must run this in admin mode due to file access requirements.

The command to test the FileOrganizerCore is `msbuild FileOrganizerCoreTest\FileOrganizerCoreTest.csproj -t:Test`. You also need admin 
access here.