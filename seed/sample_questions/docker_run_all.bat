@echo off
set SERVICE=postgres
set DB=examhub
set DBUSER=postgres
echo Dang chay 00_reset.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "00_reset.sql"
if errorlevel 1 ( echo LOI khi chay 00_reset.sql & exit /b 1 )
echo Dang chay 00_topics.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "00_topics.sql"
if errorlevel 1 ( echo LOI khi chay 00_topics.sql & exit /b 1 )
echo Dang chay g01_MATH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g01_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g01_MATH.sql & exit /b 1 )
echo Dang chay g01_VIE.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g01_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g01_VIE.sql & exit /b 1 )
echo Dang chay g02_MATH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g02_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g02_MATH.sql & exit /b 1 )
echo Dang chay g02_VIE.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g02_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g02_VIE.sql & exit /b 1 )
echo Dang chay g03_MATH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g03_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g03_MATH.sql & exit /b 1 )
echo Dang chay g03_TNXH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g03_TNXH.sql"
if errorlevel 1 ( echo LOI khi chay g03_TNXH.sql & exit /b 1 )
echo Dang chay g03_VIE.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g03_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g03_VIE.sql & exit /b 1 )
echo Dang chay g04_MATH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g04_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g04_MATH.sql & exit /b 1 )
echo Dang chay g04_SCI.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g04_SCI.sql"
if errorlevel 1 ( echo LOI khi chay g04_SCI.sql & exit /b 1 )
echo Dang chay g04_VIE.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g04_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g04_VIE.sql & exit /b 1 )
echo Dang chay g05_MATH.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g05_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g05_MATH.sql & exit /b 1 )
echo Dang chay g05_SCI.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g05_SCI.sql"
if errorlevel 1 ( echo LOI khi chay g05_SCI.sql & exit /b 1 )
echo Dang chay g05_VIE.sql ...
docker compose exec -T %SERVICE% psql -U %DBUSER% -d %DB% -v ON_ERROR_STOP=1 < "g05_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g05_VIE.sql & exit /b 1 )
echo Hoan tat!