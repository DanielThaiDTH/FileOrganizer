# Notes

## Tag relationships (Plan)

**Primary Key**: TagID

**Column**: ParentTagID

Unique Constraint (TagID, ParentTagID)

### Adding steps

1. Check for reverse relationship (Existing TagID = new ParentTagID and vice versa), check for check for 
unique constraint and primary key, stop if found

2. Start transaction

3. Add to table

4. Successively search up ancestor IDs (check parent, then grandparent, etc.) and ensure the tag ID of the current 
relationship does not ever appear as a ParentTagID for each tag relationship with an ancestor ID. If this step is 
successful, continue. If not, rollback and end.

5. Commit transaction

### Update (change parent) steps

1. Check for relationship existence using the primary key, continue if found

2. Change parent tag id 

*Automatically updating assigned tags to files in consideration*
 
### Steps to assign ancestor tags as well when assigning new tags

1. Start transaction

2. Assign new tag to file

3. For each ancestor relationship, assign the ancestor ID to the file. Record error if failed

4. Commit transaction



