
### Start Node 1 locally
dotnet ./htmakkahost.dll --port  8081  --sysname HtmCluster --seedhosts="localhost:8081,localhost:8082" --hostname=localhost --publichostname=localhost

### Start Node 2 locally
dotnet ./htmakkahost.dll --port  8082  --sysname HtmCluster --seedhosts="localhost:8081,localhost:8082" --hostname=localhost --publichostname=localhost


### Start Node 1 locally NO SEEDS
dotnet ./htmakkahost.dll --port  8081  --sysname HtmCluster  --hostname=localhost --publichostname=localhost

### Start Node 2 locally
dotnet ./htmakkahost.dll --port  8082  --sysname HtmCluster  --hostname=localhost --publichostname=localhost


dotnet htmakkahost  --port  8081  --sysname HtmCluster --seedhosts="localhost:8081" --hostname=localhost --publichostname=localhost


-e port=8081  -e sysname=HtmCluster -e hostname=0.0.0.0 -e publichostname=localhost

az container create --resource-group RG-COLLAB --name mycollabfunc --image damir.azurecr.io/collab:v1 --ip-address public --ports 80 --os-type linux --registry-login-server damir.azurecr.io --registry-username damir --registry-password "kjOvM=J/sEyYTBW6TFltwuV5qXkVH70" -e SbConnStr="Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ="

### Create ACI 
az container create -g  RG-HTM --name htm-node1 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node1 --environment-variables port=8081 sysname=HtmCluster hostname=0.0.0.0 publichostname=htm-node1.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70


az container create -g  RG-HTM --name htm-node2 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node2 --environment-variables port=8081 sysname=HtmCluster hostname=0.0.0.0 publichostname=htm-node2.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70




az container create -g  RG-HTM --name htm-node1 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node1 --environment-variables port=8081 sysname=HtmCluster hostname=htm-node1.westeurope.azurecontainer.io publichostname=htm-node1.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70


az container create -g  RG-HTM --name htm-node2 --image damir.azurecr.io/htmakkahost:v3 --ports 8081 --ip-address Public --cpu 4 --memory 16 --dns-name-label htm-node2 --environment-variables port=8081 sysname=HtmCluster hostname=htm-node2.westeurope.azurecontainer.io publichostname=htm-node2.westeurope.azurecontainer.io --registry-username damir --registry-password kkjOvM=J/sEyYTBW6TFltwuV5qXkVH70


## Create VM

#Enter name of the ResourceGroup in which you have the snapshots
$resourceGroupName ='RG-PHD-VM-SNAPSHOT'

#Enter name of the snapshot that will be used to create Managed Disks
$snapshotName = 'PHD-VM-DISC'

#Enter size of the disk in GB
$diskSize = '128'

#Enter the storage type for Disk. PremiumLRS / StandardLRS.
$storageType = 'StandardLRS'

#Enter the Azure region where Managed Disk will be created. It should be same as Snapshot location
$location = 'westeurope'

#Set the context to the subscription Id where Managed Disk will be created
Select-AzureRmSubscription -SubscriptionId '2d335f20-77b0-4a56-ac60-dc7018c9e5ee'

#Get the Snapshot ID by using the details provided
$snapshot = Get-AzureRmSnapshot -ResourceGroupName $resourceGroupName -SnapshotName $snapshotName 

#Create a new Managed Disk from the Snapshot provided 
$disk = New-AzureRmDiskConfig -AccountType $storageType -Location $location -CreateOption Copy -SourceResourceId $snapshot.Id

#Enter name of the Managed Disk
$diskName = '<Name of the Disk to be created>'

New-AzureRmDisk -Disk $disk -ResourceGroupName $resourceGroupName -DiskName $diskName

## Example Debug Args
--SystemName=node1 --RequestMsgQueue=actorsystem/actorqueue --ReplyMsgQueue=actorsystem/rcvnode1 --SbConnStr="Endpoint=sb://bastasample.servicebus.windows.net/;SharedAccessKeyName=demo;SharedAccessKey=MvwVbrrJdsMQyhO/0uwaB5mVbuXyvYa3WRNpalHi0LQ="