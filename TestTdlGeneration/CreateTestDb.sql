﻿CREATE DATABASE TdlTest
GO

USE TdlTest
GO
ALTER DATABASE TdlTest SET PAGE_VERIFY CHECKSUM
ALTER DATABASE TdlTest SET AUTO_CREATE_STATISTICS ON (INCREMENTAL = ON)
ALTER DATABASE TdlTest SET AUTO_UPDATE_STATISTICS_ASYNC ON
ALTER DATABASE TdlTest SET ALLOW_SNAPSHOT_ISOLATION ON
ALTER DATABASE TdlTest SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE
GO

CREATE TABLE GlobalInts(
	ParameterName varchar( 50 )
		NOT NULL
		CONSTRAINT GlobalIntsPk PRIMARY KEY,
	ParameterValue int
		NOT NULL
)

INSERT INTO GlobalInts VALUES( 'LineMarker', 0 )
GO

CREATE SEQUENCE PrimarySequence AS int start with 1;
GO

CREATE TABLE States (
	StateId int NOT NULL CONSTRAINT pkStates PRIMARY KEY,
	StateName nvarchar( 50 ) NOT NULL CONSTRAINT uniqueStateName UNIQUE,
	Abbreviation nvarchar( 2 ) NOT NULL CONSTRAINT uniqueStateAbbr UNIQUE
)
GO
INSERT INTO States VALUES( 1, 'Alabama', 'AL' );
INSERT INTO States VALUES( 2, 'Alaska', 'AK' );
INSERT INTO States VALUES( 3, 'Arizona', 'AZ' );
INSERT INTO States VALUES( 4, 'Arkansas', 'AR' );
INSERT INTO States VALUES( 4000, 'New York', 'NY' );
GO