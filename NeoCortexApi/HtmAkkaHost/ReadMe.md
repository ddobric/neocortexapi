
### Start Node 1 locally
dotnet ./htmakkahost.dll --port  8081  --sysname HtmCluster --seedhosts="localhost:8081,localhost:8082" --hostname=localhost --publichostname=localhost

### Start Node 2 locally
dotnet ./htmakkahost.dll --port  8082  --sysname HtmCluster --seedhosts="localhost:8081,localhost:8082" --hostname=localhost --publichostname=localhost


### Start Node 1 locally NO SEEDS
dotnet ./htmakkahost.dll --port  8081  --sysname HtmCluster  --hostname=localhost --publichostname=localhost

### Start Node 2 locally
dotnet ./htmakkahost.dll --port  8082  --sysname HtmCluster  --hostname=localhost --publichostname=localhost


dotnet htmakkahost  --port  8081  --sysname HtmCluster --seedhosts="localhost:8081" --hostname=localhost --publichostname=localhost


-e port=8081  -e sysname=HtmCluster -e seedhosts="localhost:8081,localhost:8082" -e hostname=localhost -e publichostname=localhost

az container create --resource-group RG-COLLAB --name mycollabfunc --image damir.azurecr.io/collab:v1 --ip-address public --ports 80 --os-type linux --registry-login-server damir.azurecr.io --registry-username damir --registry-password "kjOvM=J/sEyYTBW6TFltwuV5qXkVH70" -e SbConnStr="Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ="

### Create ACI 
az container create -g  RG-HTM --name htm-node1 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node1 --environment-variables port=8081 sysname=HtmCluster hostname=0.0.0.0 publichostname=htm-node1.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70


az container create -g  RG-HTM --name htm-node2 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node2 --environment-variables port=8081 sysname=HtmCluster hostname=0.0.0.0 publichostname=htm-node2.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70
