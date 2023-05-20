IF NOT EXIST publish (md publish) ELSE (del publish\*)
del publish\FileOrganizer.zip
cd FileOrganizerUI\publish
zip -r FileOrganizer.zip *
copy FileOrganizer.zip ..\..\publish