# syntax=docker/dockerfile:1

# ExamHub.API và ExamHub.Core có 7 ProjectReference dạng `..\..\..\trungtv_core\...`, tức trỏ ra
# NGOÀI cây repo này. Nên build context không thể chỉ là thư mục project.
#
# Cách giải ở đây: context chính vẫn nằm trong repo (exam_hub_api), còn repo sibling vào bằng một
# named additional context. Nhờ vậy không phải đặt context ở thư mục cha (sẽ kéo theo mọi thứ nằm
# cạnh hai repo và không đặt được .dockerignore vì nó ở ngoài repo).
#
# Layout trong image phải giữ đúng độ sâu tương đối để 7 ProjectReference kia resolve được:
#   /src/ExamHub/exam_hub_api/ExamHub.API  +  ..\..\..\  ->  /src/trungtv_core   ✔
#
# Layout sibling này chính là layout CI đang dùng (checkout vào `ExamHub/` và `trungtv_core/`).

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Repo sibling; đổi đường dẫn thật ở compose (`additional_contexts`), không sửa Dockerfile.
COPY --from=trungtv_core . /src/trungtv_core

# Context chính là exam_hub_api/
COPY . /src/ExamHub/exam_hub_api

WORKDIR /src/ExamHub/exam_hub_api/ExamHub.API
# Không tách restore thành layer riêng: restore cần cả 8 csproj (7 của trungtv_core + Core), copy
# lẻ từng file để cache chỉ đổi lấy một đoạn Dockerfile dài mà vẫn invalidate khi bất kỳ csproj đổi.
RUN dotnet publish "./ExamHub.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

# curl chỉ để Compose healthcheck gọi /health/ready — image aspnet không có curl/wget/nc.
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .

# Chạy bằng user không phải root (image aspnet đã tạo sẵn user này).
USER $APP_UID

# Kestrel trong container chỉ nghe HTTP; TLS do reverse proxy phía trước đảm nhiệm.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ExamHub.API.dll"]
