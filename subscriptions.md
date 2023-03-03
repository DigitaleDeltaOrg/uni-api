# Subscriptions

Subscriptions is an optional module. It allows notifications to specific changes in data by means of the MQTT protocol.

https://github.com/node-red/designs/blob/master/designs/dynamic-mqtt-node.md

With subscriptions, the implementation acts as an MQTT broker. The implementation will regularly check the subscription list and perform the requested query. The implementation will then store the last time the query for the subscription was performed and the result was returned to the subscriber. Only changes reported after that timestamp will be returned at the next request.

The subscription will be in the form of an OData query.

The minimum subscription period will be 60 seconds.

All subscription requests must authorize via OAUTH2 Client Credential Flow.

TODO: Request format
The response format will be the same as the response format for the [query](query.md).
