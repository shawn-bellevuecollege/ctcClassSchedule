CREATE ROLE [WebApplicationUser]
AUTHORIZATION [dbo]
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\JUAN-VM-WIN7$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'campus\julloa'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'campus\n216i-pt$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\SSOUTHVM1-N216K$'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'CAMPUS\tiis-schedulebetaqa'
GO
EXEC sp_addrolemember N'WebApplicationUser', N'ClassSchedule_WebUser'
GO