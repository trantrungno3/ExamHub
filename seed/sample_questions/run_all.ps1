param([Parameter(Mandatory=$true)][string]$Conn)
$ErrorActionPreference='Stop'
$files=@(
    "00_reset.sql",
    "00_topics.sql",
    "g01_MATH.sql",
    "g01_VIE.sql",
    "g02_MATH.sql",
    "g02_VIE.sql",
    "g03_MATH.sql",
    "g03_TNXH.sql",
    "g03_VIE.sql",
    "g04_MATH.sql",
    "g04_SCI.sql",
    "g04_VIE.sql",
    "g05_MATH.sql",
    "g05_SCI.sql",
    "g05_VIE.sql"
)
foreach ($f in $files) { Write-Host "Dang chay $f ..."; psql $Conn -v ON_ERROR_STOP=1 -f $f; if ($LASTEXITCODE -ne 0) { Write-Error "Loi khi chay $f"; exit 1 } }
Write-Host "Hoan tat!"