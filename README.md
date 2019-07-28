# Battleship
● Two players ● Each have a 10x10 board ● During setup, players can place an arbitrary
number of “battleships” on their board. The ships are 1-by-n sized, must fit entirely on the
board, and must be aligned either vertically or horizontally. ● During play, players take turn
“attacking” a single position on the opponent’s board, and the opponent must respond by
either reporting a “hit” on one of their battleships (if one occupies that position) or a “miss” ●
A battleship is sunk if it has been hit on all the squares it occupies ● A player wins if all of
their opponent’s battleships have been sunk.

## Solution structure
```
Battleship
│   README.md
│
└───src
   │   Battleship.sln
   │
   └───app
   |   └───Battleship
   |       │   ...
   |       │   ...
   |
   └───tests
        └───Battleship.IntegrationTests
        |  │   ...
        |  │   ...
        └───Battleship.UnitTests
           |   ...
           |   ...
```

## Implemented Functionalities
* Create Board
* Create Ship
* Place Ships on board
* Player to attack

## Swagger
Swagger doc gives information about api like available routes and its models and also it works like test harness.
You can run the swagger from 
* Local http://localhost:5001/swagger
* Server http://battleship-suresh.azurewebsites.net/swagger

## Health Check
You can run the health check from 
* Local http://localhost:5001/healthchecks-ui
* Server http://battleship-suresh.azurewebsites.net/healthchecks-ui

## Integration tests
The integration tests create a database for each test (the tests will remove the db afterwards as well).  The tests will either use the local database server if available (it will look for localhost on port 1433, so make sure you have TCP/IP enabled if you want to use this option) or if there is no database server installed it will use docker instead.  If you have docker installed these tests will run automatically without any interaction (although they may fail the first time if you don't have the required images, docker will download these for you, so just rerun the tests).  If you are going to be running these integration tests often, then you may prefer to keep the required docker image running as this will speed up the test execution (i.e. it won't need to spin up and tear down the image for each test run).  Use the following to run the docker image:

```bash
docker run -d --rm --name battleship-mssql -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=Password01!" -p 61234:1433 mcr.microsoft.com/mssql/server:2017-latest
```

