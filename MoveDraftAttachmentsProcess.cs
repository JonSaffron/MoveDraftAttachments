using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MoveDraftAttachments
    {
    internal class MoveDraftAttachmentsProcess
        {
        private readonly string _databaseConnectionString;
        private readonly string _attachmentsDirectory;

        public MoveDraftAttachmentsProcess(string databaseConnectionString, string attachmentsDirectory)
            {
            if (string.IsNullOrWhiteSpace(databaseConnectionString))
                throw new ArgumentException("databaseConnectionString");
            if (string.IsNullOrWhiteSpace(attachmentsDirectory))
                throw new ArgumentException("attachmentsDirectory");

            this._databaseConnectionString = databaseConnectionString;
            this._attachmentsDirectory = attachmentsDirectory;
            }

        public void Process()
            {
            var listToProcess = GetDraftAttachmentList();
            foreach (var item in listToProcess)
                {
                RenderItem(item);
                try
                    {
                    ProcessItem(item);
                    }
                catch (Exception ex)
                    {
                    Console.WriteLine(ex.ToString());
                    }
                }
            }

        private IEnumerable<Attachment> GetDraftAttachmentList()
            {
            var dt = GetAttachmentsRawData();
            var result =
                from DataRow dr in dt.Rows 
                select new Attachment
                    {
                    ArchetypeCode = (string) dr["ParentArchetypeCode"],
                    Parent = (Guid) dr["ParentItemID"],
                    FileName = (string) dr["FileName"]
                    };
            return result;      // lazy enumeration
            }

        private DataTable GetAttachmentsRawData()
            {
            using (var conn = new SqlConnection(this._databaseConnectionString))
                {
                conn.Open();

                var sql = "SELECT ParentArchetypeCode, ParentItemID, FileName FROM NxAttachment_Draft ORDER BY ParentArchetypeCode";
                using (var command = new SqlCommand(sql, conn))
                    {
                    using (var adapter = new SqlDataAdapter(command))
                        {
                        var result = new DataTable();
                        adapter.Fill(result);
                        return result;
                        }
                    }
                }
            }

        private static void RenderItem(Attachment attachment)
            {
            Console.WriteLine();
            Console.WriteLine("- {0} {1} {2}", attachment.ArchetypeCode, attachment.Parent, attachment.FileName);
            }

        private void ProcessItem(Attachment attachment)
            {
            if (IsAttachmentAlreadyLocatedInDraftDirectory(attachment))
                {
                Console.WriteLine("Already in draft directory - no action will be taken.");
                return;
                }                

            if (!CanAttachmentBeFound(attachment))
                {
                Console.WriteLine("Cannot find attachment - no action will be taken.");
                return;
                }

            MoveAttachmentToDraftDirectory(attachment);
            Console.WriteLine("File moved to Draft directory.");
            }

        private bool IsAttachmentAlreadyLocatedInDraftDirectory(Attachment attachment)
            {
            var whereFileShouldEndUp = GenerateDestinationFileName(attachment);
            var result = File.Exists(whereFileShouldEndUp);
            return result;
            }

        private bool CanAttachmentBeFound(Attachment attachment)
            {
            var whereFileShouldStart = GenerateSourceFileName(attachment);
            var result = File.Exists(whereFileShouldStart);
            return result;
            }

        private void MoveAttachmentToDraftDirectory(Attachment attachment)
            {
            var source = GenerateSourceFileName(attachment);
            var destination = GenerateDestinationFileName(attachment);
            var destinationDirectory = Path.GetDirectoryName(destination);
            EnsureDirectoryExists(destinationDirectory);
            File.Move(source, destination);
            }

        private string GenerateSourceFileName(Attachment attachment)
            {
            var result = Path.Combine(this._attachmentsDirectory, attachment.ArchetypeCode);
            result = Path.Combine(result, attachment.Parent.ToString("D"));
            result = Path.Combine(result, attachment.FileName);
            return result;
            }

        private string GenerateDestinationFileName(Attachment attachment)
            {
            var result = Path.Combine(this._attachmentsDirectory, attachment.ArchetypeCode);
            result = Path.Combine(result, attachment.Parent.ToString("D"));
            result = Path.Combine(result, "Draft");
            result = Path.Combine(result, attachment.FileName);
            return result;
            }

        private static void EnsureDirectoryExists(string pathToDirectory)
            {
            if (Directory.Exists(pathToDirectory))
                return;
            string parentDirectory = Path.GetDirectoryName(pathToDirectory);
            if (string.IsNullOrWhiteSpace(parentDirectory))
                throw new InvalidOperationException("Invalid directory " + pathToDirectory);
            EnsureDirectoryExists(parentDirectory);
            Directory.CreateDirectory(pathToDirectory);
            }
        }
    }
