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

        //***USER RELATED METHODS BELOW***

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


        //Update     
        public static async Task<bool> UpdateUser(
            Guid userID,
            string firstName,
            string surname,
            string email,
            string password,
            byte[] key,
            byte[] salt)
        {
            try
            {
                var toUpdateUser = (await firebaseDatabase
                .Child("Users")
                .OnceAsync<User>()).Where(a => a.Object.Email == email).FirstOrDefault();
                await firebaseDatabase
                .Child("Users")
                .Child(toUpdateUser.Key)
                .PutAsync(new User() { UserID = userID, FirstName = firstName, Surname = surname, Email = email, Password = password, Key = key, Salt = salt });
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

        //***MEDIA RELATED METHODS BELOW***


        //Upload image 
        public static async Task<string> UploadImage(FileStream fileStream, string fileName, Guid userid)
        {
            try
            {
                var fileAlreadyExists = await GetImage(fileName, userid);
                if (fileAlreadyExists == null)
                {
                    try
                    {
                        var imageurl = await firebaseStorage
                        .Child("Images")
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
                        .Child("Images")
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

        //upload video
        public static async Task<string> UploadVideo(FileStream fileStream, string fileName, Guid userid)
        {
            try
            {
                var fileAlreadyExists = await GetVideo(fileName, userid);
                if (fileAlreadyExists == null)
                {
                    try
                    {
                        var videourl = await firebaseStorage
                        .Child("Videos")
                        .Child(fileName + userid)
                        .PutAsync(fileStream);
                        return videourl;
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
                        var videourl = await firebaseStorage
                        .Child("Videos")
                        .Child(fileName + Guid.NewGuid() + userid)
                        .PutAsync(fileStream);
                        return videourl;
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



        //upload uploaded images download url for easy retrieval later
        public static async Task<bool> UploadImageURL(string fileName, string downloadurl, Guid userID)
        {
            try
            {
                await firebaseDatabase
                .Child("ImageURLs")
                .PostAsync(new ImageObject()
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

        //upload uploaded videos download url for easy retrieval later
        public static async Task<bool> UploadVideoURL(string fileName, string downloadurl, Guid userID)
        {
            try
            {
                await firebaseDatabase
                .Child("VideoURLs")
                .PostAsync(new VideoObject()
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

        // Get image using file name
        public static async Task<string> GetImage(string fileName, Guid userid)
        {
            try
            {
                return await firebaseStorage
                .Child("Images")
                .Child(fileName + userid)
                .GetDownloadUrlAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        public static async Task<string> GetVideo(string fileName, Guid userid)
        {
            try
            {
                return await firebaseStorage
                .Child("Videos")
                .Child(fileName + userid)
                .GetDownloadUrlAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }

        //Get user's image objects
        public static async Task<List<ImageObject>> GetUsersImageObject(Guid userid)
        {
            var allImageObjects = await GetAllImageObjects();
            List<ImageObject> usersImageObjects = new List<ImageObject>();
            foreach (var a in allImageObjects)
            {
                if (a.Userid == userid)
                {
                    usersImageObjects.Add(a);
                }
            }
            return usersImageObjects;
        }

        //Get all image objects
        public static async Task<List<ImageObject>> GetAllImageObjects()
        {
            try
            {
                var listOfAllImageObjects = (await firebaseDatabase
                .Child("ImageURLs")
                .OnceAsync<ImageObject>()).Select(item =>
                new ImageObject
                {
                    FileName = item.Object.FileName,
                    Url = item.Object.Url,
                    Userid = item.Object.Userid
                }).ToList();
                return listOfAllImageObjects;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }



        //Get user's video objects
        public static async Task<List<VideoObject>> GetUsersVideoObject(Guid userid)
        {
            var allVideoObjects = await GetAllVideoObjects();
            List<VideoObject> usersVideoObjects = new List<VideoObject>();
            foreach (var a in allVideoObjects)
            {
                if (a.Userid == userid)
                {
                    usersVideoObjects.Add(a);
                }
            }
            return usersVideoObjects;
        }

        //Get all video objects
        public static async Task<List<VideoObject>> GetAllVideoObjects()
        {
            try
            {
                var listOfAllVideoObjects = (await firebaseDatabase
                .Child("VideoURLs")
                .OnceAsync<VideoObject>()).Select(item =>
                new VideoObject
                {
                    FileName = item.Object.FileName,
                    Url = item.Object.Url,
                    Userid = item.Object.Userid
                }).ToList();
                return listOfAllVideoObjects;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
                return null;
            }
        }





        // Delete image
        public static async Task DeleteImage(string fileName, Guid userid)
        {
            await firebaseStorage
                 .Child("Images")
                 .Child(fileName + userid)
                 .DeleteAsync();

        }

        // Delete video
        public static async Task DeleteVideo(string fileName, Guid userid)
        {
            await firebaseStorage
                 .Child("Videos")
                 .Child(fileName + userid)
                 .DeleteAsync();

        }

        //Delete Image Object
        public static async Task DeleteImageObject(string fileName, Guid userid)
        {
            try
            {
                var toDeleteObject = (await firebaseDatabase
                .Child("ImageURLs")
                .OnceAsync<ImageObject>()).Where(a => a.Object.FileName == fileName && a.Object.Userid == userid).FirstOrDefault();
                await firebaseDatabase.Child("ImageURLs").Child(toDeleteObject.Key).DeleteAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
            }

        }

        //Delete Video Object
        public static async Task DeleteVideoObject(string fileName, Guid userid)
        {
            try
            {
                var toDeleteObject = (await firebaseDatabase
                .Child("VideoURLs")
                .OnceAsync<VideoObject>()).Where(a => a.Object.FileName == fileName && a.Object.Userid == userid).FirstOrDefault();
                await firebaseDatabase.Child("VideoURLs").Child(toDeleteObject.Key).DeleteAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error:{e}");
            }

        }



        

    }
}
