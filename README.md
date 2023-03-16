# About

## Testing

The default command to test the FileDBManager is `dotnet test FileDBManagerTest\FileDBManagerTest.csproj -v n --runtime win-x64`.
There is a TestDataTemplate.xml config file. Copy it and modify it to refer to the test db location. Name it TestData.xml in the same folder.

The command to test the SymLinkMaker is `dotnet test SymLinkMakerTest\SymLinkMakerTest.csproj -v n --runtime win-x64`. You 
must run this in admin mode due to file access requirements.

The command to test the FileOrganizerCore is `dotnet test FileOrganizerCoreTest\FileOrganizerCoreTest.csproj -v n --runtime win-x64`. You must have a config.xml file in the folder with the executable. Copy the one in the test folder root. Requires a symlink and symlink2 folder in the executable root for testing. Requires admin access for tests to pass.