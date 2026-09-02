/* ============================================================
   RaceDay Database Schema
   Part 1 - Section C
   Target: Microsoft SQL Server (SSMS 22)
   Matches ERD.png exactly - 6 entities:
   Users, Venues, Events, Categories, Enrolments, Results
   ============================================================ */
/* ============================================================
   DATA TYPE NOTES
   - VARCHAR (not NVARCHAR) is used throughout, since the system
     currently only needs to support English/Latin-script text.
   - DECIMAL is used for money (EntryFee) and distance (DistanceKM)
     instead of FLOAT, because DECIMAL stores exact values with no
     rounding errors - important for money and race distances.
   ============================================================ */
IF DB_ID('RaceDayDB') IS NULL
BEGIN
    CREATE DATABASE RaceDayDB;
END
GO

USE RaceDayDB;
GO

/* Drop tables if they already exist, child tables first,
   so the script can be re-run cleanly on a fresh instance. */
IF OBJECT_ID('dbo.Results', 'U') IS NOT NULL DROP TABLE dbo.Results;
IF OBJECT_ID('dbo.Enrolments', 'U') IS NOT NULL DROP TABLE dbo.Enrolments;
IF OBJECT_ID('dbo.Categories', 'U') IS NOT NULL DROP TABLE dbo.Categories;
IF OBJECT_ID('dbo.Events', 'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Venues', 'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
GO

/* ============================================================
   TABLE: Users
   Holds both Organisers and Participants, distinguished by Role.
   ============================================================ */
CREATE TABLE dbo.Users (
    UserID          INT IDENTITY(1,1)   PRIMARY KEY,
    FullName        VARCHAR(100)        NOT NULL,
    Email           VARCHAR(150)        NOT NULL UNIQUE,
    PasswordHash    VARCHAR(255)        NOT NULL,
    Role            VARCHAR(20)         NOT NULL DEFAULT 'Participant',
    CreatedAt       DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Organiser', 'Participant'))
);
GO

/* ============================================================
   TABLE: Venues
   Physical locations where events are held, used to pull
   live weather and route information on race day.
   ============================================================ */
CREATE TABLE dbo.Venues (
    VenueID         INT IDENTITY(1,1)   PRIMARY KEY,
    VenueName       VARCHAR(150)        NOT NULL,
    City            VARCHAR(100)        NOT NULL,
    Province        VARCHAR(100)        NOT NULL,
    Latitude        DECIMAL(9,6)        NULL,
    Longitude       DECIMAL(9,6)        NULL
);
GO

/* ============================================================
   TABLE: Events
   Created and managed by an Organiser, hosted at a Venue.
   ============================================================ */
CREATE TABLE dbo.Events (
    EventID         INT IDENTITY(1,1)   PRIMARY KEY,
    EventName       VARCHAR(150)        NOT NULL,
    Description     VARCHAR(500)        NULL,
    EventDate       DATETIME            NOT NULL,
    VenueID         INT                 NOT NULL,
    OrganiserID     INT                 NOT NULL,
    CreatedAt       DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Events_Venue     FOREIGN KEY (VenueID)     REFERENCES dbo.Venues(VenueID),
    CONSTRAINT FK_Events_Organiser FOREIGN KEY (OrganiserID) REFERENCES dbo.Users(UserID)
);
GO

/* ============================================================
   TABLE: Categories
   The distances/classes on offer for a specific Event
   (e.g. 5km Fun Run, 10km, Half Marathon).
   ============================================================ */
CREATE TABLE dbo.Categories (
    CategoryID      INT IDENTITY(1,1)   PRIMARY KEY,
    EventID         INT                 NOT NULL,
    CategoryName    VARCHAR(100)        NOT NULL,
    DistanceKM      DECIMAL(6,2)        NOT NULL,
    MaxParticipants INT                 NOT NULL DEFAULT 500,
    EntryFee        DECIMAL(8,2)        NOT NULL DEFAULT 0,
    CONSTRAINT FK_Categories_Event FOREIGN KEY (EventID) REFERENCES dbo.Events(EventID)
        ON DELETE CASCADE
);
GO

/* ============================================================
   TABLE: Enrolments
   Links a Participant (User) to a Category they have entered.
   ============================================================ */
CREATE TABLE dbo.Enrolments (
    EnrolmentID     INT IDENTITY(1,1)   PRIMARY KEY,
    ParticipantID   INT                 NOT NULL,
    CategoryID      INT                 NOT NULL,
    EnrolmentDate   DATETIME            NOT NULL DEFAULT GETDATE(),
    Status          VARCHAR(20)         NOT NULL DEFAULT 'Confirmed',
    CONSTRAINT FK_Enrolments_Participant FOREIGN KEY (ParticipantID) REFERENCES dbo.Users(UserID),
    CONSTRAINT FK_Enrolments_Category    FOREIGN KEY (CategoryID)    REFERENCES dbo.Categories(CategoryID),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Pending', 'Confirmed', 'Cancelled')),
    CONSTRAINT UQ_Enrolments_Participant_Category UNIQUE (ParticipantID, CategoryID)
);
GO

/* ============================================================
   TABLE: Results
   One result per Enrolment (one-to-one), captured after
   the event by the Organiser.
   ============================================================ */
CREATE TABLE dbo.Results (
    ResultID        INT IDENTITY(1,1)   PRIMARY KEY,
    EnrolmentID     INT                 NOT NULL UNIQUE,
    FinishTime      TIME                NULL,
    Position        INT                 NULL,
    Status          VARCHAR(20)         NOT NULL DEFAULT 'Finished',
    RecordedAt      DATETIME            NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Results_Enrolment FOREIGN KEY (EnrolmentID) REFERENCES dbo.Enrolments(EnrolmentID)
        ON DELETE CASCADE,
    CONSTRAINT CK_Results_Status CHECK (Status IN ('Finished', 'DNF', 'DNS'))
);
GO

/* ============================================================
   SEED DATA
   2 Organisers, 2 Participants, 3 Events, categories per
   event, and sample enrolments/results.
   ============================================================ */
/* ============================================================
   INDEXES
   Improve lookup performance on columns that are searched or
   filtered frequently by the application.
   ============================================================ */

-- Speeds up login lookups (WHERE Email = ...)
CREATE INDEX IX_Users_Email ON dbo.Users(Email);

-- Speeds up "browse upcoming events" queries (WHERE EventDate > ...)
CREATE INDEX IX_Events_EventDate ON dbo.Events(EventDate);
GO
-- Users: Organisers and Participants
INSERT INTO dbo.Users (FullName, Email, PasswordHash, Role) VALUES
('Thandeka Nkosi',  'thandeka.nkosi@raceday.co.za',  'hashed_pw_001', 'Organiser'),
('Johan van der Merwe', 'johan.vdm@raceday.co.za',   'hashed_pw_002', 'Organiser'),
('Smisokuhle Dube', 'smiso.dube@student.co.za',      'hashed_pw_003', 'Participant'),
('Aiden Pillay',    'aiden.pillay@gmail.com',        'hashed_pw_004', 'Participant');
GO

-- Venues
INSERT INTO dbo.Venues (VenueName, City, Province, Latitude, Longitude) VALUES
('Sahara Kingsmead Stadium',    'Durban',      'KwaZulu-Natal', -29.8231, 31.0186),
('Green Point Athletics Track', 'Cape Town',   'Western Cape',  -33.9036, 18.4103),
('FNB Stadium Precinct',        'Soweto',      'Gauteng',       -26.2349, 27.9821);
GO

-- Events
INSERT INTO dbo.Events (EventName, Description, EventDate, VenueID, OrganiserID) VALUES
('Comrades Marathon',        'Iconic ultramarathon between Pietermaritzburg and Durban.', '2027-06-13 05:30:00', 1, 1),
('Cape Town Cycle Tour',     'Scenic road cycling event around the Cape Peninsula.',      '2027-03-08 06:00:00', 2, 2),
('Soweto Marathon',          'Community road running event through the streets of Soweto.', '2027-11-07 06:00:00', 3, 1);
GO

-- Categories (per event)
INSERT INTO dbo.Categories (EventID, CategoryName, DistanceKM, MaxParticipants, EntryFee) VALUES
(1, 'Down Run - Full Ultra', 87.00, 20000, 950.00),
(1, '10km Charity Fun Run',  10.00, 2000,  150.00),
(2, 'Full Cycle Tour',       109.00, 35000, 650.00),
(2, 'Mini Cycle Tour',       47.00,  5000,  350.00),
(3, 'Full Marathon',         42.20, 8000,  400.00),
(3, '10km Fun Run',          10.00, 3000,  120.00);
GO

-- Enrolments (Participants entering Categories)
INSERT INTO dbo.Enrolments (ParticipantID, CategoryID, Status) VALUES
(3, 1, 'Confirmed'),  -- Smisokuhle - Comrades Down Run
(3, 5, 'Confirmed'),  -- Smisokuhle - Soweto Full Marathon
(4, 3, 'Confirmed'),  -- Aiden - Cape Town Full Cycle Tour
(4, 6, 'Pending');    -- Aiden - Soweto 10km Fun Run
GO

-- Results (one per completed Enrolment)
INSERT INTO dbo.Results (EnrolmentID, FinishTime, Position, Status) VALUES
(1, '08:42:15', 3450, 'Finished'),
(3, '05:12:40', 812,  'Finished');
GO

/* ============================================================
   VERIFICATION QUERIES (optional - for manual checking in SSMS)
   ============================================================ */
-- SELECT * FROM dbo.Users;
-- SELECT * FROM dbo.Venues;
-- SELECT * FROM dbo.Events;
-- SELECT * FROM dbo.Categories;
-- SELECT * FROM dbo.Enrolments;
-- SELECT * FROM dbo.Results;
