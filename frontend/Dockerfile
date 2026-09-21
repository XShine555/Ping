# Build context: frontend/  (docker compose -f deploy/compose.yml --profile apps build)
FROM node:24-alpine AS build
WORKDIR /src

COPY package.json package-lock.json ./
RUN npm ci

COPY . .

# Vite inlines VITE_* variables at build time, so they have to be build args, not
# runtime environment variables — see deploy/compose.yml's web service.
ARG VITE_API_BASE_URL
ARG VITE_AUTH_AUTHORITY
ARG VITE_AUTH_CLIENT_ID
ARG VITE_AUTH_REDIRECT_URI
ARG VITE_LIVEKIT_URL
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL \
    VITE_AUTH_AUTHORITY=$VITE_AUTH_AUTHORITY \
    VITE_AUTH_CLIENT_ID=$VITE_AUTH_CLIENT_ID \
    VITE_AUTH_REDIRECT_URI=$VITE_AUTH_REDIRECT_URI \
    VITE_LIVEKIT_URL=$VITE_LIVEKIT_URL

RUN npm run build

FROM nginx:alpine AS runtime
COPY --from=build /src/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 3100
