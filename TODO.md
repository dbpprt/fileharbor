
- all service methods with either update,insert should accept a tx as parameter
- wrap transactions in own struct to get rid of if commit == true
- implement switch for null logging
- add echo error handler which inserts all unhandled exceptions into database
- optimize all Exists methods in services to be more efficient