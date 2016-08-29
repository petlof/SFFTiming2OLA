select firstName, dbRuns.RaceTime,dbRUns.Status,
(select Firstname from dbRelay where RaceID = 3 and Leg = 1 and NameID = dbName.ID) as firstName1,
(select lastName from dbRelay where RaceID = 3 and Leg = 1 and NameID = dbName.ID) as lastName1,
(select Fiscode from dbRelay where RaceID = 3 and Leg = 1 and NameID = dbName.ID) as iofId1,
(select Firstname from dbRelay where RaceID = 3 and Leg = 2 and NameID = dbName.ID) as firstName2,
(select lastName from dbRelay where RaceID = 3 and Leg = 2 and NameID = dbName.ID) as lastName2,
(select Fiscode from dbRelay where RaceID = 3 and Leg = 2 and NameID = dbName.ID) as iofId2,
(select Firstname from dbRelay where RaceID = 3 and Leg = 3 and NameID = dbName.ID) as firstName3,
(select lastName from dbRelay where RaceID = 3 and Leg = 3 and NameID = dbName.ID) as lastName3,
(select Fiscode from dbRelay where RaceID = 3 and Leg = 3 and NameID = dbName.ID) as iofId3,
(select Firstname from dbRelay where RaceID = 3 and Leg = 4 and NameID = dbName.ID) as firstName4,
(select lastName from dbRelay where RaceID = 3 and Leg = 4 and NameID = dbName.ID) as lastName4,
(select Fiscode from dbRelay where RaceID = 3 and Leg = 4 and NameID = dbName.ID) as iofId4
from dbName, dbRuns  where dbName.raceId=3 and dbRuns.RaceID=3 and dbRuns.NameId = dbName.ID
order by status desc, RaceTime
