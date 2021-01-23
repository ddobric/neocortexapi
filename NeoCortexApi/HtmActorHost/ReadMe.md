
## SbActors (dotnet actors with SB)

Start host

If the host should listen as 'node1' in the cluster.
~~~
--SystemName=node1 --RequestMsgQueue=actorsystem/actorqueue --ReplyMsgQueue=actorsystem/rcvnode1 --SbConnStr="***" --ActorSystemName=inst701 --SubscriptionName=node1
~~~

If multiple nodes listen on the same subscription and clients routes messages to the cluster without specifying the node.

~~~
--SystemName=node1 --RequestMsgTopic=actorsystem2/actortopic --RequestMsgQueue=actorsystem2/actorqueue --ReplyMsgQueue=actorsystem2/rcvlocal --ActorSystemName=systemzero --SubscriptionName=default
~~~

Note: If you want to omit the connection string, create an environment variable SbConnStr. The host will automatically read it if the connection string is not specified in the argument list.

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