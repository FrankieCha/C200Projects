DROP TABLE IF EXISTS SysUser;
DROP TABLE IF EXISTS DataRecord;

CREATE TABLE DataRecord (
   Field0      INT IDENTITY PRIMARY KEY,
   Field1      VARCHAR(40),
   Field2      VARCHAR(40),
   Field3      INT,
   Field4      FLOAT,
   Field5      BIT,
   Field6      DATETIME
);

INSERT INTO DataRecord(Field1,Field2,Field3,Field4,Field5,Field6) VALUES
('IronMan',       'Tony Stark',    123, 81.3,  'false', '2018-12-31T13:11:33'),
('CaptainMarvel', 'Carol Danvers', 456, 122.7, 'false', '2019-12-31T13:11:33'),
('Batman',        'Bruce Wayne',   123, 81.3,  'false', '2018-12-31T13:11:33'),
('WonderWoman',   'Diana Prince',  456, 122.7, 'false', '2019-12-31T13:11:33'),
('Superman',      'Clark Kent',    789, 452.3, 'true',  '2019-12-31T13:11:33');

CREATE TABLE SysUser
(
   UserId    VARCHAR(10) PRIMARY KEY,
   UserPw    VARBINARY(50) NOT NULL,
   FullName  VARCHAR(50) NOT NULL,
   Email     VARCHAR(50) NOT NULL,
   UserRole  VARCHAR(10) NOT NULL,
   LastLogin DATETIME NULL
);

INSERT INTO SysUser(UserId, UserPw, FullName, Email, UserRole) VALUES
('mary',  HASHBYTES('SHA1', 'secret0'),   'Mary Low',     'mary@mymovie.com',  'admin'),
('mark',  HASHBYTES('SHA1', 'password1'), 'Mark Anthony', 'marka@xyc.com',     'member'),
('scott', HASHBYTES('SHA1', 'password2'), 'Scott Tiger',  'stiger@netorg.com', 'user'),
('alvin', HASHBYTES('SHA1', 'secret2'),   'Alvin Chan',   'achan@qwerty.com',  'manager');
