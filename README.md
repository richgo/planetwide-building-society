#  Simple Building Society 

This project currently contains 4 microservices structured like:

```mermaid
  graph TD;
      A["Gateway"]
      B["Accounts Api"]
      C["Members Api"]
      D["Transactions Api"]
      A-->B;
      A-->C;
      A-->D;
```

## Why Graphql

* Data Fetching
  * No under or over fetching
* Schema & Type Safety
* Stitching allows us to integrate anything into the graphql

![Why Graphql](docs/images/requests.png "Why Graphql")

## To run
* Run `docker compose up` in the src folder

Some data will be randomly generated and stored in memory.

## Endpoints
* Heathchecks Ui http://localhost:9001/healthchecks-ui
  * This will check all endpoints & databases are available
* Federated Gateway schema explorer http://localhost:9001/graphql

## Docs
* [Queries](docs/Queries.md)
* [Mutations](docs/Mutations.md)
* [Subscriptions](docs/Subscriptions.md)
