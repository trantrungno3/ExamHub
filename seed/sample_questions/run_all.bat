@echo off
REM Chay: run_all.bat "postgresql://postgres:secret@localhost:5432/examhub"
set CONN=%1
if "%CONN%"=="" ( echo Vui long truyen connection string. & exit /b 1 )
echo Dang chay 00_reset.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "00_reset.sql"
if errorlevel 1 ( echo LOI khi chay 00_reset.sql & exit /b 1 )
echo Dang chay 00_topics.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "00_topics.sql"
if errorlevel 1 ( echo LOI khi chay 00_topics.sql & exit /b 1 )
echo Dang chay g01_MATH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g01_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g01_MATH.sql & exit /b 1 )
echo Dang chay g01_VIE.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g01_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g01_VIE.sql & exit /b 1 )
echo Dang chay g02_MATH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g02_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g02_MATH.sql & exit /b 1 )
echo Dang chay g02_VIE.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g02_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g02_VIE.sql & exit /b 1 )
echo Dang chay g03_MATH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g03_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g03_MATH.sql & exit /b 1 )
echo Dang chay g03_TNXH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g03_TNXH.sql"
if errorlevel 1 ( echo LOI khi chay g03_TNXH.sql & exit /b 1 )
echo Dang chay g03_VIE.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g03_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g03_VIE.sql & exit /b 1 )
echo Dang chay g04_MATH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g04_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g04_MATH.sql & exit /b 1 )
echo Dang chay g04_SCI.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g04_SCI.sql"
if errorlevel 1 ( echo LOI khi chay g04_SCI.sql & exit /b 1 )
echo Dang chay g04_VIE.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g04_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g04_VIE.sql & exit /b 1 )
echo Dang chay g05_MATH.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g05_MATH.sql"
if errorlevel 1 ( echo LOI khi chay g05_MATH.sql & exit /b 1 )
echo Dang chay g05_SCI.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g05_SCI.sql"
if errorlevel 1 ( echo LOI khi chay g05_SCI.sql & exit /b 1 )
echo Dang chay g05_VIE.sql ...
psql "%CONN%" -v ON_ERROR_STOP=1 -f "g05_VIE.sql"
if errorlevel 1 ( echo LOI khi chay g05_VIE.sql & exit /b 1 )
echo Hoan tat!