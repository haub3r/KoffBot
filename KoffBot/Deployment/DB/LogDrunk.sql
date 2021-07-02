BEGIN TRANSACTION
SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
COMMIT
BEGIN TRANSACTION
GO
CREATE TABLE dbo.LogDrunk
	(
	Id int IDENTITY(1,1) NOT NULL,
	ModifiedBy varchar(50) NOT NULL,
	Modified datetime2(7) NOT NULL,
	CreatedBy varchar(50) NOT NULL,
	Created datetime2(7) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.LogDrunk ADD CONSTRAINT
	PK_LogDrunk PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.LogDrunk SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
