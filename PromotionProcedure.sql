go;
alter procedure PromoteStudents (@Study nvarchar(100),@Semester int)
as
begin
	Declare @CurrentEnrollmentId int,@NeededEnrollmentId int;
	Select @CurrentEnrollmentId=IdEnrollment from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where Semester=@Semester and Name=@Study;
	select @NeededEnrollmentId=IdEnrollment from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where Semester=@Semester+1 and Name=@Study
	If @NeededEnrollmentId is Not Null
	begin
		Update Student set IdEnrollment=@NeededEnrollmentId where IdEnrollment=@CurrentEnrollmentId
		Select IdEnrollment,Name,Semester,StartDate from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where IdEnrollment=@NeededEnrollmentId
	end
	else
	begin
	Declare @NewEnrollId int,@CurrentStudyId int;
		Select @CurrentStudyId=IdStudy from Studies where Name=@Study;
		Select @NewEnrollId=IsNull(MAX(IdEnrollment),0)+1 from Enrollment;
		Insert into Enrollment Values(@NewEnrollId,@Semester+1,@CurrentStudyId,(SELECT CONVERT(date, getdate())));
		Update Student set IdEnrollment=@NewEnrollId where IdEnrollment=@CurrentEnrollmentId;
		Select IdEnrollment,Name,Semester,StartDate from Enrollment inner join Studies on Enrollment.IdStudy=Studies.IdStudy where IdEnrollment=@NewEnrollId;
	end
end