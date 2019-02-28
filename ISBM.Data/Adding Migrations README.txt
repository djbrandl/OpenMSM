If you modify the model or the DbContext, ensure that you run the following in the Package Manager Console

*******************************************************************************
EntityFrameworkCore\Add-Migration ChangeNameGoesHere -Project ISBM.Data
Update-Database
*******************************************************************************