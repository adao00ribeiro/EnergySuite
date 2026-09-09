# Multi-stage build for Angular App Shell
FROM node:22-alpine AS build
WORKDIR /app

# Install dependencies com cache de pacotes npm
COPY package.json package-lock.json .npmrc ./
RUN --mount=type=cache,id=npm,target=/root/.npm \
    npm config set fetch-retries 5 \
    && npm config set fetch-retry-mintimeout 20000 \
    && npm config set fetch-retry-maxtimeout 120000 \
    && npm ci --legacy-peer-deps --maxsockets 5 --fetch-timeout=600000

# Copy source and build
COPY . .
RUN --mount=type=cache,target=/app/.angular/cache npm run build --configuration=production

# Final image using Nginx
FROM nginx:alpine
# Copy nginx configuration with reverse proxy rules for backend services and MFEs
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts to nginx public folder
COPY --from=build /app/dist/app-shell/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
