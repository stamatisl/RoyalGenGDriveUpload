using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Google.Apis;
using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;

namespace FileUploader
{

    public class RoyalGenGDriveUpload
    {

        // public DriveService service;
        public static string DriveUploadBasic(string fileName)
        {
            try
            {
                // "SERVICE_ACCOUNT_EMAIL_HERE";
                String serviceAccountEmail = "[OMITTED FOR SECURITY REASONS]@blahblah.iam.gserviceaccount.com";
                string[] SCOPES = { "https://www.googleapis.com/auth/drive" };

                // Scope and user email id which you want to impersonate
                var initializer = new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = SCOPES,
                    User = "[OMITTED FOR SECURITY REASONS]"
                };

                //get private key, from .JSON file
                var credential = new ServiceAccountCredential(initializer.FromPrivateKey("-----BEGIN PRIVATE KEY-----\n[OMITTED FOR SECURITY REASONS]\n-----END PRIVATE KEY-----\n"));
                
                // Create Drive API service.
                var service = new DriveService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = credential,
                    ApplicationName = "[OMITTED FOR SECURITY REASONS]"
                });
            

                // Upload file on drive.
                var fileMetadata = new Google.Apis.Drive.v3.Data.File()
                {
                    Name = fileName,
                    // Parents = new List<string> { "1Ph3i2qtHmzHdcH_wCW38DiX-kogGsViK" } // Unique ID of the folder, if exists
                };

                string query = "trashed = false and name = '" + fileName + "'";
                FilesResource.ListRequest req;
                req = service.Files.List();
                req.Q = query;
                req.Fields = "files(id, name)";
                var result = req.Execute();
                Console.WriteLine("Files with the same name: {0}", result.Files.Count);


                if (result.Files.Count >= 1)
                {
                    var updatedFileMetadata = new Google.Apis.Drive.v3.Data.File()
                    {
                        Name = fileName
                    }; 

                    FilesResource.UpdateMediaUpload updateRequest;
                    // Update a file on drive.
                    string fileId = result.Files[0].Id;
                    string fileMime = result.Files[0].MimeType;

                    using (var stream = new FileStream(fileName, FileMode.Open))
                    {
                        // Update a file, with metadata, stream and content type.
                        updateRequest = service.Files.Update(updatedFileMetadata, fileId, stream, fileMime);
                        updateRequest.Upload();

                    }

                    // Prints the uploaded file id.
                    Console.WriteLine("Uploaded File ID: " + updateRequest.ResponseBody.Id);
                    return updateRequest.ResponseBody.Id;
                } 
                else
                { 
                    FilesResource.CreateMediaUpload request;
                    // Create a new file on drive.
                    using (var stream = new FileStream(fileName, FileMode.Open))
                    {
                        // Create a new file, with metadata and stream.
                        request = service.Files.Create(fileMetadata, stream, "application/unknow");
                        request.Fields = "id";
                        request.Upload();
                    }

                    var file = request.ResponseBody;
                    // Prints the uploaded file id.
                    Console.WriteLine("Uploaded File ID: " + file.Id);
                    return file.Id;
                }
            }
            catch (Exception e)
            {
                // TODO(developer) - handle error appropriately
                if (e is AggregateException)
                {
                    Console.WriteLine("Credential Not found");
                }
                else if (e is FileNotFoundException)
                {
                    Console.WriteLine("File not found");
                }
                else
                {
                    throw;
                }
            }

            return null;
        }
    
        public static void Main(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("Filename: {0}", args[i]);
                DriveUploadBasic(args[i]);

            }

        }

    }

}
