using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;

namespace MoveDraftAttachments
    {
    internal class Program
        {
        internal static int Main(string[] args)
            {
            try
                {
                Run(args);
                return 0;
                }
            catch (Exception ex)
                {
                Console.WriteLine();
                Console.WriteLine(ex.ToString());
                return 1;
                }
            }

        private static void Run(string[] args)
            {
            Console.WriteLine("Move draft attachments to draft folder");
            Console.WriteLine();

            var targetEnvironment = TargetEnvironment(args);
            if (string.IsNullOrWhiteSpace(targetEnvironment))
                {
                Console.WriteLine("Usage: MoveDraftAttachments <targetenvironment>");
                return;
                }
            var databaseConnectionString = DatabaseConnectionString(targetEnvironment);
            var attachmentsDirectory = AttachmentsDirectory(targetEnvironment);

            Console.WriteLine("Target environment: {0}", targetEnvironment);
            Console.WriteLine("Database: {0}", databaseConnectionString);
            Console.WriteLine("Attachments: {0}", attachmentsDirectory);
            Console.WriteLine();

            VerifyCanConnectToDatabase(databaseConnectionString);
            VerifyCanAccessAttachmentsDirectory(attachmentsDirectory);

            var p = new MoveDraftAttachmentsProcess(databaseConnectionString, attachmentsDirectory);
            p.Process();

            Console.WriteLine("Finished.");
            }

        private static void VerifyCanConnectToDatabase(string databaseConnectionString)
            {
            try
                { 
                using (var conn = new SqlConnection(databaseConnectionString))
                    {
                    conn.Open();
                    }
                }
            catch (Exception ex)
                { 
                throw new InvalidOperationException("Could not connect to the database. Check that the connection string is correct.", ex);
                }
            }

        private static void VerifyCanAccessAttachmentsDirectory(string attachmentsDirectory)
            {
            try
                {
                if (!Directory.Exists(attachmentsDirectory))
                    throw new InvalidOperationException("The attachments directory does not exist or could not be accessed.");
                if (!Directory.EnumerateDirectories(attachmentsDirectory).Any())
                    throw new InvalidOperationException("The attachments directory does not seem to be correct.");
                }
            catch (Exception ex)
                {
                throw new InvalidOperationException("An error occurred whilst accessing the attachments directory.", ex);
                }
            }

        private static string DatabaseConnectionString(string targetEnvironment)
            {
            var cs = ConfigurationManager.ConnectionStrings[targetEnvironment];
            if (cs == null)
                throw new InvalidOperationException("Could not get database connection string for " + targetEnvironment);
            var result = cs.ConnectionString;
            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException("Database connection string is invalid for " + targetEnvironment);
            return result;
            }

        private static string AttachmentsDirectory(string targetEnvironment)
            {
            var result = ConfigurationManager.AppSettings[targetEnvironment];
            if (string.IsNullOrWhiteSpace(result))
                throw new InvalidOperationException("Could not get attachments directory for " + targetEnvironment);
            return result;
            }

        private static string TargetEnvironment(string[] args)
        {
            var enumerator = args.AsEnumerable().GetEnumerator();
            if (!enumerator.MoveNext())
                return null;
            var result = enumerator.Current;
            return result;
            }
        }   
    }
