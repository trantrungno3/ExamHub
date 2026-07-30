#!/bin/bash
# Chạy toàn bộ theo đúng thứ tự bằng psql (bao gồm reset dữ liệu mẫu cũ)
set -e
psql "$1" -v ON_ERROR_STOP=1 -f "00_reset.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "00_topics.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g01_MATH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g01_VIE.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g02_MATH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g02_VIE.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g03_MATH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g03_TNXH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g03_VIE.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g04_MATH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g04_SCI.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g04_VIE.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g05_MATH.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g05_SCI.sql"
psql "$1" -v ON_ERROR_STOP=1 -f "g05_VIE.sql"