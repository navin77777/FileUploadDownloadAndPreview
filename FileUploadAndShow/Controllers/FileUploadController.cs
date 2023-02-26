using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.SqlClient;
using System.Configuration;
using FileUploadAndShow.Models;
using System.Data;

namespace FileUploadDemo.Controllers
{
    public class FileUploadController : Controller
    {
        string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        // GET: FileUpload
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
            {
                try
                {
                    Guid guid = Guid.NewGuid();
                    string guidStr = guid.ToString();

                    string uniqueId = guidStr.Substring(0, 8); 


                    string filename = Path.GetFileName(uniqueId + file.FileName);
                    string extension = Path.GetExtension(file.FileName);
                    string path = Path.Combine(Server.MapPath("~/Uploads"), filename);

                    file.SaveAs(path);

                    string savePath = "~/Content/Uploads/"+ filename;

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        SqlCommand command = new SqlCommand("[sp_InsertFile]", connection);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@FileName", filename);
                        command.Parameters.AddWithValue("@FilePath", savePath);
                        command.Parameters.AddWithValue("@FileExtension", extension);

                        connection.Open();
                        command.ExecuteNonQuery();
                        connection.Close();
                    }

                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error:" + ex.Message.ToString();
                }
            }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }

            return View("Index");
        }


        [HttpGet]
        public ActionResult GetFiles()
        {
            List<FileUploadModel> files = new List<FileUploadModel>();

            try
            {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("sp_GetAllFiles", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    FileUploadModel file = new FileUploadModel();
                    file.Id = Convert.ToInt32(reader["Id"]);
                    file.FileName = reader["FileName"].ToString();
                    file.FilePath = reader["FilePath"].ToString();
                    file.FileExtension = reader["FileExtension"].ToString();
                    file.FileExtension = reader["FileExtension"].ToString();
                    files.Add(file);
                }

                connection.Close();
            }

            } catch(Exception ex)
            {
                throw ex;
            }

            return View(files);
        }




        [HttpGet]
        public ActionResult PreviewFile(int Id)
        {
            FileUploadModel file = new FileUploadModel();

            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand("sp_GetFileById", connection);
                command.CommandType = System.Data.CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@Id", Id);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    file.Id = Convert.ToInt32(reader["Id"]);
                    file.FileName = reader["FileName"].ToString();
                    //file.FilePath = reader["~/" + file.FileName].ToString();

                    //file.FilePath = Path.Combine(Server.MapPath("~/Uploads"), file.FileName);
                    file.FilePath = Path.Combine("~/Uploads", file.FileName);


                    file.FileExtension = reader["FileExtension"].ToString();
                    file.FileExtension = reader["FileExtension"].ToString();
                    
                    
                }

                connection.Close();
            }

            return View(file);
        }





        public ActionResult DownloadFile(int id)
        {
            byte[] fileBytes;
            string fileName, filePath;


            string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_GetFileById", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Id", id);
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            fileName = reader["FileName"].ToString();
                            filePath = reader["FilePath"].ToString();
                            fileBytes = System.IO.File.ReadAllBytes(filePath);

                            //fileBytes = (byte[])reader["FileData"];
                            //fileName = reader["FileName"].ToString();
                            //contentType = reader["ContentType"].ToString();
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }
            }

            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, fileName);
        }


     }
    }








//CREATE TABLE[dbo].[Files]
//(

//   [ID] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,

//   [FileName] NVARCHAR(100) NOT NULL,

//   [FilePath] NVARCHAR(200) NOT NULL,

//   [FileExtension] NVARCHAR(10) NOT NULL,

//   [UploadDate] DATETIME NOT NULL DEFAULT GETDATE()
//)

//use Learning

//CREATE PROCEDURE [dbo].[sp_InsertFile]
//@FileName NVARCHAR(100),
//    @FilePath NVARCHAR(200),
//    @FileExtension NVARCHAR(10)
//AS
//BEGIN
//    INSERT INTO [dbo].[Files]
//([FileName], [FilePath], [FileExtension])
//    VALUES
//        (@FileName, @FilePath, @FileExtension)
//END


//select *from [dbo].[Files]

//use Learning

//CREATE PROCEDURE sp_GetAllFiles
//AS
//BEGIN
//    SET NOCOUNT ON;

//SELECT Id, FileName, FilePath, FileExtension, UploadDate
//    FROM Files
    
//    SET NOCOUNT OFF;
//END


//CREATE PROCEDURE sp_GetFileById
//    @Id INT
//AS
//BEGIN
//    SET NOCOUNT ON;

//SELECT* FROM Files WHERE ID = @Id;

//SET NOCOUNT OFF;
//END

