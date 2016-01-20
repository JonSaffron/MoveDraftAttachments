# MoveDraftAttachments
**Move 3e draft attachments to the Draft subdirectory**

At some point in time, the 3e framework started to expect to find attachments that are associated with draft records to be located in the Draft subdirectory.

In other words, when a new Client record is created and a file attached to it, that file would be stored in
```
TE_3E_Share\TE_3E_Instance\Inetpub\Attachment\Client\(record guid)\Draft
```
and then, when the record was saved, the file would be moved up a directory to the main storage area.

If you have draft records with attachments dating back to before this change was made then you can run this utility to get the files to the correct location.

Simply modify the app.config file to provide the database connection string and location of the attachments directory and it will scan the NxAttachment_Draft table and move any matching files it finds in the main area into the Draft subdirectory.
