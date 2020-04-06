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
    
        public PromoteStudentResponse PromoteStudent(PromoteStudentRequest request)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
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
                    if (reader.Read()) {
                        PromoteStudentResponse response = new PromoteStudentResponse();
                        response.IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                        response.Semester= int.Parse(reader["Semester"].ToString());
                        response.Study = reader["Name"].ToString();
                        response.StartDate = DateTime.Parse(reader["StartDate"].ToString());
                        reader.Close();
                        return response;
                    }
                    else
                    {
                        return null;
                    }
                }
                catch(SqlException ex)
                {
                    return null;
                }
            }
         }

        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
        {

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
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
                    if (!reader.Read()) {
                        return null;
                    }
                    int StudyId = int.Parse(reader["IdStudy"].ToString());

                    reader.Close();
                    com.CommandText = "Select IdEnrollment from Enrollment where StartDate=(select Max(StartDate) from Enrollment where IdStudy=@id and Semester=1) and IdStudy=@id and Semester=1";
                    com.Parameters.AddWithValue("@id", StudyId);
                    bool dataPresent = false;
                    int IdEnrollment=0;
                    reader = com.ExecuteReader();
                    if (reader.Read())
                    {
                        dataPresent = true;
                        IdEnrollment = int.Parse(reader["IdEnrollment"].ToString());
                    }
                    reader.Close();
                    SqlTransaction transaction= con.BeginTransaction();
                try
                {
                    
                        com.Transaction = transaction;
                        
                        EnrollStudentResponse response = new EnrollStudentResponse();
                        if (dataPresent)  
                        {
                            com.CommandText = "Insert into Student Values(@IndexNumber,@FirstName,@LastName,@BirthDate,@IdEnrollment)";
                            com.Parameters.AddWithValue("@IndexNumber", request.IndexNumber);
                            com.Parameters.AddWithValue("@FirstName", request.FirstName);
                            com.Parameters.AddWithValue("@LastName", request.LastName);
                            com.Parameters.AddWithValue("@BirthDate", request.Birthdate);
                            com.Parameters.AddWithValue("@IdEnrollment", IdEnrollment);
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
                                com.CommandText = "Insert into Student Values(@IndexNumber,@FirstName,@LastName,@BirthDate,(Select Max(IdEnrollment) from Enrollment))";
                                com.Parameters.AddWithValue("@IndexNumber", request.IndexNumber);
                                com.Parameters.AddWithValue("@FirstName", request.FirstName);
                                com.Parameters.AddWithValue("@LastName", request.LastName);
                                com.Parameters.AddWithValue("@BirthDate", request.Birthdate);
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
                            else {
                                return null;
                            }
                        }
                    }
                    catch (Exception ex2) {
                        transaction.Rollback();
                        transaction.Dispose();
                        return null;
                    }
                }
                catch (SqlException ex) {
                    return null;
                }
            }            
        }
    }
}
