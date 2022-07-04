#!/bin/bash

az aks delete --name $CLUSTER_NAME --resource-group $CLUSTER_NAME --yes

az group delete --name $CLUSTER_NAME --location uksouth

az group delete --name MC_$CLUSTER_NAME_$CLUSTER_NAME_uksouth
