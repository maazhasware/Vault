using csharpcorner.Models;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using System.Security.Cryptography;

namespace csharpcorner.ViewModels
{
    public class FirebaseHelper
    {
        //Connect app with firebase database 
        public static FirebaseClient firebaseDatabase = new FirebaseClient("https://vault-cf38b.firebaseio.com/");

        //connect app with firebase storage
        public static FirebaseStorage firebaseStorage = new FirebaseStorage("vault-cf38b.appspot.com");

        //Read All    
        public static async Task<List<User>> GetAllUser()
        {
            try
            {
                var userlist = (await firebaseDatabase
                .Child("Users")
                .OnceAsync<User>()).Select(item =>
                new User
                {
                    Key = item.Object.Key,
                    Salt = item.Object.Salt,
                    UserID = item.Object.UserID,
                    FirstName = item.Object.FirstName,
                    Surname = item.Object.Surname,
                    Email = item.Object.Email,
                    Password = item.Object.Password
                }).ToList();
                return userlist;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        //Get User
        public static async Task<User> GetUser(string email)
        {
            try
            {
                var allUsers = await GetAllUser();
                await firebaseDatabase
                .Child("Users")
                .OnceAsync<User>();
                return allUsers.Where(a => a.Email == email).FirstOrDefault();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        //Generate secure bytes for key and salt use
        public static byte[] GenerateSecureBytes()
        {
            byte[] bytes = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                for (int i = 0; i < 10; i++)
                {
                    // Fill the buffer with the generated data
                    rng.GetBytes(bytes);
                }
            }
            return bytes;
        }

        //Add new user    
        public static async Task<bool> AddUser(
            string firstname,
            string surname,
            string email, 
            string password
            )
        {
            try
            {
                var userAlreadyExists = await GetUser(email);
                if (userAlreadyExists == null)
                {
                    try
                    {
                        await firebaseDatabase
                        .Child("Users")
                        .PostAsync(new User()
                        {
                            UserID = Guid.NewGuid(),
                            FirstName = firstname,
                            Surname = surname,
                            Email = email,
                            Password = password,
                            Key = GenerateSecureBytes(), //TODO: store in KMS
                            Salt = GenerateSecureBytes()
                    });

                        return true;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error:{e}");
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return false;
            }
        }

        //Upload file to firebase storage
        public static async Task<string> UploadFile(FileStream fileStream, string fileName, Guid userid)
        {
            try
            {
                var fileAlreadyExists = await GetFile(fileName, userid);
                if (fileAlreadyExists == null)
                {
                    try
                    {
                        var imageurl = await firebaseStorage
                        .Child("Media")
                        .Child(fileName + userid)
                        .PutAsync(fileStream);
                        return imageurl;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error:{e}");
                        return null;
                    }
                }
                else
                {
                    //below code never gets used, firebase already recognises it is duplicate and appends a number to the filename, prevents duplicates
                    try
                    {
                        var imageurl = await firebaseStorage
                        .Child("Media")
                        .Child(fileName + Guid.NewGuid() + userid)
                        .PutAsync(fileStream);
                        return imageurl;
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine($"Error:{e}");
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            } 
        }

        //upload uploaded file's download url for easy retrieval later

        public static async Task<bool> UploadURL(string fileName, string downloadurl, Guid userID)
        {
            try
            {
                await firebaseDatabase
                .Child("DownloadURLs")
                .PostAsync(new DownloadURL()
                {
                    FileName = fileName,
                    Url = downloadurl,
                    Userid = userID
                });
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return false;
            }
        }

        // Get a file using file name
        public static async Task<string> GetFile(string fileName, Guid userid)
        {
            try
            {
                return await firebaseStorage
                .Child("Media")
                .Child(fileName + userid)
                .GetDownloadUrlAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        //Get user's files download urls only
        public static async Task<List<string>> GetUsersFilesDownloadURL(Guid userid)
        {
            var allFiles = await GetAllFiles();
            List<string> usersFiles = new List<string>();
            foreach (var a in allFiles)
            {
                if (a.Userid == userid)
                {
                    usersFiles.Add(a.Url);
                }
            }
            return usersFiles;
        }

        //Get user's files downloadurl objects
        public static async Task<List<DownloadURL>> GetUsersFilesDownloadURLObject(Guid userid)
        {
            var allFilesDownloadURLObjects = await GetAllFiles();
            List<DownloadURL> usersFilesDownloadsURLObjects = new List<DownloadURL>();
            foreach (var a in allFilesDownloadURLObjects)
            {
                if (a.Userid == userid)
                {
                    usersFilesDownloadsURLObjects.Add(a);
                }
            }
            return usersFilesDownloadsURLObjects;
        }

        //Get all files
        public static async Task<List<DownloadURL>> GetAllFiles()
        {
            try
            {
                var listOfDownloadUrls = (await firebaseDatabase
                .Child("DownloadURLs")
                .OnceAsync<DownloadURL>()).Select(item =>
                new DownloadURL
                {
                    FileName = item.Object.FileName,
                    Url = item.Object.Url,
                    Userid = item.Object.Userid
                }).ToList();
                return listOfDownloadUrls;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        // Delete file
        public static async Task DeleteFile(string fileName, Guid userid)
        {
            await firebaseStorage
                 .Child("Media")
                 .Child(fileName + userid)
                 .DeleteAsync();

        }

        //Delete downloadUrlObject
        public static async Task DeleteDownloadURLObject(string fileName, Guid userid)
        {
            try
            {
                var toDeleteObject = (await firebaseDatabase
                .Child("DownloadURLs")
                .OnceAsync<DownloadURL>()).Where(a => a.Object.FileName == fileName && a.Object.Userid == userid).FirstOrDefault();
                await firebaseDatabase.Child("DownloadURLs").Child(toDeleteObject.Key).DeleteAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
            }

        }
      



        //Update     
        public static async Task<bool> UpdateUser(string email, string password)
        {
            try
            {
                var toUpdateUser = (await firebaseDatabase
                .Child("Users")
                .OnceAsync<User>()).Where(a => a.Object.Email == email).FirstOrDefault();
                await firebaseDatabase
                .Child("Users")
                .Child(toUpdateUser.Key)
                .PutAsync(new User() { Email = email, Password = password });
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return false;
            }
        }

        //Delete User    
        public static async Task<bool> DeleteUser(string email)
        {
            try
            {
                var toDeletePerson = (await firebaseDatabase
                .Child("Users")
                .OnceAsync<User>()).Where(a => a.Object.Email == email).FirstOrDefault();
                await firebaseDatabase.Child("Users").Child(toDeletePerson.Key).DeleteAsync();
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return false;
            }
        }

    }
}
