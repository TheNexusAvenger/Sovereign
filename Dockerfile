FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
ARG APP_NAME
ARG PORT

# Copy the application files and build them.
WORKDIR /build
COPY Bouncer/* Bouncer/
COPY Sovereign.Core/* Sovereign.Core/
COPY ${APP_NAME}/* ${APP_NAME}/
RUN dotnet build ${APP_NAME} -c release -r linux-musl-x64 --self-contained -o /publish

# Switch to a container for runtime.
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as runtime

# Prepare the runtime.
WORKDIR /app
COPY --from=build /publish .
RUN apk add wget icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
RUN ln -s ${APP_NAME}.dll app.dll
EXPOSE ${PORt}
ENTRYPOINT ["dotnet", "/app/app.dll"]