FROM mcr.microsoft.com/dotnet/aspnet:6.0

WORKDIR /app

EXPOSE ${PORT}

COPY ./bin/Release/net6.0/publish/ /app/

ENV ASPNETCORE_ENVIRONMENT="Development"
ENV ASPNETCORE_TCP_MAX_SESSIONS=10
ENV ASPNETCORE_BOARD_WIDTH=10
ENV ASPNETCORE_BOARD_HEIGHT=8

ENTRYPOINT ["dotnet", "Match3GameServer.dll"]

#COPY ./https/certificate.pfx /https/certificate.pfx

#RUN ls -l /https/certificate.pfx