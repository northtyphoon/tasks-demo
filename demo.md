# ACR Tasks

## 1. Set default registry

```sh
az configure --default acr=bindudemo
```

## 2. Build the container image which contains run a dotnet core app to create blobs on the target storage account

```sh
az acr build ^
  -t createblobwithmi:latest ^
  https://github.com/northtyphoon/tasks-demo.git#master:storage/CreateBlobWithMI
```

## 3. Create the build task with commit trigger enabled

```sh
az acr task create ^
  -c https://github.com/northtyphoon/tasks-demo.git#master:storage/CreateBlobWithMI ^
  -n buildcreateblobwithmi ^
  -f Dockerfile ^
  -t createblobwithmi:latest ^
  --git-access-token <PAT>
```

## 4. Create the context-less task with system assigned managed identity enabled

```sh
az acr task create ^
  -c /dev/null ^
  -n createblobwithmi ^
  -f acb.yaml ^
  --set ContainerUri=https://bindudemo.blob.core.windows.net/demo ^
  --assign-identity
```

## 5. Assign the managed identity to access the storage account

```sh
FOR /F "tokens=*" %g IN ('az acr task show --name createblobwithmi  --query identity.principalId --output tsv') do (SET principal=%g)

FOR /F "tokens=*" %g IN ('az storage account show -n bindudemo --query id --out tsv') do (SET storage=%g)

az role assignment create --role "Storage Blob Data Contributor" --assignee %principal% --scope %storage%
```

## 6. Manually schedule runs to test

```sh
az acr task run ^
  -n createblobwithmi
```

## 7. Schedule runs from *.aztask.io

```sh
curl -i -d "" ^
  -H "Content-Type: application/json" ^
  -H "registry-key: <ACR-ADMIN-PASSWORD>" ^
  -X POST https://bindudemo.aztask.io/v1/tasks/createblobwithmi/api/invoke
```

## 8. Enable the timer scheduler to schedule runs every 2 minutes

```sh
az acr task timer add ^
  -n createblobwithmi ^
  --timer-name every2minutes ^
  --schedule "*/2 * * * *"
```

## 9. List runs and show logs

```sh
az acr task list-runs -o table
az acr task logs --run-id <id>
```

## 10. Disable the timer scheduler

```sh
az acr task timer update ^
  -n createblobwithmi ^
  --timer-name every2minutes ^
  --enabled False
```

## 11. Pack build local node app without Dockerfile

```sh
az acr pack build ^
  -t my-build-pack-node-app:{{.Run.ID}} ^
  --pull ^
  --builder cloudfoundry/cnb:bionic ^
  .
```

```sh
az acr pack build ^
  -t my-build-pack-node-app:{{.Run.ID}} ^
  --pull ^
  --builder cloudfoundry/cnb:bionic ^
  https://github.com/northtyphoon/tasks-demo.git#master:buildpack/node
```

```sh
az acr pack build ^
  -t my-build-pack-java-app:{{.Run.ID}} ^
  --pull ^
  --builder cloudfoundry/cnb:bionic ^
  https://github.com/northtyphoon/tasks-demo.git#master:buildpack/java
```

## 12. Relay-Gateway

```sh
az acr run -f acb.yaml --values values.yaml --set RELAY_SAS_KEY_VALUE=<RELAY_SAS_KEY_VALUE> https://github.com/northtyphoon/tasks-demo.git#master:relay-gateway/app

curl -i -X GET https://bindudemo.servicebus.windows.net/app

az acr task cancel-run --run-id <ID>
```
