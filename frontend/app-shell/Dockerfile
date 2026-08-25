# Multi-stage build for Angular 22 App Shell
FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies
COPY package.json package-lock.json .npmrc ./
RUN npm config set fetch-retries 5 \
    && npm config set fetch-retry-mintimeout 20000 \
    && npm config set fetch-retry-maxtimeout 120000 \
    && npm ci --legacy-peer-deps --maxsockets 5 --fetch-timeout=600000

# Copy source and build
COPY . .
RUN npm run build --configuration=production

# Final image using Nginx
FROM nginx:alpine
# Copy nginx configuration if needed
# COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy build artifacts to nginx public folder
COPY --from=build /app/dist/app-shell/browser /usr/share/nginx/html

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
