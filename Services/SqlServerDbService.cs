using Cw5.DTOs.Requests;
using Cw5.DTOs.Responses;
using Cw5.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Cw5.Services
{
    public class SqlServerDbService : IStudentsDbService
    {
        private const string ConString = "Data Source=win-3alsm9qls2n;Initial Catalog=master;Integrated Security=True";
        readonly IPasswordService passwordService;

        public SqlServerDbService(IPasswordService password)
        {
            this.passwordService = password;
        }
        public PromoteStudentResponse PromoteStudent(PromoteStudentRequest request)
        {
            using SqlConnection con = new SqlConnection(ConString);
            using SqlCommand com = new SqlCommand();
            try
            {
                com.Connection = con;
                com.CommandText = "select * from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where Name=@StudyName and Semester=@Semester";
                com.Parameters.AddWithValue("@StudyName", request.Studies);
                com.Parameters.AddWithValue("@Semester", request.Semester);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                reader.Close();
                com.Parameters.Clear();
                com.CommandText = "PromoteStudents";
                com.CommandType = CommandType.StoredProcedure;
                com.Parameters.AddWithValue("@Study", request.Studies);
                com.Parameters.AddWithValue("@Semester", request.Semester);
                reader = com.ExecuteReader();
                if (reader.Read())
                {
                    PromoteStudentResponse response = new PromoteStudentResponse
                    {
                        IdEnrollment = int.Parse(reader["IdEnrollment"].ToString()),
                        Semester = int.Parse(reader["Semester"].ToString()),
                        Study = reader["Name"].ToString(),
                        StartDate = DateTime.Parse(reader["StartDate"].ToString())
                    };
                    reader.Close();
                    return response;
                }
                else
                {
                    return null;
                }
            }
            catch (SqlException)
            {
                return null;
            }
        }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {

            using SqlConnection con = new SqlConnection(ConString);
            using SqlCommand com = new SqlCommand();
            try
            {
                com.Connection = con;
                com.CommandText = "Select * from Student where IndexNumber=@index";
                com.Parameters.AddWithValue("@index", request.IndexNumber);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                if (reader.Read())
                    return null;

                reader.Close();
                com.CommandText = "select IdStudy from Studies where Name=@StudyName";
                com.Parameters.AddWithValue("@StudyName", request.Studies);

                reader = com.ExecuteReader();
                if (!reader.Read())
                {
                    return null;
                }
                int StudyId = int.Parse(reader["IdStudy"].ToString());

                reader.Close();
                com.CommandText = "Select IdEnrollment from Enrollment where StartDate=(select Max(StartDate) from Enrollment where IdStudy=@id and Semester=1) and IdStudy=@id and Semester=1";
                com.Parameters.AddWithValue("@id", StudyId);
                bool dataPresent = false;
                int IdEnrollment = 0;
                reader = com.ExecuteReader();
                if (reader.Read())
                {
                    dataPresent = true;
                    IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                }
                reader.Close();
                SqlTransaction transaction = con.BeginTransaction();
                try
                {

                    com.Transaction = transaction;

                    EnrollStudentResponse response = new EnrollStudentResponse();
                    if (dataPresent)
                    {
                        com.CommandText = "Insert into Student Values(@IndexNumber,@FirstName,@LastName,@BirthDate,@IdEnrollment,@StudentPassword,@Salt,NULL)";
                        com.Parameters.AddWithValue("@IndexNumber", request.IndexNumber);
                        com.Parameters.AddWithValue("@FirstName", request.FirstName);
                        com.Parameters.AddWithValue("@LastName", request.LastName);
                        com.Parameters.AddWithValue("@BirthDate", request.Birthdate);
                        com.Parameters.AddWithValue("@IdEnrollment", IdEnrollment);
                        String salt = passwordService.CreateSalt();
                        com.Parameters.AddWithValue("@StudentPassword", passwordService.HashPassword(request.Password, salt));
                        com.Parameters.AddWithValue("@Salt", salt);
                        com.ExecuteNonQuery();

                        com.Parameters.Clear();
                        com.CommandText = "select IdEnrollment,Semester,StartDate,Name from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where IdEnrollment=@IdEnrollment";
                        com.Parameters.AddWithValue("@IdEnrollment", IdEnrollment);
                        reader = com.ExecuteReader();
                        if (reader.Read())
                        {
                            response.IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                            response.Semester = int.Parse(reader["Semester"].ToString());
                            response.Study = reader["Name"].ToString();
                            response.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                            reader.Close();
                            transaction.Commit();
                            transaction.Dispose();
                            return response;
                        }
                        return null;
                    }
                    else
                    {
                        reader.Close();
                        com.CommandText = "Insert into Enrollment Values((Select ISNULL(Max(IdEnrollment),0)+1 from Enrollment),1,@IdStudy,(SELECT CONVERT(date, getdate())))";
                        com.Parameters.AddWithValue("@IdStudy", StudyId);
                        if (com.ExecuteNonQuery() == 1)
                        {
                            com.CommandText = "Insert into Student Values(@IndexNumber,@FirstName,@LastName,@BirthDate,(Select Max(IdEnrollment) from Enrollment),@StudentPassword,@Salt,NULL)";
                            com.Parameters.AddWithValue("@IndexNumber", request.IndexNumber);
                            com.Parameters.AddWithValue("@FirstName", request.FirstName);
                            com.Parameters.AddWithValue("@LastName", request.LastName);
                            com.Parameters.AddWithValue("@BirthDate", request.Birthdate);
                            String salt = passwordService.CreateSalt();
                            com.Parameters.AddWithValue("@StudentPassword", passwordService.HashPassword(request.Password, salt));
                            com.Parameters.AddWithValue("@Salt", salt);
                            com.ExecuteNonQuery();

                            com.CommandText = "select IdEnrollment,Semester,StartDate,Name from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where IdEnrollment=(Select MAX(IdEnrollment) from Enrollment)";
                            reader = com.ExecuteReader();
                            if (reader.Read())
                            {
                                response.IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                                response.Semester = int.Parse(reader["Semester"].ToString());
                                response.Study = reader["Name"].ToString();
                                response.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                                reader.Close();
                                transaction.Commit();
                                transaction.Dispose();
                                return response;
                            }
                            return null;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    transaction.Dispose();
                    return null;
                }
            }
            catch (SqlException)
            {
                return null;
            }
        }
        public bool Check(String index)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "Select * from Student where IndexNumber=@index";
                com.Parameters.AddWithValue("@index", index);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                if (reader.Read())
                    return false;

                reader.Close();
            }
            return true;
        }

        public bool Login(string Index, string Password)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "Select * from Student where IndexNumber=@index and Password = @pass";
                com.Parameters.AddWithValue("@index", Index);
                com.Parameters.AddWithValue("@pass",Password);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                if (reader.Read())
                    return false;

                reader.Close();
            }
            return true;
        }

        public void SetRefreshToken(string IndexNumber, string refresh)
        {
            try
            {
                using SqlConnection con = new SqlConnection(ConString);
                using SqlCommand com = new SqlCommand
                {
                    Connection = con,
                    CommandText = "Update Student Set RefreshToken=@Token where IndexNumber=@IndexNumber"
                };
                com.Parameters.AddWithValue("@Token", refresh);
                com.Parameters.AddWithValue("@IndexNumber", IndexNumber);
                con.Open();
                com.ExecuteNonQuery();
            }
            catch (Exception)
            { }
        }

        public string GetRefreshTokenOwner(string refreshToken)
        {
            try
            {
                using SqlConnection con = new SqlConnection(ConString);
                using SqlCommand com = new SqlCommand
                {
                    Connection = con,
                    CommandText = "Select IndexNumber from Student where RefreshToken=@Token"
                };
                com.Parameters.AddWithValue("@Token", refreshToken);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                if (reader.Read())
                {
                    string res = reader["IndexNumber"].ToString();
                    reader.Close();
                    return res;
                }
                reader.Close();
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public PasswordDetails GetStudentPasswordData(string IndexNumber)
        {
            PasswordDetails response = new PasswordDetails();
            try
            {
                using SqlConnection con = new SqlConnection(ConString);
                using SqlCommand com = new SqlCommand
                {
                    Connection = con,
                    CommandText = "Select Password,Salt from Student where IndexNumber=@index"
                };
                com.Parameters.AddWithValue("@index", IndexNumber);
                con.Open();
                SqlDataReader reader = com.ExecuteReader();

                if (reader.Read())
                {
                    response.Password = reader["StudentPassword"].ToString().Trim();
                    response.Salt = reader["Salt"].ToString().Trim();
                    reader.Close();
                    return response;
                }
                reader.Close();
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
