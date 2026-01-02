# مرحلة البناء

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

#"TaskFlow.API/TaskFlow.API.csproj" is the folder where the csproj file is located
#"TaskFlow.API/" folder that will contain the csproj file in the container
COPY ["TaskFlow.API/TaskFlow.API.csproj", "TaskFlow.API/"]
COPY ["TaskFlow.Core/TaskFlow.Core.csproj", "TaskFlow.Core/"]
COPY ["TaskFlow.Infrastructure/TaskFlow.Infrastructure.csproj", "TaskFlow.Infrastructure/"]
COPY ["TaskFlow.Business/TaskFlow.Business.csproj", "TaskFlow.Business/"]
COPY ["TaskFlow.Domain/TaskFlow.Domain.csproj", "TaskFlow.Domain/"]
COPY ["TaskFlow.Shared/TaskFlow.Shared.csproj", "TaskFlow.Shared/"]
COPY ["TaskFlow.DependencyInjection/TaskFlow.DependencyInjection.csproj", "TaskFlow.DependencyInjection/"]
#result
#/app (داخل الكونتينر)
 #├── TaskFlow.API/          <-- (فولدر)
 #│    └── TaskFlow.API.csproj
 #├── TaskFlow.Core/         <-- (فولدر)
 #│    └── TaskFlow.Core.csproj
 #└── TaskFlow.Shared/       <-- (فولدر)
      #└── TaskFlow.Shared.csproj

# 2. نزل المكاتب (هيقدر يوصلهم دلوقتي لأننا نسخنا ملفاتهم)
RUN dotnet restore "TaskFlow.API/TaskFlow.API.csproj"

# 3. انسخ باقي الكود كله (كل الفولدرات)
COPY . .

# 4. ابنِ المشروع
WORKDIR "/app/TaskFlow.API"
RUN dotnet publish "TaskFlow.API.csproj" -c Release -o /app/out

# مرحلة التشغيل (زي ما هي)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "TaskFlow.API.dll"]