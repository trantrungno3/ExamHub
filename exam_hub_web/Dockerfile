# syntax=docker/dockerfile:1

# ── Build ────────────────────────────────────────────────────────────────────
# Node/pnpm chỉ tồn tại ở stage này. Image cuối không có Node runtime.
FROM node:24-alpine AS build
WORKDIR /src

# corepack lấy đúng pnpm khai báo trong package.json ("packageManager"), không pin lần thứ hai ở đây.
COPY package.json pnpm-lock.yaml pnpm-workspace.yaml ./
RUN corepack enable && pnpm install --frozen-lockfile

COPY . .

# Vite nhúng VITE_API_BASE vào bundle lúc build, nên đây là build-time arg chứ không phải env runtime.
# Đặt bằng chính origin công khai mà browser truy cập: nginx dưới đây proxy /api sang API, nhờ đó
# frontend và API cùng origin — điều kiện để cookie refresh SameSite=Lax được gửi kèm.
ARG VITE_API_BASE=http://localhost:8080
ENV VITE_API_BASE=$VITE_API_BASE
RUN pnpm build

# ── Runtime ──────────────────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS final

# Template trong /etc/nginx/templates được entrypoint của image chạy envsubst rồi ghi ra
# /etc/nginx/conf.d, nên API_ORIGIN đổi được lúc chạy mà không phải build lại image.
ENV API_ORIGIN=http://api:8080
COPY nginx.conf /etc/nginx/templates/default.conf.template
COPY --from=build /src/dist /usr/share/nginx/html

EXPOSE 80
