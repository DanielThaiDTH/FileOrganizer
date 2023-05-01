# About

A WinfForms file organizer application that can tag files, search up known files and create symlinks to files.

## Usage

To let the organizer know about files to manage, start by opening the application and then clicking on `Add files`. Select the files you wish to add through the file picker. Basic information like the location, name, creation date and hash is automatically added. 

Added files can be searched up using the search bar on top. Plain entry of a query will match the filename. Case is ignored in plain queries. You can also create more advanced queries (explanation to be added later). Double clicking on any result of the query (located in the large empty space in the middle) will open up a file info modal. Editing of file metadata and deleting the metadata on a file is also possible here. 

On the side bar there will be another search bar, along with a section to add a new tag. Searches of tags simply checks if the query is found in anywhere in the name of a tag. The results of a tag search will be listed in the box in the middle. Double clicking any lets you edit or delete a tag.

## Testing

The default command to test the FileDBManager is `dotnet test FileDBManagerTest\FileDBManagerTest.csproj -v n --runtime win-x64`.
There is a TestDataTemplate.xml config file. Copy it and modify it to refer to the test db location. Name it TestData.xml in the same folder.

The command to test the SymLinkMaker is `dotnet test SymLinkMakerTest\SymLinkMakerTest.csproj -v n --runtime win-x64`. You 
must run this in admin mode due to file access requirements.

The command to test the FileOrganizerCore is `dotnet test FileOrganizerCoreTest\FileOrganizerCoreTest.csproj -v n --runtime win-x64`. You must have a config.xml file in the folder with the executable. Copy the one in the test folder root. Requires a symlink and symlink2 folder in the executable root for testing. Requires admin access for tests to pass.