# Multi-stage build for mf-pricing
FROM node:24-alpine AS build
WORKDIR /app

# Install dependencies
COPY package.json package-lock.json ./
RUN npm config set fetch-retries 5 \
    && npm config set fetch-retry-mintimeout 20000 \
    && npm config set fetch-retry-maxtimeout 120000 \
    && npm ci --legacy-peer-deps --maxsockets 5 --fetch-timeout=600000

# Copy source and build
COPY . .
RUN --mount=type=cache,target=/app/.angular/cache npm run build --configuration=production

# Final image using Nginx
FROM nginx:alpine

# Copy nginx config for SPA routing and CORS if needed for MFE
RUN echo 'server { listen 80; location / { root /usr/share/nginx/html; index index.html; try_files $uri $uri/ /mf-pricing/index.html; add_header Access-Control-Allow-Origin *; } }' > /etc/nginx/conf.d/default.conf

# Copy build artifacts to nginx public folder
COPY --from=build /app/dist/mf-pricing/browser /usr/share/nginx/html/mf-pricing

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
